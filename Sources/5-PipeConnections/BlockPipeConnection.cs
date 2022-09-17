// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeFacilitator;

public interface IRotationLimitedBlock { }
public interface IRotationSimpleBlock : IRotationLimitedBlock { }

public class BlockPipeConnection : ImpBlockGridNodeUnpowered, IRotationLimitedBlock
{

	//########################################################
	//########################################################

	public override TYPES NodeType => PipeConnection.NodeType;

	//########################################################
	//########################################################

}
