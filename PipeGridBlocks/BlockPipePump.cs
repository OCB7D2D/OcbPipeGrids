using PipeManager;

class BlockPipePump : ImpBlockPipeReservoirPowered
{

    public override bool NeedsPower => true;

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddPump();
		action.Setup(position, bv, ConnectMask);
		PipeGridInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemovePump();
		action.Setup(position);
		PipeGridInterface.SendToServer(action);
	}

}
