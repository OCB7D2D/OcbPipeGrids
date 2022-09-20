using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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

    }
}
