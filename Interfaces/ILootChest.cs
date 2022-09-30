public interface ILootChest
{
    Vector3i WorldPos { get; }
    // byte CurrentSunLight { get; set; }
    ItemStack[] GetItems();
}
