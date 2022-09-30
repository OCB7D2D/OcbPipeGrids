// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeManager;

public class BlockFluidConverter : ImpBlockGridNodeUnpowered
{
	public override void CreateGridItem(Vector3i position, BlockValue bv) 
	{
		var action = new ActionAddFluidConverter();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
    {
		var action = new ActionRemoveFluidConverter();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

}
