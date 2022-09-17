using NodeFacilitator;
using UnityEngine;

public class BlockPlantationGrowLight : BlockRemoteDesc, ISprinklerBlock
{

    //########################################################
    //########################################################

    public override TYPES NodeType => PlantationGrowLight.NodeType;

	//########################################################
	//########################################################

	//########################################################
	//########################################################

	public ReachConfig Reach { get; set; }	
	// public Vector3i BlockReach { get; set; } = Vector3i.zero;
    // public Vector3i ReachOffset { get; set; } = Vector3i.zero;
	public Vector3i Dimensions => multiBlockPos?.dim ?? Vector3i.one;
	public Color BoundHelperColor { get; set; }
		= new Color32(0, 82, 45, 255);
    public Color ReachHelperColor { get; set; }
		= new Color32(160, 0, 45, 255);

	private string SoundSprinklerLoop = string.Empty;

    //########################################################
    //########################################################

    // public MaintenanceOptions WaterMaintenance = new
    //     MaintenanceOptions(0.01f, 1.25f, 0.1f);
    // public MaintenanceOptions SoilMaintenance = new
    //     MaintenanceOptions(0.01f, 1.25f, 0.1f);
    // 
    // public RangeOptions WaterRange = new RangeOptions(0.02f, 2f);
    // public RangeOptions SoilRange = new RangeOptions(0.03f, 3f);

    //########################################################
    // Parse custom properties on init
    //########################################################

    public override void Init()
    {
        base.Init();
		SoundSprinklerLoop = BlockHelper.ParseString(
			Properties, "SoundSprinklerLoop", string.Empty);
        BlockConfig.InitBlock(this);
        //BlockConfig.InitReacher(this);
        //BlockConfig.UpdateReacher(this, ref NodeManager
		//	.NodeManager.GrowLightToPlantReach, 1);
	}

	//########################################################
	//########################################################

	public BlockPlantationGrowLight()
    {
        this.IsNotifyOnLoadUnload = true;
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

	private BlockActivationCommand[] cmds = new BlockActivationCommand[2] {
        new BlockActivationCommand("activate", "electric_switch", true),
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
		// Mine are always enabled
		return cmds;
	}

	// Dispatch lower commands to parent
	// Handle all other commands here
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
		// Toggle bit flag
		if (cmd == 0) bv.meta ^= 2;
        else if (cmd == 1) bv.meta2 ^= 1;
        // Update block in world
        world.SetBlockRPC(pos, bv);
        // All is good
        return true;
	}
	*/

	public static void UpdateLightState(BlockEntityData model, Vector3i pos, BlockValue bv)
	{
		if (model == null || model.bHasTransform == false) return;
        if (model.transform.FindInChildren("MainLight") is Transform transform)
        {
            if (transform.GetComponent<LightLOD>() is LightLOD lightLOD)
            {
                bool state = BlockHelper.GetEnabled(bv);
                Log.Out("Update Light State {0}", bv.meta2);
                lightLOD.SwitchOnOff(state, pos);
                if (lightLOD.GetLight() is Light light)
                    light.enabled = state; // Switch light
            }
        }
    }

    public static void UpdateLightState(
		Chunk chunk, Vector3i pos, BlockValue bv)
	{
		if (chunk == null) return;
        BlockEntityData model = chunk.GetBlockEntity(pos);
		UpdateLightState(model, pos, bv);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase world,
		Vector3i pos, int clrIdx, BlockValue bv, BlockEntityData model)
    {
        base.OnBlockEntityTransformAfterActivated(world, pos, clrIdx, bv, model);
        UpdateLightState(model, pos, bv);
    }

    public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new, this, this);
		UpdateLightState(chunk, pos, bv_new);
		if (BlockHelper.GetEnabled(bv_old) ==
			BlockHelper.GetEnabled(bv_new)) return;
	}

	//########################################################
	// Implementation for node manager
	//########################################################

	public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		BlockHelper.UpdateBoundHelper(pos, bv, this, this);
        UpdateLightState(chunk, pos, bv);

        //if (!NodeManagerInterface.HasServer) return;
        // NodeBlockHelper.OnBlockAdded(this, pos, bv);
    }

    public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		BoundsHelper.RemoveBoundsHelper(pos);
		//if (!NodeManagerInterface.HasServer) return;
		// NodeBlockHelper.OnBlockRemoved(this, pos, bv);
	}

	//########################################################
	//########################################################

	public override string GetCustomDescription(Vector3i pos, BlockValue bv)
		=> NodeManagerInterface.Instance.Client.GetCustomDescription(pos, bv) + " => " + bv.ischild.ToString() + " - " + pos.ToString();

	//########################################################
	//########################################################

	public override void OnBlockUnloaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
	{
		BoundsHelper.RemoveBoundsHelper(pos);
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
	}

	public override void OnBlockLoaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
	{
		base.OnBlockLoaded(world, clrIdx, pos, bv);
		BlockHelper.UpdateBoundHelper(pos, bv, this, this);
        UpdateLightState(world.GetChunkSync(pos) as Chunk, pos, bv);
    }

    //########################################################
    //########################################################

    //########################################################
    //########################################################

}
 