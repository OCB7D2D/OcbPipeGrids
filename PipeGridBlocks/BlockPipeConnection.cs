// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using PipeManager;

public class BlockPipeConnection : ImpBlockGridNodeUnpowered
{
	public override void CreateGridItem(Vector3i position, BlockValue bv) 
	{
		var action = new ActionAddConnection();
		action.Setup(position, bv);
		PipeGridInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
    {
		var action = new ActionRemoveConnection();
		action.Setup(position);
		PipeGridInterface.SendToServer(action);
	}

}
