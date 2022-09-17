using PipeManager;

class BlockPipeSource : ImpBlockPipeReservoirPowered
{

    public override bool BreakDistance => true;
    public override bool NeedsPower => true;

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddSource();
		action.Setup(position, bv, ConnectMask);
		PipeGridInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveSource();
		action.Setup(position);
		PipeGridInterface.SendToServer(action);
	}

}
