using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public Dictionary<Vector3i, ItemStack[]> Chests
            = new Dictionary<Vector3i, ItemStack[]>();

        public ItemStack[] GetChest(Vector3i pos)
        {
            return Chests.TryGetValue(pos, out ItemStack[] rv) ? rv : null;
        }

        public bool AddLootChest(ILootChest loot)
        {
            Log.Warning("Called add loot chest at pos");
            // Check if key is already known to use (and skip?)
            if (Chests.ContainsKey(loot.WorldPos)) return false;
            Log.Warning("Add loot chest at pos");
            Log.Warning("Add loot chest at pos");
            Log.Warning("Add loot chest at pos");
            Log.Warning("Add loot chest at pos");
            Log.Warning("Add loot chest at pos");
            // GetItems will already return a clone
            Chests.Add(loot.WorldPos, loot.GetItems());
            // Added an item
            return true;
        }

        public bool RemoveLootChest(ILootChest loot)
        {
            return Chests.Remove(loot.WorldPos);
        }

        public bool AddChest(Vector3i pos, ItemStack[] stack)
        {
            Chests[pos] = stack;
            return true;
        }

        public bool UpdateChest(Vector3i pos, ItemStack[] stack)
        {
            Chests[pos] = stack;
            return true;
        }

        public bool RemoveChest(Vector3i pos)
        {
            return Chests.Remove(pos);
        }

        private void ReadChests(BinaryReader br)
        {
            return;
            int nodes = br.ReadInt32();
            for (int i = 0; i < nodes; i += 1)
            {
                Vector3i pos = new Vector3i(
                    br.ReadInt32(),
                    br.ReadInt32(),
                    br.ReadInt32());
                short items = br.ReadInt16();
                var stack = new ItemStack[items];
                for (int n = 0; n < items; ++n)
                    stack[n].Read(br);
                Chests.Add(pos, stack);
            }

        }
        private void WriteChests(BinaryWriter bw)
        {
            return;
            bw.Write(Chests.Count);
            foreach (var chest in Chests)
            {
                bw.Write(chest.Key.x);
                bw.Write(chest.Key.y);
                bw.Write(chest.Key.z);
                bw.Write((short)chest.Value.Length);
                foreach (ItemStack itemStack in chest.Value)
                    itemStack.Clone().Write(bw);
            }
        }

    }
}
