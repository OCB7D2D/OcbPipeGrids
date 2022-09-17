using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{

    //################################################################
    //################################################################

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        //********************************************************
        // Data structure holding pending block changes by chunk
        //********************************************************

        public static readonly Dictionary<Vector3i, List<BlockChangeInfo>>
            PendingChunkChanges = new Dictionary<Vector3i, List<BlockChangeInfo>>(512);

        //********************************************************
        // Load pending chunk changes from disk
        //********************************************************

        private static void ReadPendingChunkChanges(BinaryReader br)
        {
            int count = br.ReadInt32();
            PendingChunkChanges.Clear();
            for (int i = 0; i < count; i += 1)
            {
                var key = StreamUtils.ReadVector3i(br);
                var changes = br.ReadInt32();
                var list = new List<BlockChangeInfo>();
                PendingChunkChanges.Add(key, list);
                for (int n = 0; n < changes; n += 1)
                {
                    var change = new BlockChangeInfo();
                    change.Read(br); list.Add(change);
                }

            }
        }

        //********************************************************
        // Persist pending chunk changes to disk
        //********************************************************

        private static void WritePendingChunkChanges(BinaryWriter bw)
        {
            bw.Write(PendingChunkChanges.Count);
            foreach (var changes in PendingChunkChanges)
            {
                StreamUtils.Write(bw, changes.Key);
                bw.Write(changes.Value.Count);
                foreach (var change in changes.Value)
                    change.Write(bw);
            }
        }

        //********************************************************
        // Call this function if you want to update any block
        // Will either dispatch right away if chunk is loaded
        // Otherwise it will be registered to apply later
        //********************************************************

        public static void ExecuteBlockChange(BlockChangeInfo change)
        {
            var world = GameManager.Instance.World;
            // Check if block (its chunk) is loaded
            if (BlockHelper.IsLoaded(world, change.pos))
            {
                // If so we can directly add the changes
                world.SetBlockRPC(change.pos, change.blockValue);
            }
            else
            {
                // Otherwise we need to wait until chunk is loaded
                Vector3i chunkpos = World.toChunkXYZ(change.pos);
                if (PendingChunkChanges.TryGetValue(chunkpos,
                    out List<BlockChangeInfo> changes))
                {
                    Log.Warning("Overwrite pending late changes! {0}", change.clrIdx);
                    changes.RemoveAll(item => item.pos == change.pos);
                    changes.Add(change); // ToDo: change to dictionary?
                }
                else
                {
                    Log.Warning("Added to pending late changes! {0}", change.clrIdx);
                    changes = new List<BlockChangeInfo> { change };
                    PendingChunkChanges.Add(chunkpos, changes);
                }
            }
        }

        //********************************************************
        // Helper to add late block changes in the next update loop 
        //********************************************************

        private static IEnumerator UpdateBlockLater(List<BlockChangeInfo> changes)
        {
            yield return new WaitForSeconds(0f);
            if (GameManager.Instance.World is World world)
                world.SetBlocksRPC(changes);
            // Add some `yield return null`?
        }

        //********************************************************
        // Invoked on `Chunk.recalcIndexedBlocks`
        //********************************************************

        public static void ApplyPendingChangesToChunk(Chunk chunk)
        {
            if (!NodeManagerInterface.HasServer) return;
            // All this shenanigan to just postpone `adds` a little
            // If we call this synced we get weird behavior and not
            // sure if there is any other hook that would work ...
            if (PendingChunkChanges.TryGetValue(chunk.ChunkPos,
                out List<BlockChangeInfo> changes))
            {
                if (changes.Count > 0) GameManager.Instance
                    .StartCoroutine(UpdateBlockLater(changes));
                PendingChunkChanges.Remove(chunk.ChunkPos);
            }
        }

        //********************************************************
        // Harmony Hook into `Chunk.recalcIndexedBlocks`
        //********************************************************

        [HarmonyPatch(typeof(Chunk))]
        [HarmonyPatch("recalcIndexedBlocks")]
        public class PatchLateBlockChanges
        {
            private static void Prefix(Chunk __instance)
                => ApplyPendingChangesToChunk(__instance);
        }

        //********************************************************
        //********************************************************

    }

    //################################################################
    //################################################################

}
