using NodeManager;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlantationGrowing : BlockPlantGrowing, IPlantBlock
{

	//########################################################
	//########################################################
	Block IBlockNode.BLK => this;
	public MaintenanceOptions SoilMaintenance { get; set; } =
		new MaintenanceOptions(0.05f, 2.35f, 0.001f);
	public MaintenanceOptions WaterMaintenance { get; set; } =
		new MaintenanceOptions(0.05f, 2.35f, 0.001f);

	public float GrowthMaintenanceFactor { get; set; } = 0.05f / 1200f;
	public float LightMaintenance { get; set; }
	public string IllnessEffect { get; set; } = string.Empty;

	public float GrowthRate => 1f / (12f * GetGrowthRate());

	//########################################################
	// Implementation for block specialization
	//########################################################

	

    public BlockPlantationGrowing()
    {
		this.IsNotifyOnLoadUnload = true;
	}

	public override void Init()
    {
        base.Init();
		// Init defaults or from config
		BlockConfig.InitPlant(this);
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
		//var chunk = world.GetChunkFromWorldPos(pos);
		//UpdateParticleEffect(world, chunk, pos, bv);
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
		UpdateParticleEffect(pos, bv);
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
		if (checks.TryGetValue(pos,
			out GameObject go))
        {
			Origin.Remove(go.transform);
			go.SetActive(false);
			Object.Destroy(go);
			checks.Remove(pos);
        }
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
    {
		Log.Out("~ Plant Changed");
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		UpdateParticleEffect(_blockPos, _newBlockValue);
    }

    public override void OnBlockLoaded(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv)
    {
        base.OnBlockLoaded(world, clrIdx, pos, bv);
		if (bv.isair || bv.ischild) return;
		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());
		// var chunk = world.GetChunkFromWorldPos(pos);
		UpdateParticleEffect(pos, bv);
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

    public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		Log.Warning("On Transform Before Activated ============== {0}");
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		Log.Warning("On Transform After Activated ============== {0}");
		// UpdateParticleEffect(_world);
	}

	static Dictionary<Vector3i, GameObject> checks =
		new Dictionary<Vector3i, GameObject>();

	public void UpdateParticleEffect(
		Vector3i pos, BlockValue bv)
	{
		if (IllnessEffect == string.Empty) return;
		// Try to get game object
		if (!checks.TryGetValue(pos,
			out GameObject go))
        {
			Log.Warning("Load Fly particle effect again");
			var effect = DataLoader.LoadAsset<GameObject>(IllnessEffect);
			if (effect == null) return;
			go = Object.Instantiate(effect);
			if (go == null) return;
			checks.Add(pos, go);
			// Shift with origin
			Origin.Add(go.transform, -1);
		}
		if (go == null) return;
		// Update position for object
		go.transform.localPosition = pos
			+ Vector3.one / 2f
			- Origin.position;
		var illness = BlockHelper.GetIllness(bv);
		foreach (var particles in go.transform.GetComponentsInChildren<ParticleSystem>())
        {
			var pmain = particles.main;
			pmain.maxParticles = illness;
			Log.Out("Changes particles to {0}", pmain.maxParticles);

		}
		// Check if plant is actually sick
		go.SetActive(illness > 0);
	}

}
