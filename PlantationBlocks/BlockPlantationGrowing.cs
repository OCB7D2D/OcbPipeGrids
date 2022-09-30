using NodeManager;

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
		Log.Out("Delegate tot Create Plant");
		var action = new ActionAddPlantGrowing();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public virtual void RemoveGridItem(Vector3i position)
	{
		Log.Out("Delegate tot Delete Plant");
		var action = new ActionRemovePlantGrowing();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

	public override ulong GetTickRate() => 5; // (ulong)(growthRate * 20.0 * 60.0);

	private static bool IsLoaded(WorldBase world, Vector3i position)
		=> world.GetChunkFromWorldPos(position) != null;
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

	public override bool UpdateTick(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv,
		bool bRandomTick, ulong ticksIfLoaded, GameRandom rnd)
    {
		// This should never be false when ticked
		if (!IsLoaded(world, pos)) return true;

		var action = new ActionUpdatePlantStats();
		action.Setup(world, clrIdx, pos);
		NodeManagerInterface.Instance.ToWorker.Enqueue(action);

		if (bv.isair || bv.ischild) return true;
		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());
		return true;
    }

    public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		Log.Out("+ Plant Added");
		base.OnBlockAdded(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockAdded(this, pos, bv);
		// Add tick to update server stuff when loaded
		if (bv.isair || bv.ischild) return;
		Log.Out("   +  Schedule Update {0} {1}", blockID, bv.type);
		BlockHelper.SetScheduledBlockUpdate(
			chunk.ClrIdx, pos, blockID, GetTickRate());
		//world.GetWBT().AddScheduledBlockUpdate(
		//	chunk.ClrIdx, pos, blockID, GetTickRate());
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		if (bv.isair || bv.ischild) return;
		Log.Out("- Plant Removed");
		PipeBlockHelper.OnBlockRemoved(this, pos, bv);
	}

    public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
    {
		Log.Out("~ Plant Changed");
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
    }

    public override void OnBlockLoaded(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv)
    {
        base.OnBlockLoaded(world, clrIdx, pos, bv);
		if (bv.isair || bv.ischild) return;
		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());
	}

	// public override void OnBlockUnloaded(WorldBase world,
	// 	int clrIdx, Vector3i pos, BlockValue bv)
    // {
    //     base.OnBlockUnloaded(world, clrIdx, pos, bv);
	// 	if (bv.isair || bv.ischild) return;
	// }

	public override string GetCustomDescription(
	Vector3i pos, BlockValue bv)
	{
		return NodeManagerInterface.Instance.Mother
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
