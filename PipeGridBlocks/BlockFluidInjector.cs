using NodeManager;
using System.Collections.Generic;
using UnityEngine;


public class BlockFluidInjector : ImpBlockChest, ILootBlock, IBlockReservoir
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeFluidInjector;

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

	public virtual bool BreakDistance => false;
	public virtual bool NeedsPower => false;
	public virtual byte ConnectMask { get; set; } = 63;
	public virtual uint SideMask { get; set; } = 0;
	public virtual bool MultiBlockPipe { get; set; } = false;
	public virtual byte PipeDiameter { get; set; } = 0;
	public virtual int MaxConnections { get; set; } = 6;

	public byte ConnectFlags => (byte)(BreakDistance ? ConnectorFlag.Breaker : ConnectorFlag.None);

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
			&& NodeManagerInterface.Instance.Mother.CanConnect(pos, BCC.Set(bv, this));
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
