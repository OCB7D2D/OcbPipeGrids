using NodeFacilitator;
using System.Collections.Generic;


public class BlockFluidInjector : ImpBlockChest, ILootBlock, IBlockReservoir
{

	//########################################################
	//########################################################

	public override TYPES NodeType => PipeFluidInjector.NodeType;

	//########################################################
	//########################################################
	
	// *******************************************************
	// IBlockReservoir
	// *******************************************************

	public virtual float MaxFillState { get; set; } = 150f;
	public ushort FluidType { get; set; }

	// *******************************************************
	// IBlockConnection
	// *******************************************************

	public virtual bool BreakSegment { get; set; } = false;
	public virtual bool NeedsPower => false;
	public virtual byte ConnectMask { get; set; } = 63;
	public virtual uint SideMask { get; set; } = 0;
	public virtual byte PipeDiameter { get; set; } = 0;
	public virtual int MaxConnections { get; set; } = 6;

	public byte ConnectFlags => (byte)(BreakSegment ? ConnectorFlag.Breaker : ConnectorFlag.None);

	public virtual bool CanConnect(byte side, byte rotation)
		=> NodeBlockHelper.CanConnect(ConnectMask, side, rotation);

	public override void Init()
	{
		base.Init();
		// Call helper implementation
		// Parses `IBlockPipeConnection`
		BlockConfig.InitConnection(this);
	}

	// *******************************************************
	// Shared implementation for all `IBlockPipeNode`
	// *******************************************************

	// Reuse object to pass around as parameter to `CanConnect`
	// Not sure much this brings; less allocations always good!?
	private static BlockConnector BCC = new BlockConnector();

	public override bool CanPlaceBlockAt(
		WorldBase world, int clrIdx,
		Vector3i pos, BlockValue bv,
		// Ignore existing blocks?
		bool omitCollideCheck = false)
	{
		if (bv.ischild) return false; // ToDo: add more fancy code for multidims?
		return base.CanPlaceBlockAt(world, clrIdx, pos, bv, omitCollideCheck)
			&& NodeManagerInterface.Instance.Client.CanConnect(pos, BCC.Set(bv, this));
	}


	//########################################################
	// Implementation for block specialization
	//########################################################

	// Sync the state with worker manager
	// When loading sync from manager to TE
	// When loaded sync after TE is unlocked
	// When unloading, sync again just to be sure
	// So the manager has a copy of all items in chest!?

	public IBlockNode IBLK => this;

	public BlockFluidInjector()
	{
		this.IsNotifyOnLoadUnload = true;
        BlockHelper.ExtendActivationCommands(this,
            typeof(ImpBlockChest),
            ref cmds, ref cmd_offset);
    }

    public override void OnBlockValueChanged(WorldBase world, Chunk chunk,
		int clrIdx, Vector3i pos, BlockValue bv_old, BlockValue bv_new)
	{
		base.OnBlockValueChanged(world, chunk, clrIdx, pos, bv_old, bv_new);
	}

	//########################################################
	// Implementation for node manager
	//########################################################

	public override ulong GetTickRate() => 5; // (ulong)(growthRate * 20.0 * 60.0);

	private static bool IsLoaded(WorldBase world, Vector3i position)
		=> world.GetChunkFromWorldPos(position) != null;
	
	public static int ToHashCode(int _clrIdx, Vector3i _pos) => 31 * (31 * (31 * _clrIdx + _pos.x) + _pos.y) + _pos.z;

    public override void OnBlockAdded(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
		base.OnBlockAdded(world, chunk, pos, bv);
		if (!NodeManagerInterface.HasServer) return;
		NodeBlockHelper.OnBlockAdded(this, pos, bv);
	}

	public override void OnBlockRemoved(
		WorldBase world, Chunk chunk,
		Vector3i pos, BlockValue bv)
	{
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



    //########################################################
    // Add functionality for additional commands
    //########################################################

    private BlockActivationCommand[] cmds = new BlockActivationCommand[1] {
        new BlockActivationCommand("remove", "trash", true),
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
        // Send reset message to worker
        var msg = new MsgNodeReset();
        msg.Setup(pos); // Just pass position
        NodeManagerInterface.SendToWorker(msg);
        // All is good
        return true;
    }
*/

}
