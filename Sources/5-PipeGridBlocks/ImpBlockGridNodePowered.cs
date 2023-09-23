﻿using NodeFacilitator;

public abstract class ImpBlockGridNodePowered : PoweredBase, IBlockConnection
{
	public virtual bool BreakSegment { get; set; } = false;
	public virtual bool NeedsPower => false;
	public virtual byte ConnectMask { get; set; } = 63;
	public virtual uint SideMask { get; set; } = 0;
	public virtual byte PypeType { get; set; } = 0;
	public virtual byte PipeDiameter { get; set; } = 0;
	public virtual int MaxConnections { get; set; } = 6;

	public byte ConnectFlags => (byte)(BreakSegment ? ConnectorFlag.Breaker : ConnectorFlag.None);

	public virtual bool CanConnect(byte side, byte rotation)
		=> NodeBlockHelper.CanConnect(ConnectMask, side, rotation);

	public override void Init()
	{
		base.Init();
		// Call helper implementation
		// Parses `IBlockPipeConnection`
		BlockConfig.InitConnection(this);
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
		if (bv.ischild || bv.isair) return false; // ToDo: add more fancy code for multidims?
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
		var txt = base.GetActivationText(world, bv, clrIdx, pos, focused);
		if (!string.IsNullOrWhiteSpace(txt)) return txt;
        return GetCustomDescription(pos, bv);
	}

}