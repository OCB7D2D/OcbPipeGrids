using HarmonyLib;
using NodeManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlantationGrowing : BlockPlantGrowing, IBlockNode
{

	//########################################################
	// Implementation for block specialization
	//########################################################

	Block IBlockNode.Block => this;

	public BlockPlantationGrowing()
    {
		this.IsNotifyOnLoadUnload = true;
	}

	public virtual void CreateGridItem(Vector3i position, BlockValue bv)
	{
		Log.Out("Create Plan");
		var action = new ActionAddPlantGrowing();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public virtual void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemovePlantGrowing();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

	public override ulong GetTickRate() => 5; // (ulong)(growthRate * 20.0 * 60.0);

	public static bool IsLoaded(WorldBase world, Vector3i position)
	{
		return world.GetChunkFromWorldPos(position) != null;
	}

	HarmonyFieldProxy<Dictionary<int, WorldBlockTickerEntry>> FieldScheduledTicksDict = new
		HarmonyFieldProxy<Dictionary<int, WorldBlockTickerEntry>>(typeof(WorldBlockTicker), "scheduledTicksDict");

	HarmonyFieldProxy<SortedList> FieldScheduledTicksSorted = new
		HarmonyFieldProxy<SortedList>(typeof(WorldBlockTicker), "scheduledTicksSorted");
 
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

	public override bool UpdateTick(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv, bool bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {

		var dict = FieldScheduledTicksDict.Get(world.GetWBT());
		var sorted = FieldScheduledTicksSorted.Get(world.GetWBT());

		Log.Out("2) Rescheduled does still exist? {0} {1}",
			pos, dict.ContainsKey(ToHashCode(clrIdx, pos)));

		var light = world.GetBlockLightValue(clrIdx, pos);

		// Log.Out(System.Environment.StackTrace);

		var fert = world
			.GetBlock(pos + Vector3i.down)
			.Block.blockMaterial.FertileLevel;

		Log.Out("Tick plant when loaded {0} {1} {2} sun: {3} fert: {4}",
			IsLoaded(world, pos), bRandomTick, _ticksIfLoaded, light, fert);



		if (bv.isair || bv.ischild) return true;
		if (!IsLoaded(world, pos)) return true;
		Log.Out("Register Ticker {0}", blockID);
		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());
		return true;
        // return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }

    public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		// isPlantGrowingRandom = true; // To skip base tick
		base.OnBlockAdded(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockAdded(this, pos, bv);
		// Add tick to update server stuff when loaded
		if (bv.isair || bv.ischild) return;
		if (!IsLoaded(world, pos)) return;
		Log.Out("2) Register Ticker {0}", blockID);
		world.GetWBT().AddScheduledBlockUpdate(
			chunk.ClrIdx, pos, blockID, GetTickRate());
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockRemoved(this, pos, bv);
	}

    public override void OnBlockLoaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
    {
        base.OnBlockLoaded(world, clrIdx, pos, bv);
		if (bv.isair || bv.ischild) return;

		Log.Warning("Loaded Loaded Loaded Loaded Loaded Loaded");

		var dict = FieldScheduledTicksDict.Get(world.GetWBT());
		Log.Out("Rescheduled does still exist? {0} {1}",
			pos, dict.ContainsKey(ToHashCode(clrIdx, pos)));

		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());

		Log.Out("=> Rescheduled does still exist? {0} {1} at {2}",
			pos, dict.ContainsKey(ToHashCode(clrIdx, pos)), ToHashCode(clrIdx, pos));

		var sorted = FieldScheduledTicksSorted.Get(world.GetWBT());

		Log.Out("Sorted has {0}", (sorted.GetKey(0) as WorldBlockTickerEntry).scheduledTime);

		BlockValue block = world.GetBlock(clrIdx, pos);

		Log.Out("===== Fucker is loading ok? {0} vs {1}", block.type, blockID);

	}

	public override void OnBlockUnloaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
    {
        base.OnBlockUnloaded(world, clrIdx, pos, bv);
		if (bv.isair || bv.ischild) return;
		Log.Warning("Unloaded Unloaded Unloaded Unloaded Unloaded ");
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
		return "Activation: " + 
			base.GetActivationText(world, bv, clrIdx, pos, focused)
			+ "\n" + GetCustomDescription(pos, bv);
	}


}
