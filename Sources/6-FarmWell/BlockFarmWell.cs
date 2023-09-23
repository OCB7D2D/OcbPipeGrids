﻿using NodeFacilitator;
using UnityEngine;

public class BlockFarmWell : ImpBlockPipeReservoirUnpowered, IBoundHelper, IReacherBlock, IWellBlock, IFillStateVisual, IStateBlock
{

	//########################################################
	//########################################################
	public IVisualState CreateEmptyVisualState() => new FilledState();

	public override TYPES NodeType => TYPES.FarmWell;

	//########################################################
	//########################################################

	//########################################################
	// Implementation for block specialization
	//########################################################

	public float FromGround { get; set; } = 0.08f / 1000f;
	public float FromFreeSky { get; set; } = 0.25f / 1000f;
    public float FromWetSurface { get; set; } = 0.15f / 1000f;
    public float FromSnowSurface { get; set; } = 0.05f / 1000f;
	public float FromSnowfall { get; set; } = 0.4f / 1000f;

    public float FromRainfall { get; set; } = 0.8f / 1000f;
	public float FromIrrigation { get; set; } = 5f / 1000f;
	public float MaxWaterLevel { get; set; } = 150f;

	//	public int BlockReach = 2;

	public ReachConfig Reach { get; set; }
	//public Vector3i BlockReach { get; set; } = Vector3i.one;
	// public Vector3i ReachOffset { get; set; } = Vector3i.one;
	public Vector3i Dimensions => multiBlockPos?.dim ?? Vector3i.one;
	public Color BoundHelperColor { get; set; } = Color.green;
	public Color ReachHelperColor { get; set; } = Color.blue;

    public IBlockNode IBLK => this;
	public IReacherBlock RBLK => this;

	//public Vector3i RotatedReach(byte rotation)
	//	=> FullRotation.Rotate(rotation, BlockReach);

	public override void Init()
	{
		base.Init();
        BlockConfig.InitBlock(this);
        // BlockConfig.InitWell(this);
        // BlockConfig.InitReacher(this);
        //BlockHelper.UpdateReacher(this, ref NodeManager
		//	.NodeManager.WellToSoilReach);
	}


	public BlockFarmWell()
	{
		// Prepends `cmds` from parent to our `cmds`
		// Will update `cmd_offset` to parents length
		// Our commands will be moved to that position
		// In order to use base `GetBlockActivationCommands`
		BlockHelper.ExtendActivationCommands(this,
			typeof(ImpBlockPipeReservoirUnpowered),
			ref cmds, ref cmd_offset);
	}

	//########################################################
	// Add functionality for bound helper
	//########################################################

	private BlockActivationCommand[] cmds = new BlockActivationCommand[1] {
		new BlockActivationCommand("show_bounds", "frames", true),
	};

	// Offset after parent command are added
	private readonly int cmd_offset = 0;

	// We added our own command after any parent commands on constructor
	// Unfortunately we need to copy state from old to new array (why?)
	public override BlockActivationCommand[] GetBlockActivationCommands(
		WorldBase world, BlockValue bv, int clrIdx,
		Vector3i pos, EntityAlive entityFocusing)
    {
		// Get state for parent commands
		var old = base.GetBlockActivationCommands(world,
			bv, clrIdx, pos, entityFocusing);
		// Copy state from old to new array
		for (int i = 0; i < old.Length; i += 1)
			cmds[i].enabled = old[i].enabled;
		// Enable any new commands (add tests etc)
		// Not needed here as it's always enabled
		// for (int i = cmd_offset; i < cmds.Length; i += 1)
		// 	cmds[i].enabled = true;
		return cmds;
	}

    // Dispatch lower commands to parent
    // Handle all other commands here
    public override bool OnBlockActivated(string cmd, WorldBase world,
        int cIdx, Vector3i pos, BlockValue bv, EntityAlive player)
    {
        if (cmd == "show_bounds")
        {
            // Toggle bit flag
            bv.meta2 ^= 1;
            // Update block in world
            world.SetBlockRPC(pos, bv);
            // All is good
            return true;
        }
        else
        {
            return base.OnBlockActivated(cmd,
                world, cIdx, pos, bv, player);
        }
    }
    
