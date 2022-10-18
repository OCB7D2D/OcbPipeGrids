using NodeManager;

class BlockPipePump : ImpBlockPipeReservoirPowered
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipePump;

	//########################################################
	//########################################################
	
	public override bool NeedsPower => true;


}
