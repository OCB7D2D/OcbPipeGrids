using NodeManager;
using UnityEngine;

public class BlockPlantationSprinkler : BlockRemote, IReacherBlock
{

    //########################################################
    //########################################################

    public override TYPES NodeType => TYPES.PlantationSprinkler;

    //########################################################
    //########################################################

    //########################################################
    //########################################################

    public Vector3i BlockReach { get; set; } = Vector3i.zero;
    public Vector3i ReachOffset { get; set; } = Vector3i.zero;
    public Color BoundHelperColor { get; set; } = new Color32(160, 82, 45, 255);
    public Color ReachHelperColor { get; set; } = new Color32(160, 82, 45, 255);

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
		// Initialize maintenance options
		// SoilMaintenance.Init(Properties, "Soil");
		// WaterMaintenance.Init(Properties, "Water");
		BlockConfig.InitReacher(this);
    }

    //########################################################
    //########################################################

    public BlockPlantationSprinkler()
    {
        this.IsNotifyOnLoadUnload = true;
    }

	//########################################################
	// Add functionality for bound helper
	//########################################################

	private BlockActivationCommand[] cmds = new BlockActivationCommand[1] {
		new BlockActivationCommand("show_bounds", "frames", true),
	};

	// We added our own command after any parent commands on constructor
	// Unfortunately we need to copy state from old to new array (why?)
	public override BlockActivationCommand[] GetBlockActivationCommands(
		WorldBase world, BlockValue bv, int clrIdx,
		Vector3i pos, EntityAlive entityFocusing)
	{
		cmds[0].enabled = true;
		return cmds;
	}

	// Dispatch lower commands to parent
	// Handle all other commands here
	public override bool OnBlockActivated(int cmd,
		WorldBase world, int cIdx, Vector3i pos,
		BlockValue bv, EntityAlive player)
	{
		// We only have one command
		if (cmd != 0) return false;
		// Toggle bit flag
		bv.meta2 ^= 1;
		// Update block in world
		world.SetBlockRPC(pos, bv);
		// All is good
		return true;
	}

	public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new, this, this);
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
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockAdded(this, pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		BoundsHelper.RemoveBoundsHelper(pos);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockRemoved(this, pos, bv);
	}

	public override string GetCustomDescription(
	Vector3i pos, BlockValue bv)
	{
		return NodeManagerInterface.Instance.Mother
			.GetCustomDescription(pos, bv);
	}

	public override void OnBlockUnloaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
	{
		BoundsHelper.RemoveBoundsHelper(pos);
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
	}

	public override void OnBlockLoaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
	{
		base.OnBlockLoaded(world, clrIdx, pos, bv);
		BlockHelper.UpdateBoundHelper(pos, bv, this, this);
	}

}
