namespace NodeFacilitator
{
    public interface ILootChest : IWorldPos
    {
        // byte CurrentSunLight { get; set; }
        ItemStack[] GetItems();
    }
}
