using HarmonyLib;
using System.Collections;
using System.Collections.Generic;

namespace NodeManager
{

	[HarmonyPatch(typeof(WorldBlockTicker))]
	[HarmonyPatch("tickScheduled")]
	public class tickScheduled
	{

		static readonly uint RecheckTicks = 30;
		static readonly int MaxTickersPerTick = 100;

		static bool Prefix(
			WorldBlockTicker __instance, GameRandom _rnd,
			object ___lockObject, World ___world,
			SortedList ___scheduledTicksSorted,
			Dictionary<int, WorldBlockTickerEntry> ___scheduledTicksDict,
			DictionarySave<long, HashSet<WorldBlockTickerEntry>> ___chunkToScheduledTicks,
			ref bool __result)
		{
			int amount;
			lock (___lockObject)
			{
				amount = ___scheduledTicksSorted.Count;
				if (amount != ___scheduledTicksDict.Count)
					throw new System.Exception("Invalid State");
			}
			if (amount > MaxTickersPerTick) amount = MaxTickersPerTick;
			for (int index = 0; index < amount; ++index)
			{
				WorldBlockTickerEntry key;
				lock (___lockObject)
				{
					key = (WorldBlockTickerEntry)___scheduledTicksSorted.GetKey(0);
					if (key.scheduledTime <= GameTimer.Instance.ticks)
					{
						___scheduledTicksSorted.Remove(key);
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
							key.clrIdx, key.worldPos, key.blockID,
							// Add some randomness to avoid all ticks
							// to be fired at the same tick for chunk
							RecheckTicks + (uint)_rnd.RandomRange(0, 15));
				}
			}
			__result = ___scheduledTicksSorted.Count > 0;
			return false;
		}
	}

}
