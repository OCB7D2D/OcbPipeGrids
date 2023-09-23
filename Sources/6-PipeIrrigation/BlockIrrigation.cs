using NodeFacilitator;
using UnityEngine;

class BlockIrrigation : ImpBlockGridNodePowered, IBoundHelper, IReacherBlock
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeIrrigation;

	//########################################################
	//########################################################

	public IBlockNode IBLK => this;
	public IReacherBlock RBLK => this;

	public ReachConfig Reach { get; set; }
	// public Vector3i BlockReach { get; set; } = Vector3i.zero;
	// public Vector3i ReachOffset { get; set; } = Vector3i.zero;
	public Vector3i Dimensions => multiBlockPos?.dim ?? Vector3i.one;
	public Color BoundHelperColor { get; set; } = new Color32(160, 82, 45, 255);
	public Color ReachHelperColor { get; set; } = new Color32(160, 82, 45, 255);

	public override bool BreakSegment => true;
	public override bool NeedsPower => true;

	public override void Init()
	{
		base.Init();
        // Parse block XML properties
        BlockConfig.InitBlock(this);
        //BlockConfig.InitReacher(this);
        //BlockConfig.UpdateReacher(this, ref NodeManager
		//	.NodeManager.IrrigatorToWellReach);
	}




    public BlockIrrigation()
    {
        // Prepends `cmds` from parent to our `cmds`
        // Will update `cmd_offset` to parents length
        // Our commands will be moved to that position
        // In order to use base `GetBlockActivationCommands`
        BlockHelper.ExtendActivationCommands(this,
            typeof(ImpBlockGridNodePowered), ref cmds);
    }

    //########################################################
    // Add functionality for bound helper
    //########################################################

    private BlockActivationCommand[] cmds = new BlockActivationCommand[1] {
        new BlockActivationCommand("show_bounds", "frames", true),
    };

    //########################################################
    //########################################################

    // We added our own command after any parent commands on constructor
    // Unfortunately we need to copy state from old to new array (why?)
    public override BlockActivationCommand[] GetBlockActivationCommands(
        WorldBase world, BlockValue bv, int clrIdx,
        Vector3i pos, EntityAlive entityFocusing)
    {
        // Skip to get state for parent commands
        // As we don't want the land claim requirement
        cmds[0].enabled = CanPickup && TakeDelay > 0;
        // Can always toggle to show bounds
        return cmds;
    }

    //########################################################
    //########################################################

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

    //########################################################
    //########################################################

    public void UpdateBoundHelper(Vector3i pos, BlockValue bv)
        => BlockHelper.UpdateBoundHelper(pos, bv, this, this);

    public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
        int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
    {
        base.OnBlockValueChanged(world, chunk,
            clrIdx, pos, bv_old, bv_new);
        UpdateBoundHelper(pos, bv_new);
    }

    public override void OnBlockLoaded(WorldBase world,
        int clrIdx, Vector3i pos, BlockValue bv)
    {
        base.OnBlockLoaded(world, clrIdx, pos, bv);
        UpdateBoundHelper(pos, bv);
    }

    public override void OnBlockUnloaded(WorldBase world,
        int clrIdx, Vector3i pos, BlockValue bv)
    {
        BoundsHelper.RemoveBoundsHelper(pos);
        base.OnBlockUnloaded(world, clrIdx, pos, bv);
    }

    //########################################################
    //########################################################

}
