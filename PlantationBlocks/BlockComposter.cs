using NodeManager;
using System.Collections.Generic;
using UnityEngine;

public abstract class ImpBlockChest : BlockSecureLoot, ILootBlock, ITileEntityChangedListener
{
	Block IBlockNode.Block => this;

	public abstract void CreateGridItem(Vector3i blockPos, BlockValue blockValue);

	public abstract void RemoveGridItem(Vector3i blockPos);

	public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		if (world.GetTileEntity(chunk.ClrIdx, pos) is
			TileEntityLootContainer container)
		{
			if (!container.listeners.Contains(this))
			{
				Log.Out("++++ Added listener");
				container.listeners.Add(this);
			}
        }
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		if (bv.isair || bv.ischild) return;
		if (world.GetTileEntity(chunk.ClrIdx, pos) is
			TileEntityLootContainer container)
		{
			Log.Out("---- Removed listener");
			container.listeners.Remove(this);
		}
	}

    public override void OnBlockLoaded(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv)
    {
        base.OnBlockLoaded(world, clrIdx, pos, bv);
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
		{
			if (!container.listeners.Contains(this))
			{
				Log.Out("++++ Added listener");
				container.listeners.Add(this);
			}
		}
	}

	public override void OnBlockUnloaded(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv)
    {
        base.OnBlockUnloaded(world, clrIdx, pos, bv);
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
		{
			Log.Out("---- Removed listener");
			container.listeners.Remove(this);
		}
	}

	public void OnTileEntityChanged(TileEntity te, int dataObject)
    {
		Log.Warning("Tile Entity has changed {0} => {1}", te, dataObject);
		if (!(te is TileEntityLootContainer container)) return;
		var action = new ActionUpdateChest();
		action.Setup(te.ToWorldPos(), container.GetItems());
		NodeManagerInterface.Instance.ToWorker.Enqueue(action);
	}
}

public class BlockComposter : ImpBlockChest, ILootBlock, IBoundHelper
{

	//########################################################
	// Implementation for block specialization
	//########################################################




	// Sync the state with worker manager
	// When loading sync from manager to TE
	// When loaded sync after TE is unlocked
	// When unloading, sync again just to be sure
	// So the manager has a copy of all items in chest!?

	Block IBlockNode.Block => this;

	public int BlockReach { get; protected set; }

	public override void Init()
    {
        base.Init();
		// Parse optional block XML setting properties
		if (Properties.Contains("BlockReach")) BlockReach =
			int.Parse(Properties.GetString("BlockReach"));
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

	private Color BoundHelperColor = new Color32(160, 82, 45, 255);

	public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
		BlockHelper.UpdateBoundHelper(pos, bv_new,
			this, BoundHelperColor, BlockReach);
	}

	//########################################################
	// Implementation for node manager
	//########################################################


	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddComposter();
		Log.Out("Create Composter Item to send to server");
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveComposter();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

	public override ulong GetTickRate() => 5; // (ulong)(growthRate * 20.0 * 60.0);

	private static bool IsLoaded(WorldBase world, Vector3i position)
		=> world.GetChunkFromWorldPos(position) != null;
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

    public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		BlockHelper.UpdateBoundHelper(pos, bv,
			this, BoundHelperColor, BlockReach);
		if (!NodeManagerInterface.HasServer) return;
		PipeBlockHelper.OnBlockAdded(this, pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockRemoved(world, chunk, pos, bv);
		if (bv.isair || bv.ischild) return;
		LandClaimBoundsHelper.RemoveBoundsHelper(pos);
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
		LandClaimBoundsHelper.RemoveBoundsHelper(pos);
		base.OnBlockUnloaded(world, clrIdx, pos, bv);
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
		BlockHelper.UpdateBoundHelper(pos, bv,
			this, BoundHelperColor, BlockReach);
		// Apply pending changes and check if valid!
		if (world.GetTileEntity(clrIdx, pos) is
			TileEntityLootContainer container)
		{
			Log.Warning("Loaded ok, with TE");
			if (ChestChanges.TryGetValue(pos,
				out Dictionary<ushort, short> stack))
			{
				ExecuteChestModification
					.ApplyChangesToChest(
						stack, container);
			}
			// Apply all changes
			//if (ChestChanges.TryGetValue(pos,
			//	out List<ItemStack> stack))
			//{
			//    for (int i = 0; i < stack.Count; i++)
			//    {
			//        while (stack[i].count < 0)
			//        {
			//			if (container.HasItem(stack[i].itemValue))
			//				container.RemoveItem(stack[i].itemValue);
			//			else break; // Abort if not possible
			//			stack[i].count += 1;
			//		}
			//		while (stack[i].count > 0)
			//		{
			//			if (container.TryStackItem(0, stack[i]))
			//				stack[i].count = 0;
			//			else break;
			//		}
			//	}
			//}
		}
		else
        {
			Log.Warning("Loaded without TE");
        }
	}

	}
