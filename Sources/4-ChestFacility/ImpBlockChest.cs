using NodeFacilitator;
using System;

public abstract class ImpBlockChest : BlockSecureLoot, ILootBlock, ITileEntityChangedListener
{

    //########################################################
    //########################################################

    Block IBlockNode.BLK => this;

    public abstract TYPES NodeType { get; }

    public static void DoOnBlockAdded(
        ITileEntityChangedListener self,
        WorldBase world, Chunk chunk,
        Vector3i pos, BlockValue bv)
    {
        if (!NodeManagerInterface.HasServer) return;
        if (world.GetTileEntity(chunk.ClrIdx, pos) is
            TileEntityLootContainer container)
        {
            if (!container.listeners.Contains(self))
            {
                container.listeners.Add(self);
            }
        }

    }

    public static void DoOnBlockRemoved(
        ITileEntityChangedListener self,
        WorldBase world, Chunk chunk,
        Vector3i pos, BlockValue bv)
    {
        if (!NodeManagerInterface.HasServer) return;
        if (bv.isair || bv.ischild) return;
        if (world.GetTileEntity(chunk.ClrIdx, pos) is
            TileEntityLootContainer container)
        {
            container.listeners.Remove(self);
        }
    }

    public static void DoOnBlockLoaded(
        ITileEntityChangedListener self,
        WorldBase world, int clrIdx,
        Vector3i pos, BlockValue bv)
    {
        if (world.GetTileEntity(clrIdx, pos) is
            TileEntityLootContainer container)
        {
            if (!container.listeners.Contains(self))
            {
                container.listeners.Add(self);
            }
        }
    }

    public static void DoOnBlockUnloaded(
        ITileEntityChangedListener self,
        WorldBase world, int clrIdx,
        Vector3i pos, BlockValue bv)
    {
        if (world.GetTileEntity(clrIdx, pos) is
            TileEntityLootContainer container)
        {
            container.listeners.Remove(self);
        }
    }

    public static void DoOnTileEntityChanged(
        TileEntity te, int data)
    {
        if (!(te is TileEntityLootContainer container)) return;
        var action = new ActionUpdateChest();
        action.Setup(te.ToWorldPos(), container.GetItems());
        NodeManagerInterface.Instance.ToWorker.Add(action);
    }

    //########################################################
    // Abstract implementation for `BlockNode`
    // Must specialize these for specific types
    //########################################################

    // public abstract void CreateGridItem(Vector3i pos, BlockValue bv);

    // public abstract void RemoveGridItem(Vector3i pos);

    //########################################################
    //########################################################

    public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		DoOnBlockAdded(this, world, chunk, pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		DoOnBlockRemoved(this, world, chunk, pos, bv);
    }

	public override void OnBlockLoaded(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockLoaded(world, clrIdx, pos, bv);
        DoOnBlockLoaded(this, world, clrIdx, pos, bv);
    }

    public override void OnBlockUnloaded(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
        DoOnBlockUnloaded(this, world, clrIdx, pos, bv);
	}

	public void OnTileEntityChanged(TileEntity te, int data)
	{
        DoOnTileEntityChanged(te, data);
	}

    //########################################################
    //########################################################


    //########################################################
    //########################################################
}
