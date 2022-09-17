using MusicUtils;
using NodeFacilitator;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlantationGrowing : BlockPlantGrowing, IPlantGrowingBlock, IPlantBlock, IStateBlock, IBlockExchangeItems
{

	//########################################################
	//########################################################

	public IVisualState CreateEmptyVisualState() => new MsgPlantProgress();

	public Vector2 PlantScaleMin { get; set; } = Vector3.one;
	public Vector2 PlantScaleFactor { get; set; } = Vector3.zero;

	//########################################################
	//########################################################

	public TYPES NodeType => TYPES.PlantationGrowing;

    public bool AcceptItem(ItemClass item)
    {
        string name = item.GetItemName();
        return name == "resourcePesticide";
    }
    //########################################################
    //########################################################

    //########################################################
    //########################################################
    Block IPlantGrowingBlock.BLK => this;
    Block IBlockNode.BLK => this;
    public MaintenanceOptions SoilMaintenance { get; set; } =
		new MaintenanceOptions(0.05f, 2.35f, 0.001f);
	public MaintenanceOptions WaterMaintenance { get; set; } =
		new MaintenanceOptions(0.05f, 2.35f, 0.001f);
	public MaintenanceOptions SprinklerMaintenance { get; set; } =
		new MaintenanceOptions(0.05f, 2.35f, 0.001f);

	public RangeOptions SprinklerRange = new RangeOptions(0.00f, 5f);

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

	public float FixedGrowth = -1;
	public float FixedOffset { get; set; } = -0.5f;

    public override void Init()
    {
        base.Init();

		if (Properties.Contains("FixedGrowth")) FixedGrowth =
			float.Parse(Properties.GetString("FixedGrowth"));
		if (Properties.Contains("FixedOffset")) FixedOffset =
			float.Parse(Properties.GetString("FixedOffset"));
		// Init defaults or from config
		BlockConfig.InitBlock(this);
        // BlockConfig.InitPlantGrowing(this);
        // BlockConfig.InitPlantBase(this);
    }

    public override bool CanPlaceBlockAt(WorldBase world, int clrIdx,
			Vector3i pos, BlockValue bc, bool omitCollideCheck = false)
		=> base.CanPlaceBlockAt(world, clrIdx, pos, bc, omitCollideCheck)
			|| IsAdvancedFarmPlot(world, clrIdx, pos - Vector3i.up, bc);

	private bool IsAdvancedFarmPlot(WorldBase world, int clrIdx, Vector3i ps, BlockValue bv)
	{
		var bt = world.GetBlock(clrIdx, ps);
		Log.Out("Checking block underneath {0} {1} {2}", bt.Block.GetBlockName(),
			bt.Block.blockMaterial, bt.Block.blockMaterial.FertileLevel);
		return bt.Block.blockMaterial.FertileLevel >= 20;
	}

    // public virtual void CreateGridItem(Vector3i position, BlockValue bv)
    // {
    // 	Log.Out("Delegate tot Create Plant");
    // 	var action = new ActionAddPlantGrowing();
    // 	action.Setup(position, bv);
    // 	NodeManagerInterface.SendToServer(action);
    // }
    // 
    // public virtual void RemoveGridItem(Vector3i position)
    // {
    // 	Log.Out("Delegate tot Delete Plant");
    // 	var action = new ActionRemovePlantGrowing();
    // 	action.Setup(position);
    // 	NodeManagerInterface.SendToServer(action);
    // }

    public override ulong GetTickRate() => 180; // (ulong)(growthRate * 20.0 * 60.0);
	
	private static bool IsLoaded(WorldBase world, Vector3i position)
		=> world.GetChunkFromWorldPos(position) != null;
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

	public override bool UpdateTick(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv,
		bool bRandomTick, ulong ticksIfLoaded, GameRandom rnd)
    {

		return false;

		if (bv.isair || bv.ischild) return false;
	
		//var action = new ActionUpdateLightLevels();
		//action.Setup(world, clrIdx, pos);
		//NodeManagerInterface.Instance.ToWorker.Add(action);

		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());
		var chunk = world.GetChunkFromWorldPos(pos);
		if (chunk.GetBlockEntity(pos) is BlockEntityData data)
        {
			Log.Error("Got Block Entity data {0}", data);
        }
		//UpdateParticleEffect(world, chunk, pos, bv);
		return true;
    }

	public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		Log.Out("+ Plant Added {0}", pos);

		base.OnBlockAdded(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;

		if (!NodeManagerInterface.Instance.Client.PlantStates.ContainsKey(pos))
		{
			Log.Out("Create new PlantStates of zero");
			NodeManagerInterface.Instance.Client.PlantStates[pos] = new PlantProgress(bv.type, 0f);
		}

		Log.Out("   +  Schedule Update {0} {1}", blockID, bv.type);
		BlockHelper.SetScheduledBlockUpdate(
			chunk.ClrIdx, pos, blockID, GetTickRate());
		if (!NodeManagerInterface.HasServer) return;
		NodeBlockHelper.OnBlockAdded(this, pos, bv);
		VisualStateHelper.OnBlockVisible(this, pos, bv);
		// SunLightHelper.OnBlockVisible(this, pos, bv);

        //var action = new ActionUpdateLightLevels();
        //action.Setup(world, chunk.ClrIdx, pos);
        //NodeManagerInterface.Instance.ToWorker.Add(action);

        // Add tick to update server stuff when loaded
        //world.GetWBT().AddScheduledBlockUpdate(
        //	chunk.ClrIdx, pos, blockID, GetTickRate());
        UpdateParticleEffect(pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		Log.Out("- Remove Plant {0}", NodeManagerInterface.Instance.Client
			.PlantStates.Count);
		NodeManagerInterface.Instance.Client
			.PlantStates.Remove(pos);
		if (!NodeManagerInterface.HasServer) return;
		Log.Out("- Plant Removed {0}", NodeManagerInterface.Instance.Client
			.PlantStates.Count);
		NodeBlockHelper.OnBlockRemoved(this, pos, bv);
		VisualStateHelper.OnBlockInvisible(this, pos, bv);
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

		if (_newBlockValue.type != _oldBlockValue.type)
		{
			Log.Error("ValueChange with type change (add/remove expected {0} vs {1})",
				_newBlockValue.type, _oldBlockValue.type);
			Log.Out("Create new PlantStates of zero");
			NodeManagerInterface.Instance.Client.PlantStates[_blockPos] =
				new PlantProgress(_newBlockValue.type, 0f);
		}

		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (_newBlockValue.isair || _newBlockValue.ischild) return;
		UpdateParticleEffect(_blockPos, _newBlockValue);
    }

    public override void OnBlockLoaded(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv)
    {
		if (!NodeManagerInterface.Instance.Client.PlantStates.ContainsKey(pos))
		{
			Log.Out("Loaded Block, so initialized PlantState to zero");
			NodeManagerInterface.Instance.Client.PlantStates[pos] = new PlantProgress(bv.type, 0f);
		}
        Log.Warning("Plant Growing was loaded");
        base.OnBlockLoaded(world, clrIdx, pos, bv);
		if (bv.isair || bv.ischild) return;
        VisualStateHelper.OnBlockVisible(this, pos, bv);
		world.GetWBT().AddScheduledBlockUpdate(
			clrIdx, pos, blockID, GetTickRate());
		// var chunk = world.GetChunkFromWorldPos(pos);
		UpdateParticleEffect(pos, bv);
	}

	public override void OnBlockUnloaded(WorldBase world,
		int clrIdx, Vector3i pos, BlockValue bv)
	{
	    base.OnBlockUnloaded(world, clrIdx, pos, bv);
		if (bv.isair || bv.ischild) return;
		VisualStateHelper.OnBlockInvisible(this, pos, bv);
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

    public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		// Log.Warning("On Transform Before Activated ============== {0}");
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
        // Log.Warning("On Transform After Activated ============== {0}");
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

    public void ReadState(PooledBinaryReader br)
    {
		Log.Out("Reading growth progress {0}", br.ReadSingle());
		 // GrowProgress
	}

    // public void WriteState(PooledBinaryWriter bw)
    // {
    //     throw new System.NotImplementedException();
    // }
}
