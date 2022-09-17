using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections;
using System.IO;

namespace NodeFacilitator
{

    //########################################################
    //########################################################

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Provide direct world position to lock state
        public static readonly ConcurrentDictionary<Vector3i, int>
            TileEntityLocks = new ConcurrentDictionary<Vector3i, int>();

        // Query if the TileEntity at position is locked
        // Is guaranteed to be safe to call on any thread
        public static bool IsTileEntityLocked(Vector3i position)
            => TileEntityLocks.ContainsKey(position);

    }

    //########################################################
    //########################################################

    [HarmonyPatch]
    public class PatchLockAdded
    {

        static void LockTileEntity(Dictionary<TileEntity, int> locks, TileEntity te, int value)
        {
            // Also store at our own dictionary, but by position
            NodeManager.TileEntityLocks[te.ToWorldPos()] = value;
            // Implement original line
            locks[te] = value;
        }

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GameManager), "TELockServer");
        }

        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Callvirt) continue;
                if (!(codes[i].operand is MethodInfo add)) continue;
                if (add.Name != "set_Item") continue;
                codes[i] = CodeInstruction.Call(typeof(PatchLockAdded), "LockTileEntity");
                Log.Out("  transpiler patched LockTileEntity");
            }
            return codes;
        }

    }

    //########################################################
    //########################################################

    [HarmonyPatch]
    public class PatchLockRemoved
    {

        static bool UnlockTileEntity(Dictionary<TileEntity, int> locks, TileEntity te)
        {
            // Lock for thread-safe access
            lock (NodeManager.TileEntityLocks)
            {
                // Also stored at our own dictionary, but by position
                ((IDictionary)NodeManager.TileEntityLocks).Remove(te.ToWorldPos());
            }
            // Implement original line
            return locks.Remove(te);
        }

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GameManager), "ClearTileEntityLockForClient");
            yield return AccessTools.Method(typeof(GameManager), "TEUnlockServer");
            yield return AccessTools.Method(typeof(GameManager), "ChangeBlocks");
        }

        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Callvirt) continue;
                if (!(codes[i].operand is MethodInfo remove)) continue;
                if (remove.Name != "Remove") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Pop) continue;
                codes[i - 1] = CodeInstruction.Call(typeof(PatchLockRemoved), "UnlockTileEntity");
                Log.Out("  transpiler patched UnlockTileEntity");
            }
            return codes;
        }

    }

    //########################################################
    //########################################################

}
