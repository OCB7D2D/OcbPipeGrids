using System.IO;

namespace NodeManager
{
    public abstract class LootBlock<B> : NodeBlock<B>, ILootChest where B : class, ILootBlock
    {

        // We remember the items of each loot block (chest)
        // In order for tickers etc. to know what is in there
        // Otherwise only loaded chests would be available
        // Master thread will update us when needed
        // We inform master thread of any changes
        private readonly ItemStack[] Items;

        protected LootBlock(Vector3i position, BlockValue bv) : base(position, bv)
        {
            Items = new ItemStack[0];
        }

        protected LootBlock(BinaryReader br) : base(br)
        {
            // Read the ItemStack array
            Items = GameUtils.ReadItemStack(br);
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Write the ItemStack array
            GameUtils.WriteItemStack(bw, Items);
        }

        // Always returns a clone?
        public ItemStack[] GetItems()
            => ItemStack.Clone(Items);

        protected override void OnManagerAttached(NodeManager manager)
        {
            Log.Out("Manager is beeing attached to loot");
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveLootChest(this);
            manager?.AddLootChest(this);
            //manager?.AddComposter(this);
        }

    }

}