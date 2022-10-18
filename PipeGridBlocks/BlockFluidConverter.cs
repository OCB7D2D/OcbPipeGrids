// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeManager;

public class BlockFluidConverter : ImpBlockGridNodeUnpowered
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeFluidConverter;

	//########################################################
	//########################################################

}
