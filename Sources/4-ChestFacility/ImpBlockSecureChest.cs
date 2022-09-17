using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlockPlacement;

public interface IBlockSecureChest
{

    string lootList { get; set; }

    float lockPickTime { get; set; }

    string lockPickItem { get; set; }

    float lockPickBreakChance { get; set; }

}

public static class ImpBlockSecureChest
{

    static string PropLootList = "LootList";

    // static string PropLootStageMod = "LootStageMod";
    
    // static string PropLootStageBonus = "LootStageBonus";

    static string PropLockPickTime = "LockPickTime";

    static string PropLockPickItem = "LockPickItem";

    static string PropLockPickBreakChance = "LockPickBreakChance";

    public static void Init<T>(T chest) where T : Block, IBlockSecureChest
    {

        var Properties = chest.Properties;

        if (!Properties.Values.ContainsKey(PropLootList))
        {
            throw new Exception("Block with name " + chest.GetBlockName() + " doesnt have a loot list");
        }

        chest.lootList = Properties.Values[PropLootList];
        if (Properties.Values.ContainsKey(PropLockPickTime))
        {
            chest.lockPickTime = StringParsers.ParseFloat(Properties.Values[PropLockPickTime]);
        }
        else
        {
            chest.lockPickTime = 15f;
        }

        if (Properties.Values.ContainsKey(PropLockPickItem))
        {
            chest.lockPickItem = Properties.Values[PropLockPickItem];
        }

        if (Properties.Values.ContainsKey(PropLockPickBreakChance))
        {
            chest.lockPickBreakChance = StringParsers.ParseFloat(Properties.Values[PropLockPickBreakChance]);
        }
        else
        {
            chest.lockPickBreakChance = 0f;
        }
    }

    public static void PlaceBlock(WorldBase world, BlockPlacement.Result result, EntityAlive ea)
    {
        TileEntitySecureLootContainer tileEntitySecureLootContainer = world.GetTileEntity(result.clrIdx, result.blockPos) as TileEntitySecureLootContainer;
        if (tileEntitySecureLootContainer != null)
        {
            tileEntitySecureLootContainer.SetEmpty();
            if (ea != null && ea.entityType == EntityType.Player)
            {
                tileEntitySecureLootContainer.bPlayerStorage = true;
                tileEntitySecureLootContainer.SetOwner(PlatformManager.InternalLocalUserIdentifier);
            }
        }
    }

    public static void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, string lootList)
    {

        if (!_blockValue.ischild && !(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureLootContainer))
        {
            TileEntitySecureLootContainer tileEntitySecureLootContainer = new TileEntitySecureLootContainer(_chunk);
            tileEntitySecureLootContainer.localChunkPos = World.toBlock(_blockPos);
            tileEntitySecureLootContainer.lootListName = lootList;
            tileEntitySecureLootContainer.SetContainerSize(LootContainer.GetLootContainer(lootList).size);
            _chunk.AddTileEntity(tileEntitySecureLootContainer);
        }
    }
}
