using NodeFacilitator;

class BlockPump : ImpBlockPipeReservoirPowered
{

	//########################################################
	//########################################################

	public override TYPES NodeType => PipePump.NodeType;

	//########################################################
	//########################################################
	
	public override bool NeedsPower => true;


}
