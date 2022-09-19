using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{

	[HarmonyPatch(typeof(WorldBlockTicker))]
	[HarmonyPatch("tickScheduled")]
	public class tickScheduled
	{
		static bool Prefix(
			WorldBlockTicker __instance, GameRandom _rnd,
			object ___lockObject, World ___world,
			SortedList ___scheduledTicksSorted,
			Dictionary<int, WorldBlockTickerEntry> ___scheduledTicksDict,
			DictionarySave<long, HashSet<WorldBlockTickerEntry>> ___chunkToScheduledTicks,
			ref bool __result)
		{
			int num;
			lock (___lockObject)
			{
				num = ___scheduledTicksSorted.Count;
				if (num != ___scheduledTicksDict.Count)
					throw new Exception("Err");
			}
			if (num > 100)
				num = 100;
			for (int index = 0; index < num; ++index)
			{
				WorldBlockTickerEntry key;
				lock (___lockObject)
				{
					key = (WorldBlockTickerEntry)___scheduledTicksSorted.GetKey(0);
					if (key.scheduledTime <= GameTimer.Instance.ticks)
					{
						___scheduledTicksSorted.Remove((object)key);
						___scheduledTicksDict.Remove(key.GetHashCode());
						___chunkToScheduledTicks[key.GetChunkKey()]?.Remove(key);
					}
					else break;
				}

				if (___world.IsChunkAreaLoaded(
					key.worldPos.x, key.worldPos.y, key.worldPos.z))
				{
					BlockValue block = ___world.GetBlock(
						key.clrIdx, key.worldPos);
					if (block.type != key.blockID) continue;
					block.Block.UpdateTick(___world, key.clrIdx,
						key.worldPos, block, false, 0UL, _rnd);
				}
				// This is our patch that we add here
				else
				{
					// If area is not loaded yet, but the item itself is
					// Re-schedule the tick for later to check area again
					int chunkX = World.toChunkXZ(key.worldPos.x);
					int chunkZ = World.toChunkXZ(key.worldPos.z);
					if (___world.GetChunkSync(chunkX, chunkZ) != null)
						__instance.AddScheduledBlockUpdate(
							key.clrIdx, key.worldPos, key.blockID, 10);
				}
			}
			__result = ___scheduledTicksSorted.Count > 0;
			return false;
		}
	}

}
