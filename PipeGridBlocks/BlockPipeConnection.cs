// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeManager;

public interface IRotationLimitedBlock { }

public class BlockPipeConnection : ImpBlockGridNodeUnpowered, IRotationLimitedBlock
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeConnection;

	//########################################################
	//########################################################


	public override void Init()
    {
        base.Init();
		// Only allow simple rotations (avoid code errors)
		// AllowedRotations = EBlockRotationClasses.No45;
		// AllowedRotations &= ~EBlockRotationClasses.Advanced;
	}

}
