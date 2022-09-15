using PipeManager;

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
		if (!PipeGridInterface.HasServer) return;
		PipeBlockHelper.OnBlockAdded(this, pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (!PipeGridInterface.HasServer) return;
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
			&& PipeGridInterface.Instance.Client.CanConnect(pos, BCC.Set(bv, this));
	}

	public override string GetCustomDescription(
		Vector3i pos, BlockValue bv)
	{
		return PipeGridInterface.Instance.Client
			.GetCustomDescription(pos, bv);
	}

	private BlockActivationCommand[] cmds = new BlockActivationCommand[0]
	{
	//new BlockActivationCommand("activate", "electric_switch", true),
	//new BlockActivationCommand("trigger", "wrench", true)
	};

    //public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    //{
	//	// Log.Out("GetBlockActivationCommands {0}", DisplayInfo);
	//	//this.cmds[0].enabled = true;
	//	//this.cmds[1].enabled = true;
	//	return this.cmds;
	//}

	/*




	public override string GetCustomDescription(
		Vector3i _blockPos,
		BlockValue _bv)
	{
		if (PipeGridManager.Instance.TryGetNode(
			_blockPos, out PipeGridConnection connection))
		{
			return string.Format("Unpowered {0}", connection.Grid);
		}
		if (PowerManager.HasInstance)
			PowerManager.Instance.Update();
		return base.GetCustomDescription(_blockPos, _bv);
	}
	*/
}
