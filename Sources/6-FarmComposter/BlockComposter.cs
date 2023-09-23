using NodeFacilitator;
using System.Collections.Generic;
using UnityEngine;


public class BlockComposter : ImpBlockChest, IStateBlock, ILootBlock, IBoundHelper, IReacherBlock, IFilledBlock, IFillStateVisual
{

	//########################################################
	//########################################################

	public IVisualState CreateEmptyVisualState() => new FilledState();

	public override TYPES NodeType => TYPES.PlantationComposter;

	//########################################################
	//########################################################

	//########################################################
	// Implementation for block specialization
	//########################################################

	// Sync the state with worker manager
	// When loading sync from manager to TE
	// When loaded sync after TE is unlocked
	// When unloading, sync again just to be sure
	// So the manager has a copy of all items in chest!?

	public float MaxFillState { get; set; } = 50f;

	public IBlockNode IBLK => this;
	public IReacherBlock RBLK => this;

	public ReachConfig Reach { get; set; }

	 // public Vector3i BlockReach { get; set; } = Vector3i.zero;
	 // public Vector3i ReachOffset { get; set; } = Vector3i.zero;
	public Vector3i Dimensions => multiBlockPos?.dim ?? Vector3i.one;
	public Color BoundHelperColor { get; set; } = new Color32(160, 82, 45, 255);
	public Color ReachHelperColor { get; set; } = new Color32(160, 82, 45, 255);

	public override void Init()
    {
        base.Init();
        // Parse block XML properties
        BlockConfig.InitBlock(this);
        
		//BlockConfig.InitFilled(this);
		//BlockConfig.InitReacher(this);
        //BlockConfig.UpdateReacher(this, ref NodeManager
		//	.NodeManager.ComposterToSoilReach);
	}

	public BlockComposter()
	{
		this.IsNotifyOnLoadUnload = true;
		// Prepends `cmds` from parent to our `cmds`
		// Will update `cmd_offset` to parents length
		// Our commands will be moved to that position
		// In order to use base `GetBlockActivationCommands`
		BlockHelper.ExtendActivationCommands(this,
			typeof(ImpBlockChest),
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

    public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new, this, this);
	}

	//########################################################
	// Implementation for node manager
	//########################################################


	private static bool IsLoaded(WorldBase world, Vector3i position)
		=> world.GetChunkFromWorldPos(position) != null;
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

    public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		BlockHelper.UpdateBoundHelper(pos, bv, this, this);
		if (!GameManager.IsDedicatedServer)
			world.GetWBT().AddScheduledBlockUpdate(
				chunk.ClrIdx, pos, blockID, GetTickRate());
		if (!NodeManagerInterface.HasServer) return;
		NodeBlockHelper.OnBlockAdded(this, pos, bv);
        VisualStateHelper.OnBlockVisible(this, pos, bv);
    }
	
    public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		VisualStateHelper.OnBlockInvisible(this, pos, bv);
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		BoundsHelper.RemoveBoundsHelper(pos);
		if (!NodeManagerInterface.HasServer) return;
		NodeBlockHelper.OnBlockRemoved(this, pos, bv);
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


	public static Dictionary<Vector3i, Dictionary<ushort, short>> ChestChanges
		= new Dictionary<Vector3i, Dictionary<ushort, short>>();

	public override void OnBlockUnloaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
    {
		BoundsHelper.RemoveBoundsHelper(pos);
		VisualStateHelper.OnBlockInvisible(this, pos, bv);
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
		if (bv.ischild) return; // Only register master
		if (world.IsRemote()) return; // Only execute on server
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
        {
			var action = new ActionUpdateChest();
			action.Setup(pos, container.GetItems());
			NodeManagerInterface.Instance
				.ToWorker.Add(action);
        }
	}

	public override void OnBlockLoaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
	{
		base.OnBlockLoaded(world, clrIdx, pos, bv);
		VisualStateHelper.OnBlockVisible(this, pos, bv);
		BlockHelper.UpdateBoundHelper(pos, bv, this, this);
		if (bv.ischild) return; // Only register master
		if (world.IsRemote()) return; // Only execute on server
		// Apply pending changes and check if valid!
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
		{
			Log.Warning("Loaded ok, with TE {0}", pos);
			if (ChestChanges.TryGetValue(pos,
				out Dictionary<ushort, short> stack))
			{
				ExecuteChestModification
					.ApplyChangesToChest(
						stack, container);
			}

			// ToDo: inform worker of chest?
			var action = new ActionUpdateChest();
			action.Setup(pos, container.GetItems());
			NodeManagerInterface.Instance
				.ToWorker.Add(action);

		}
		else
        {
			Log.Warning("Loaded without TE {0}", pos);
        }
	}

	// double RefreshRate = 0.005;

	// public override ulong GetTickRate() => (ulong)(RefreshRate * 20.0 * 60.0);
	// 
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
	// 	NodeManagerInterface.SendToServer(query);
	// 	return true;
	// }

	public void OnFillStateChanged(Vector3i position, float level)
	{
	}

    public void UpdateVisualState(Vector3i position, float state)
    {
        // Log.Out("Update Visual State for Composter {0}", state);
        if (GameManager.IsDedicatedServer) return;
		if (!(GameManager.Instance.World is World world)) return;
        if (!(world.GetChunkFromWorldPos(position) is Chunk chunk)) return;
        if (!(chunk.GetBlockEntity(position) is BlockEntityData entity)) return;
        if (!(entity.transform?.Find("Level") is Transform transform)) return;
		// Log.Out(" ===> Update OK");
        // Do the actual update once everything is OK
        Vector3 local = transform.localPosition;
        local.y = 0.55f * state / MaxFillState + 0.05f;
        transform.localPosition = local;
    }
}
