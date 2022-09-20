using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeManager
{

	// Hook into the middle of `gmUpdate`
	[HarmonyPatch(typeof(Chunk))]
	[HarmonyPatch("recalcIndexedBlocks")]
	public class LateBlockChanges
	{

		public static Dictionary<Vector3i, List<BlockChangeInfo>> PendingChunkChanges
			= new Dictionary<Vector3i, List<BlockChangeInfo>>(512);

		public static void AddPendingBlockChange(BlockChangeInfo change)
        {
			var world = GameManager.Instance.World;
			if (BlockHelper.IsLoaded(world, change.pos))
			{
				Log.Warning("directly replace block value");
				world.SetBlockRPC(change.pos, change.blockValue);
			}
			else
			{
				Vector3i chunkpos = World.toChunkXYZ(change.pos);
				if (PendingChunkChanges.TryGetValue(chunkpos,
					out List<BlockChangeInfo> changes))
				{
					Log.Warning("Re-Marked for pending change later! {0}", change.clrIdx);
					changes.RemoveAll(item => item.pos == change.pos);
					changes.Add(change); // ToDo: change to dictionary?
				}
				else
				{
					Log.Warning("Marked for pending change later! {0}", change.clrIdx);
					changes = new List<BlockChangeInfo> { change };
					PendingChunkChanges.Add(chunkpos, changes);
				}
			}
		}

		private static IEnumerator UpdateBlockLater(List<BlockChangeInfo> changes)
		{
			yield return new WaitForSeconds(0f);
			if (GameManager.Instance.World is World world)
				world.SetBlocksRPC(changes);
		}

		private static void Prefix(Chunk __instance)
		{
			if (!NodeManagerInterface.HasServer) return;
			// All this shenanigan to just postpone adding a little
			// If we call this synced we get weird behavior and not
			// sure if there is any other hook that would work ...
			if (PendingChunkChanges.TryGetValue(__instance.ChunkPos,
				out List<BlockChangeInfo> changes))
            {
				if (changes.Count > 0) GameManager.Instance
					.StartCoroutine(UpdateBlockLater(changes));
				PendingChunkChanges.Remove(__instance.ChunkPos);
			}
		}
	}
	
}
