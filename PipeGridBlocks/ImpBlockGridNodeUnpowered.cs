using NodeManager;

public abstract class ImpBlockGridNodeUnpowered : BlockRemote, IBlockConnection
{
	public virtual bool BreakDistance => false;
	public virtual bool NeedsPower => false;
	public virtual byte ConnectMask { get; set; } = 63;
	public virtual uint SideMask { get; set; } = 0;
	public virtual bool MultiBlockPipe { get; set; } = false;
	public virtual byte PipeDiameter { get; set; } = 0;
	public virtual int MaxConnections { get; set; } = 6;

	public byte ConnectFlags => (byte)(BreakDistance ? ConnectorFlag.Breaker : ConnectorFlag.None);

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
			&& NodeManagerInterface.Instance.Mother.CanConnect(pos, BCC.Set(bv, this));
	}

}
