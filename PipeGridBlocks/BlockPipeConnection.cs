// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeManager;

public interface IRotationLimitedBlock { }

public class BlockPipeConnection : ImpBlockGridNodeUnpowered, IRotationLimitedBlock
{
	public override void Init()
    {
        base.Init();
		// Only allow simple rotations (avoid code errors)
		// AllowedRotations = EBlockRotationClasses.No45;
		// AllowedRotations &= ~EBlockRotationClasses.Advanced;
	}

	public override void CreateGridItem(Vector3i position, BlockValue bv) 
	{
		var action = new ActionAddConnection();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
    {
		var action = new ActionRemoveConnection();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

}
