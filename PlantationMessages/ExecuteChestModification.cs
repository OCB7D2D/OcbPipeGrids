using System;
using System.Collections.Generic;

namespace NodeManager
{
    public class ExecuteChestModification : IActionClient
    {

        public int RecipientEntityId
        {
            get => -1; set => throw new
                System.Exception("Can send stop signal from clients!");
        }

        public Vector3i Position;

        public Dictionary<ushort, short> Changes
            = new Dictionary<ushort, short>();

        public void Setup(Vector3i position)
        {
            Position = position;
        }

        public void AddChange(ItemValue item, short count)
        {
            ushort type = (ushort)item.type;
            if (Changes.TryGetValue(type, out short have))
            {
                have += count;
                if (have == 0) Changes.Remove(type);
                else Changes[type] = have;
                Log.Out("Update change {0} to {1}",
                    item.type, have);
            }
            else if (count != 0)
            {
                Changes.Add(type, count);
                Log.Out("Set change {0} to {1}",
                    item.type, count);
            }
        }

        public static bool AddItemToChest(IInventory inv, ItemStack stack)
        {
            inv.TryStackItem(0, stack); // Stack what we can
            return stack.count > 0 && inv.AddItem(stack);
        }

        public void ProcessOnMainThread(NodeManagerMother mother)
        {
            Log.Out("Got chest modification for real {0}", Position);
            World world = GameManager.Instance.World;
            if (world.GetTileEntity(0, Position) is
                TileEntityLootContainer container)
            {
                Log.Out("Container is loaded, commit");
                if (container.IsUserAccessing()) return;
                ApplyChangesToChest(Changes, container);
                return;
            }
            // Changes could not be committed right away
            // Make sure structure is there (create if needed)
            if (!BlockComposter.ChestChanges.TryGetValue(Position,
                out Dictionary<ushort, short> changes))
            {
                BlockComposter.ChestChanges.Add(Position,
                    changes = new Dictionary<ushort, short>());
            }
            // Remember all changes that are sent to us
            MergePerndingChestChanges(ref changes, Changes);
        }

        private void MergePerndingChestChanges(
            ref Dictionary<ushort, short> to,
            Dictionary<ushort, short> from)
        {
            // Process all new changes (by type and count)
            foreach (KeyValuePair<ushort, short> change in from)
            {
                // Skip if nothing changes!?
                if (change.Value == 0) continue;
                // Check if we already know this type
                // Note: count should not be zero ever!
                if (to.TryGetValue(change.Key, out short count))
                {
                    // Check if new calculation would make count zero
                    if (count + change.Value == 0) to.Remove(change.Key);
                    // Otherwise update the count for the change by type
                    else to[change.Key] = (short)(count + change.Value);
                }
                // Or create new entry in the dictionary
                else to.Add(change.Key, change.Value);
            }
        }

        static readonly SortedSet<ushort>
            Removals = new SortedSet<ushort>();

        public static void ApplyChangesToChest(
            Dictionary<ushort, short> changes,
            TileEntityLootContainer container)
        {
            foreach (var kv in changes)
            {
                ItemValue iv = new ItemValue(kv.Key);
                ItemStack change = new ItemStack(iv, kv.Value);
                Log.Out("Commit change {0} {1}", iv.type, change.count);
                if (change.count < 0)
                {
                    Log.Out("Removing some item {0} x {1}", iv, change.count);
                    var has = container.GetItems();
                    for (var i = 0; i < has.Length; i += 1)
                    {
                        // Break when fulfilled
                        if (change.count == 0) break;
                        // Skip all empty slots
                        if (has[i].IsEmpty()) continue;
                        // Check that types are the same
                        if (has[i].itemValue.type !=
                            change.itemValue.type) continue;
                        // Slot has enough to fulfill it
                        if (has[i].count >= -change.count)
                        {
                            has[i].count += change.count;
                            change.count = 0; // had enough
                            if (has[i].count != 0) continue;
                            has[i].itemValue = ItemValue.None;
                        }
                        // Has only partially
                        else
                        {
                            change.count -= has[i].count; // Some
                            has[i].itemValue = ItemValue.None;
                            has[i].count = 0; // Removed all
                        }
                    }
                }
                if (change.count > 0)
                {
                    Log.Out("Adding some item {0} => {1}", iv, change.count);
                    AddItemToChest(container, change);
                }
                // container.UpdateSlot(i, has[i]);
                if (change.count == 0) Removals.Add(kv.Key);
                else changes[kv.Key] = (short)change.count;
            }
            Log.Out("Can commit right away");
            // Remove marked key after loop
            foreach (var key in Removals)
                changes.Remove(key);
            // Clear list for later
            Removals.Clear();
        }
    }
}