	/*
        public override bool OnBlockActivated(int cmd,
            WorldBase world, int cIdx, Vector3i pos,
            BlockValue bv, EntityAlive player)
        {
            // Execute for base commands
            if (cmd < cmd_offset) return base.
                OnBlockActivated(cmd, world,
                    cIdx, pos, bv, player);
            // Make it zero based again
            cmd -= cmd_offset;
            // We only have one command
            if (cmd != 0) return false;
            // All is good
            return true;
        }
    */

    public void UpdateBoundHelper(Vector3i pos, BlockValue bv)
    {
		BlockHelper.UpdateBoundHelper(pos, bv, this, this);
	}

	public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
    {
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		UpdateBoundHelper(pos, bv_new);
	}

	public override void OnBlockLoaded(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv)
    {
        base.OnBlockLoaded(world, clrIdx, pos, bv);
		VisualStateHelper.OnBlockVisible(this, pos, bv);
		UpdateBoundHelper(pos, bv);
	}

    public override void OnBlockUnloaded(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv)
    {
		BoundsHelper.RemoveBoundsHelper(pos);
		VisualStateHelper.OnBlockInvisible(this, pos, bv);
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
	}

	//########################################################
	// Update visual well fill state after activation
	//########################################################

	public override void OnBlockEntityTransformAfterActivated(
		WorldBase _world, Vector3i _blockPos, int _cIdx,
		BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(
			_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (_blockValue.ischild) return;
		//if (PipeGridManager.Instance.TryGetNode(
		//	_blockPos, out PipeGridWell well))
		//{
		//	well.UpdateBlockEntity(_ebcd);
		//}
	}

	public void UpdateBlockEntity(BlockEntityData entity, float level)
	{
		if (entity == null) return;
		if (entity.transform == null) return;
		if (entity.transform.Find("WaterLevel") is Transform transform)
		{
			Vector3 position = transform.localPosition;
			position.y = 0.33f / MaxWaterLevel * level - 0.49f;
			//Log.Out("Update water level to {0}", position.y);
			transform.localPosition = position;
		}
	}

	public void OnFillStateChanged(Vector3i position, float level)
	{
		if (GameManager.Instance.World is WorldBase world)
		{
			if (world.GetChunkFromWorldPos(position) is Chunk chunk)
			{
				UpdateBlockEntity(chunk.GetBlockEntity(position), level);
			}
		}
	}

	public override void OnBlockAdded(
	  WorldBase world, Chunk chunk,
	  Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		VisualStateHelper.OnBlockVisible(this, pos, bv);
		if (bv.isair || bv.ischild) return;
		UpdateBoundHelper(pos, bv);
		// Only execute on clients
		if (GameManager.IsDedicatedServer) return;
		world.GetWBT().AddScheduledBlockUpdate(
			chunk.ClrIdx, pos,
			blockID, GetTickRate());
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		VisualStateHelper.OnBlockInvisible(this, pos, bv);
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		BoundsHelper.RemoveBoundsHelper(pos);
	}

	// double RefreshRate = 0.005;

    // public override ulong GetTickRate() => (ulong)(RefreshRate * 20.0 * 60.0);

	// public override bool UpdateTick(
	// 	WorldBase _world,
	// 	int _clrIdx,
	// 	Vector3i _blockPos,
	// 	BlockValue _blockValue,
	// 	bool _bRandomTick,
	// 	ulong _ticksIfLoaded,
	// 	GameRandom _rnd)
	// {
	// 	_world.GetWBT().AddScheduledBlockUpdate(
	// 		_clrIdx, _blockPos, blockID, GetTickRate());
	// 	var query = new MsgFillStateQuery();
	// 	query.Setup(_blockPos);
	// 	query.ChangeFillState = 0f;
	// 	var weather = WeatherManager.Instance;
	// 	query.ChangeFillState += FromWetSurface * weather.GetCurrentWetSurfaceValue();
	// 	query.ChangeFillState += FromSnowfall * weather.GetCurrentSnowfallValue();
	// 	query.ChangeFillState += FromRainfall * weather.GetCurrentRainfallValue();
	// 	NodeManagerInterface.SendToServer(query);
	// 	return true;
	// }

}