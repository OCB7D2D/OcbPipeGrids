using NodeManager;

public abstract class ImpBlockChest : BlockSecureLoot, ILootBlock, ITileEntityChangedListener
{

	//########################################################
	//########################################################

	Block IBlockNode.BLK => this;

    public abstract TYPES NodeType { get; }


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
		if (!NodeManagerInterface.HasServer) return;
		if (world.GetTileEntity(chunk.ClrIdx, pos) is
			TileEntityLootContainer container)
		{
			if (!container.listeners.Contains(this))
			{
				container.listeners.Add(this);
			}
		}
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		if (bv.isair || bv.ischild) return;
		if (world.GetTileEntity(chunk.ClrIdx, pos) is
			TileEntityLootContainer container)
		{
			container.listeners.Remove(this);
		}
	}

	public override void OnBlockLoaded(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockLoaded(world, clrIdx, pos, bv);
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
		{
			if (!container.listeners.Contains(this))
			{
				container.listeners.Add(this);
			}
		}
	}

	public override void OnBlockUnloaded(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
		{
			container.listeners.Remove(this);
		}
	}

	public void OnTileEntityChanged(TileEntity te, int data)
	{
		if (!(te is TileEntityLootContainer container)) return;
		var action = new ActionUpdateChest();
		action.Setup(te.ToWorldPos(), container.GetItems());
		NodeManagerInterface.Instance.ToWorker.Enqueue(action);
	}

	//########################################################
	//########################################################


	//########################################################
	//########################################################
}
