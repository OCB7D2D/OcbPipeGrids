using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NodeManager
{
    class BlockHelper
    {

        public static bool IsLoaded(WorldBase world, Vector3i position)
        {
            return world.GetChunkFromWorldPos(position) != null;
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

        internal static bool IsInReach(Vector3i a, Vector3i b, int dist)
            => Math.Abs(a.x - b.x) <= dist
            || Math.Abs(a.y - b.y) <= dist
            || Math.Abs(a.z - b.z) <= dist;

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
            Log.Out("=== RemoveScheduledBlockUpdate");
            var wbt = GameManager.Instance.World.GetWBT();
            lock (LockObject.Get(wbt))
            {
                var scheduledTicksDict = ScheduledTicksDict.Get(wbt);
                if (scheduledTicksDict.TryGetValue(
                    ToHashCode(clrIdx, pos),
                    out WorldBlockTickerEntry wbte))
                {
                    Log.Out("Remove Dict {0}", scheduledTicksDict.Remove(wbte.GetHashCode()));
                    var chunkToScheduledTicks = ChunkToScheduledTicks.Get(wbt);
                    Log.Out("Remove {0}", chunkToScheduledTicks[wbte.GetChunkKey()]?.Remove(wbte));
                    var scheduledTicksSorted = ScheduledTicksSorted.Get(wbt);
                    Log.Out("Count before {0}", scheduledTicksSorted.Count);
                    scheduledTicksSorted.Remove(wbte);
                    Log.Out("Count after {0} vs {1}", scheduledTicksSorted.Count,
                        scheduledTicksDict.Count);
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
            BlockValue bv, Block block, Color color, int reach)
        {
            if (bv.ischild || bv.isair) return;
            var helper = LandClaimBoundsHelper.GetBoundsHelper(pos);
            foreach (Renderer componentsInChild in helper.GetComponentsInChildren<Renderer>())
                componentsInChild.material.SetColor("_Color", color);
            Vector3 dim = block.multiBlockPos.dim.ToVector3();
            // float reach_x = (block.multiBlockPos.dim.x);
            // float reach_y = (block.multiBlockPos.dim.y);
            // float reach_z = (block.multiBlockPos.dim.z);
            helper.localScale = (dim + Vector3.one * reach * 2) * 2.54f;

            Vector3 halfs = new Vector3(
                dim.x % 2 == 0 ? 0.0f : 0.5f,
                dim.y % 2 == 0 ? 0.0f : 0.5f,
                dim.z % 2 == 0 ? 0.0f : 0.5f);


            Vector3 offset = new Vector3(
                dim.x % 2 == 0 ? 0.0f : 0.5f,
                dim.y / 2f, // Always at bottom
                dim.z % 2 == 0 ? 0.0f : 0.5f);

            helper.localPosition = pos - Origin.position + offset;
            helper.gameObject.SetActive((bv.meta2 & 1) != 0);
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
