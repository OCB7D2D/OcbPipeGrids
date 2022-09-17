using NodeFacilitator;

class BlockPipeDrain : ImpBlockPipeReservoirPowered
{

	//########################################################
	//########################################################

	public override TYPES NodeType => PipeDrain.NodeType;

	//########################################################
	//########################################################
	
	public override bool NeedsPower => true;


}
