using NodeManager;
using System.Collections.Generic;
using UnityEngine;


public class BlockComposter : ImpBlockChest, ILootBlock, IBoundHelper, IReacherBlock
{

	//########################################################
	//########################################################

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

	public IBlockNode IBLK => this;
	public IReacherBlock RBLK => this;

	public Vector3i BlockReach { get; set; } = Vector3i.zero;
	public Vector3i ReachOffset { get; set; } = Vector3i.zero;
	public Color BoundHelperColor { get; set; } = new Color32(160, 82, 45, 255);
	public Color ReachHelperColor { get; set; } = new Color32(160, 82, 45, 255);

	public override void Init()
    {
        base.Init();
		// Parse block XML properties
		BlockConfig.InitReacher(this);
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

	public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new, this, this);
	}

	//########################################################
	// Implementation for node manager
	//########################################################


	// public override void CreateGridItem(Vector3i position, BlockValue bv)
	// {
	// 	var action = new ActionAddComposter();
	// 	action.Setup(position, bv);
	// 	Log.Warning("Creating new composter {0}", bv.type);
	// 	NodeManagerInterface.SendToServer(action);
	// }
	// 
	// public override void RemoveGridItem(Vector3i position)
	// {
	// 	var action = new ActionRemoveComposter();
	// 	action.Setup(position);
	// 	NodeManagerInterface.SendToServer(action);
	// }

	public override ulong GetTickRate() => 5; // (ulong)(growthRate * 20.0 * 60.0);

	private static bool IsLoaded(WorldBase world, Vector3i position)
		=> world.GetChunkFromWorldPos(position) != null;
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

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
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
		if (bv.ischild) return; // Only register master
		if (world.IsRemote()) return; // Only execute on server
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
        {
			var action = new ActionUpdateChest();
			action.Setup(pos, container.GetItems());
			NodeManagerInterface.Instance
				.ToWorker.Enqueue(action);
        }
	}

	public override void OnBlockLoaded(WorldBase world, int clrIdx, Vector3i pos, BlockValue bv)
	{
		base.OnBlockLoaded(world, clrIdx, pos, bv);
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
				.ToWorker.Enqueue(action);

		}
		else
        {
			Log.Warning("Loaded without TE {0}", pos);
        }
	}

}
