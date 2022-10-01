﻿using NodeManager;
using System;
using UnityEngine;

public class BlockPipeWell : ImpBlockPipeReservoirUnpowered, IBoundHelper
{

	//########################################################
	// Implementation for block specialization
	//########################################################

	public float FromGround = 0.08f / 1000f;
	public float FromFreeSky = 0.25f / 1000f;
	public float FromWetSurface = 0.15f / 1000f;
	public float FromSnowfall = 0.4f / 1000f;

    public float FromRainfall = 0.8f / 1000f;
	public float FromIrrigation = 5f / 1000f;
	public float MaxWaterLevel = 150f;

//	public int BlockReach = 2;

	public int BlockReach { get; protected set; }

	// public override bool AllowBlockTriggers => true;

	public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		if (Properties.Contains("BlockReach")) BlockReach =
			int.Parse(Properties.GetString("BlockReach"));
		if (Properties.Contains("FromGround")) FromGround =
			float.Parse(Properties.GetString("FromGround")) / 1000f;
		if (Properties.Contains("FromFreeSky")) FromFreeSky =
			float.Parse(Properties.GetString("FromFreeSky")) / 1000f;
		if (Properties.Contains("FromWetSurface")) FromWetSurface =
			float.Parse(Properties.GetString("FromWetSurface")) / 1000f;
		if (Properties.Contains("FromSnowfall")) FromSnowfall =
			float.Parse(Properties.GetString("FromSnowfall")) / 1000f;
		if (Properties.Contains("FromRainfall")) FromRainfall =
			float.Parse(Properties.GetString("FromRainfall")) / 1000f;
		if (Properties.Contains("FromIrrigation")) FromIrrigation =
			float.Parse(Properties.GetString("FromIrrigation")) / 1000f;
		if (Properties.Contains("MaxWaterLevel")) FromIrrigation =
			float.Parse(Properties.GetString("MaxWaterLevel"));
	}


	public BlockPipeWell()
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
		// Toggle bit flag
		bv.meta2 ^= 1;
		// Update block in world
		world.SetBlockRPC(pos, bv);
		// All is good
		return true;
	}

	private Color BoundHelperColor = Color.blue;

	public void UpdateBoundHelper(Vector3i pos, BlockValue bv)
    {
		BlockHelper.UpdateBoundHelper(pos, bv,
			this, BoundHelperColor, BlockReach);
	}

	public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
    {
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new,
			this, BoundHelperColor, BlockReach);
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		BlockHelper.UpdateBoundHelper(_blockPos, _blockValue,
			this, BoundHelperColor, BlockReach);
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

	public void OnWaterLevelChanged(Vector3i position, float level)
	{
		if (GameManager.Instance.World is WorldBase world)
		{
			if (world.GetChunkFromWorldPos(position) is Chunk chunk)
			{
				UpdateBlockEntity(chunk.GetBlockEntity(position), level);
			}
		}
	}

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		Log.Out("Create Well item");
		var action = new ActionAddWell();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveWell();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}


	public override void OnBlockAdded(
	  WorldBase _world,
	  Chunk _chunk,
	  Vector3i _blockPos,
	  BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		UpdateBoundHelper(_blockPos, _blockValue);
		if (_world.IsRemote()) return; // Only execute on server
		_world.GetWBT().AddScheduledBlockUpdate(
			_chunk.ClrIdx, _blockPos,
			blockID, GetTickRate());
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		LandClaimBoundsHelper.RemoveBoundsHelper(pos);
	}

	double RefreshRate = 0.005;

    public override ulong GetTickRate() => (ulong)(RefreshRate * 20.0 * 60.0);

	public override bool UpdateTick(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue,
		bool _bRandomTick,
		ulong _ticksIfLoaded,
		GameRandom _rnd)
	{
		_world.GetWBT().AddScheduledBlockUpdate(
			_clrIdx, _blockPos, blockID, GetTickRate());
		var query = new MsgWaterLevelQuery();
		query.Setup(_blockPos);
		query.AddWater = 0f;
		var weather = WeatherManager.Instance;
		query.AddWater += FromWetSurface * weather.GetCurrentWetSurfaceValue();
		query.AddWater += FromSnowfall * weather.GetCurrentSnowfallValue();
		query.AddWater += FromRainfall * weather.GetCurrentRainfallValue();
		NodeManagerInterface.SendToServer(query);
		return true;
	}

}
