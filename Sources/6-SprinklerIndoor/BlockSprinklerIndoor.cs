using Audio;
using NodeFacilitator;
using UnityEngine;

public class BlockSprinklerIndoor : ImpBlockPipeReservoirUnpowered, ISprinklerBlock, IRotationSimpleBlock
{

    //########################################################
    //########################################################

    public override TYPES NodeType => SprinklerIndoor.NodeType;

	//########################################################
	//########################################################

	//########################################################
	//########################################################

	public ReachConfig Reach { get; set; }
	// public Vector3i BlockReach { get; set; } = Vector3i.zero;
	// public Vector3i ReachOffset { get; set; } = Vector3i.zero;
	public Vector3i Dimensions => multiBlockPos?.dim ?? Vector3i.one;
	public Color BoundHelperColor { get; set; }
		= new Color32(160, 82, 45, 255);
    public Color ReachHelperColor { get; set; }
		= new Color32(160, 82, 45, 255);

	private string SoundSprinklerLoop = string.Empty;

    public override bool CanPlaceBlockAt(
        WorldBase world, int clrIdx,
        Vector3i pos, BlockValue bv,
        // Ignore existing blocks?
        bool omitCollideCheck = false)
    {
        return base.CanPlaceBlockAt(world, clrIdx, pos, bv, omitCollideCheck);
    }

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
        //	.NodeManager.SprinklerToSoilReach);
    }

    //########################################################
    //########################################################

    public BlockSprinklerIndoor()
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
		// We only have one command
		if (cmd != 0) return false;
		// Toggle bit flag
		bv.meta2 ^= 1;
		// Update block in world
		world.SetBlockRPC(pos, bv);
		// All is good
		return true;
	}
*/

	public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new, this, this);

		// Send BV change to unerlying server
		if (NodeManagerInterface.HasServer)
		{
            var action = new ActionSetBlockValue();
            action.Setup(NodeType, pos, bv_new);
            NodeManagerInterface.PushToWorker(action);
        }

        if (BlockHelper.GetEnabled(bv_old) ==
			BlockHelper.GetEnabled(bv_new)) return;
		BlockEntityData model = chunk.GetBlockEntity(pos);
		UpdateModelState(model, bv_new);
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
		=> NodeManagerInterface.Instance.Client.GetCustomDescription(pos, bv);

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
	}

	//########################################################
	//########################################################

	private void DebugTransform(Transform transform, string ident = "  ")
    {
		if (transform == null) return;
		Log.Out(ident + " {0}", transform.name);
		foreach (Transform child in transform)
			DebugTransform(child, ident + "  ");
    }

	private void UpdateModelState(BlockEntityData model, BlockValue bv_new)
	{
		Log.Out("Update model {0} => {1}", model, bv_new.meta2);
		// Do some sanity checks first (play safe)
		if (model == null || !model.bHasTransform) return;
		// DebugTransform(model.transform);

		bool enabled = BlockHelper.GetEnabled(bv_new);

		var anim = model.transform.GetComponentsInChildren<Animator>(true);
		foreach (Animator child in anim)
		{
            // Log.Out(" Animator at {0} => {1}", child.name, enabled);
            if (enabled) child.StopPlayback();
			else child.StartPlayback();
			child.enabled = true;
		}

		var sys = model.transform.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem child in sys)
		{
            // Log.Out(" particles at {0} => {1}", child.name, enabled);
            if (enabled) child.Play();
			else child.Stop();
			// child.set = true;
		}

		if (!string.IsNullOrEmpty(SoundSprinklerLoop))
        {
            // Log.Warning("Trying to run {0}", SoundSprinklerLoop);
            if (enabled) Manager.Play(model.pos, SoundSprinklerLoop);
			else Manager.Stop(model.pos, SoundSprinklerLoop);
		}

		var audio = model.transform.GetComponentsInChildren<AudioSource>(true);
		foreach (AudioSource child in audio)
		{
			child.Stop();
			// child.set = true;
		}


		Log.Warning("We want to change it to {0} at {1}",
			BlockHelper.GetEnabled(bv_new), model);
	}

	//########################################################
	//########################################################

}
 