﻿public interface IBlockConnection
{
    void CreateGridItem(Vector3i blockPos, BlockValue blockValue);

    void RemoveGridItem(Vector3i blockPos);

    bool BreakDistance { get; }

    bool NeedsPower { get; }

    int MaxConnections { get; set; }

    bool CanConnect(byte side, byte rotation);

    Block Block { get; }

    byte ConnectMask { get; set; }

    byte ConnectFlags { get; }

    //#####################################################
    // Some base implementations each Block must have
    // Too bad we can't have default implementations yet
    //#####################################################

    string GetBlockName();

    void OnBlockAdded(
        WorldBase world, Chunk chunk,
        Vector3i pos, BlockValue bv);

    void OnBlockRemoved(
        WorldBase world, Chunk chunk,
        Vector3i pos, BlockValue bv);

    bool CanPlaceBlockAt(
        WorldBase world, int clrIdx,
        Vector3i pos, BlockValue bv,
        // Ignore existing blocks?
        bool omitCollideCheck = false);

    string GetActivationText(
        WorldBase world, BlockValue bv, int clrIdx,
        Vector3i pos, EntityAlive focused);

}
