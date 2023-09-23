﻿using Audio;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NodeFacilitator
{

    static partial class BlockHelper
    {

        public static bool IsLoaded(WorldBase world, Vector3i position)
        {
            return world.GetChunkFromWorldPos(position) != null;
        }
        public static Vector2 ParseVector2(DynamicProperties properties, string name, Vector2 def)
        {
            return properties.Contains(name) ? StringParsers
                .ParseVector2(properties.GetString(name)) : def;
        }

        public static Vector3 ParseVector3(DynamicProperties properties, string name, Vector3 def)
        {
            return properties.Contains(name) ? StringParsers
                .ParseVector3(properties.GetString(name)) : def;
        }

        public static string ParseString(DynamicProperties properties, string name, string def = "")
            => properties.Contains(name) ? properties.GetString(name) : def;

        public static float ParseFloat(DynamicProperties properties, string name, float def, float divide = 1)
        {
            properties.ParseFloat(name, ref def);
            return def / divide;
        }

        public static bool IsLoaded(Vector3i position)
        {
            if (GameManager.Instance == null) return false;
            if (GameManager.Instance.World == null) return false;
            WorldBase world = GameManager.Instance.World;
            return IsLoaded(world, position);
        }

        static readonly HarmonyFieldProxy<object>
            LockObject = new HarmonyFieldProxy<object>(
                typeof(WorldBlockTicker), "lockObject");

        static readonly HarmonyFieldProxy<SortedList>
            ScheduledTicksSorted = new HarmonyFieldProxy<SortedList>(
                typeof(WorldBlockTicker), "scheduledTicksSorted");


        //public static bool IsInReach<T>(Vector3i center, Tuple<Vector3i, T> kv) where T : IReacher
        //    => IsInReach(center, kv.Item1, kv.Item2.RBLK.BlockReach);
        //
        //public static bool IsInReach(Vector3i center, Vector3i target, Vector3i dist)
        //    => Math.Abs(center.x - target.x) <= dist.x
        //    || Math.Abs(center.y - target.y) <= dist.y
        //    || Math.Abs(center.z - target.z) <= dist.z;

        static readonly HarmonyFieldProxy<Dictionary<int, WorldBlockTickerEntry>>
            ScheduledTicksDict = new HarmonyFieldProxy<Dictionary<int, WorldBlockTickerEntry>>(
                typeof(WorldBlockTicker), "scheduledTicksDict");

        static readonly HarmonyFieldProxy<DictionarySave<long, HashSet<WorldBlockTickerEntry>>>
            ChunkToScheduledTicks = new HarmonyFieldProxy<DictionarySave<long, HashSet<WorldBlockTickerEntry>>>(
                typeof(WorldBlockTicker), "chunkToScheduledTicks");

        static readonly MethodInfo MethodAddWbte = AccessTools
            .Method(typeof(WorldBlockTicker), "add");

        private static int ToHashCode(int _clrIdx, Vector3i _pos)
            => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

        public static void RemoveScheduledBlockUpdate(int clrIdx, Vector3i pos)
        {
            // Log.Out("=== RemoveScheduledBlockUpdate");
            var wbt = GameManager.Instance.World.GetWBT();
            lock (LockObject.Get(wbt))
            {
                var scheduledTicksDict = ScheduledTicksDict.Get(wbt);
                if (scheduledTicksDict.TryGetValue(
                    ToHashCode(clrIdx, pos),
                    out WorldBlockTickerEntry wbte))
                {
                    scheduledTicksDict.Remove(wbte.GetHashCode());
                    var chunkToScheduledTicks = ChunkToScheduledTicks.Get(wbt);
                    chunkToScheduledTicks[wbte.GetChunkKey()]?.Remove(wbte);
                    var scheduledTicksSorted = ScheduledTicksSorted.Get(wbt);
                    // Log.Out("Count before {0}", scheduledTicksSorted.Count);
                    scheduledTicksSorted.Remove(wbte);
                    //Log.Out("Count after {0} vs {1}", scheduledTicksSorted.Count,
                    //    scheduledTicksDict.Count);
                }
            }
        }

        public static void SetScheduledBlockUpdate(
            int clrIdx, Vector3i pos, int blockId, ulong ticks)
        {
            if (blockId == 0) return;
            var wbt = GameManager.Instance.World.GetWBT();
            RemoveScheduledBlockUpdate(clrIdx, pos);
            lock (LockObject.Get(wbt))
            {
                WorldBlockTickerEntry wbte = new WorldBlockTickerEntry(
                    clrIdx, pos, blockId, ticks + GameTimer.Instance.ticks);
                MethodAddWbte.Invoke(wbt, new object[] { wbte });
            }
        }

        //########################################################
        // Helpers for block activation commands
        //########################################################
        public static int ExtendActivationCommands(
            Block current, System.Type ptype,
            ref BlockActivationCommand[] cmds,
            string name = "cmds")
        {
            var field = AccessTools.Field(ptype, name);
            if (field.GetValue(current) is
                BlockActivationCommand[] parent)
            {
                int size = cmds.Length;
                int offset = parent.Length;
                Array.Resize(ref cmds, size + offset);
                // Move existing to the back
                for (int i = 0; i < size; i += 1)
                    cmds[offset + i] = cmds[i];
                // Insert new on the front
                for (int i = 0; i < offset; i += 1)
                    cmds[i] = parent[i];
                return offset;
            }
            return 0;
        }

        public static void ExtendActivationCommands(
            Block current, System.Type ptype,
            ref BlockActivationCommand[] cmds,
            ref int offset, string name = "cmds")
        {
            var field = AccessTools.Field(ptype, name);
            if (field.GetValue(current) is
                BlockActivationCommand[] parent)
            {
                int size = cmds.Length; offset = parent.Length;
                System.Array.Resize(ref cmds, size + offset);
                // Move existing to the back
                for (int i = 0; i < size; i += 1)
                    cmds[offset + i] = cmds[i];
                // Insert new on the front
                for (int i = 0; i < offset; i += 1)
                    cmds[i] = parent[i];
            }
        }

        internal static void UpdateBoundHelper(Vector3i pos,
            BlockValue bv, Block block, IReacherBlock reacher)
        {
            if (bv.ischild || bv.isair) return;
            Transform helper = BoundsHelper.GetBoundsHelper(pos, false);
            Transform helper2 = BoundsHelper.GetBoundsHelper(pos, true);

            foreach (Renderer componentsInChild in helper.GetComponentsInChildren<Renderer>())
                componentsInChild.material.SetColor("_Color", reacher.BoundHelperColor * 0.5f);
            foreach (Renderer componentsInChild in helper2.GetComponentsInChildren<Renderer>())
                componentsInChild.material.SetColor("_Color", reacher.ReachHelperColor * 0.5f);

            Vector3i dim = block.multiBlockPos != null ? block.multiBlockPos.dim : Vector3i.one;

            Log.Warning("!!!!!!!!!!????????????? ROtating well for rotation {0}", bv.meta2);
            Vector3i rotated = FullRotation.Rotate(bv.rotation, dim);
            Vector3i reach = FullRotation.Rotate(bv.rotation, FullRotation.GetReach(bv.rotation, reacher.Reach));
            Vector3i offsets = FullRotation.Rotate(bv.rotation, FullRotation.GetOffset(bv.rotation, reacher.Reach));

            // float reach_x = (block.multiBlockPos.dim.x);
            // float reach_y = (block.multiBlockPos.dim.y);
            // float reach_z = (block.multiBlockPos.dim.z);
            // helper.localScale = (dim + Vector3.one * reach * 2) * 2.54f;

            Vector3 shift = new Vector3(
                rotated.x % 2 == 0 ? rotated.x < 0 ? 1f : 0.0f : 0.5f,
                rotated.y % 2 == 0 ? rotated.y < 0 ? 1f : 0.0f : 0.5f,
                rotated.z % 2 == 0 ? rotated.z < 0 ? 1f : 0.0f : 0.5f);


            // Origin.Add(helper.transform, -1);
            // Origin.Add(helper2.transform, -1);


            helper.localScale = (rotated.ToVector3()) * 2.54f;
            helper2.localScale = helper.localScale + reach * 5.08f;

            helper.localPosition = pos.ToVector3() - Origin.position + shift;
            helper2.localPosition = helper.localPosition + offsets;

            // Log.Out("DIM {0} => {1} at {2} + {3}", dim, rotated, pos, shift);


            helper.gameObject.SetActive((bv.meta2 & 1) == 1);
            helper2.gameObject.SetActive((bv.meta2 & 1) != 0);
        }

        //########################################################
        // Helpers for generic states
        //########################################################

        public static bool GetEnabled(BlockValue bv)
        {
            return (bv.meta & 0b_0000_0010) > 0;
        }
        public static bool GetEnabled2(BlockValue bv)
        {
            return (bv.meta & 0b_0000_0001) > 0;
        }

        public static void SetEnabled(ref BlockValue bv, bool enabled)
        {
            bv.meta &= 0b_1111_1101;
            if (enabled == false) return;
            bv.meta |= 0b_0000_0010;
        }

        public static void SetEnabled2(ref BlockValue bv, bool enabled)
        {
            bv.meta &= 0b_1111_1110;
            if (enabled == false) return;
            bv.meta |= 0b_0000_0001;
        }

        //########################################################
        // Helpers for plant states
        //########################################################

        public static int GetIllness(BlockValue bv)
        {
            return bv.meta2 & 0b_0000_0111;
        }

        public static void SetIllness(ref BlockValue bv, int state)
        {
            if (state < 0) state = 0;
            if (state > 7) state = 7;
            bv.meta2 = (byte)(
                (byte)(state & 0b_0000_0111) +
                (byte)(bv.meta2 & ~0b_0000_0111));
            Log.Out("Set meta2 to {0}", bv.meta2);
        }

    }
}