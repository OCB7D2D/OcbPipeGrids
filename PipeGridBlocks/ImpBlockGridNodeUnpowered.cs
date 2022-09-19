using NodeManager;

public abstract class ImpBlockGridNodeUnpowered : Block, IBlockConnection
{
	public virtual bool BreakDistance => false;
	public virtual bool NeedsPower => false;
	public virtual byte ConnectMask { get; set; } = 63;
	public virtual int MaxConnections { get; set; } = 6;
	Block IBlockConnection.Block => this;

    public byte ConnectFlags => (byte)(BreakDistance ? ConnectorFlag.Breaker : ConnectorFlag.None);

	public abstract void CreateGridItem(Vector3i blockPos, BlockValue blockValue);

	public abstract void RemoveGridItem(Vector3i blockPos);

	public virtual bool CanConnect(byte side, byte rotation)
		=> PipeBlockHelper.CanConnect(ConnectMask, side, rotation);

	public override void Init()
	{
		base.Init();
		// Call helper implementation
		// Parses `IBlockPipeConnection`
		PipeBlockHelper.InitBlock(this);
	}

	// *******************************************************
	// Shared implementation for all `IBlockPipeNode`
	// *******************************************************

	public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockAdded(this, pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockRemoved(this, pos, bv);
	}

	// Reuse object to pass around as parameter to `CanConnect`
	// Not sure much this brings; less allocations always good!?
	private static BlockConnector BCC = new BlockConnector();

	public override bool CanPlaceBlockAt(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv,
		// Ignore existing blocks?
		bool omitCollideCheck = false)
	{
		if (bv.ischild) return false; // ToDo: add more fancy code for multidims?
		return base.CanPlaceBlockAt(world, clrIdx, pos, bv, omitCollideCheck)
			&& NodeManagerInterface.Instance.Client.CanConnect(pos, BCC.Set(bv, this));
	}

	public override string GetCustomDescription(
		Vector3i pos, BlockValue bv)
	{
		return NodeManagerInterface.Instance.Client
			.GetCustomDescription(pos, bv);
	}

	public override string GetActivationText(
		WorldBase world, BlockValue bv,
		int clrIdx, Vector3i pos,
		EntityAlive focused)
	{
		return base.GetActivationText(world, bv, clrIdx, pos, focused) 
			+ "\n" + GetCustomDescription(pos, bv);
	}

}
