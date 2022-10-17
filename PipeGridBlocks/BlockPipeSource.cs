using NodeManager;

class BlockPipeSource : ImpBlockPipeReservoirPowered
{

    //########################################################
    //########################################################

    public override TYPES NodeType => TYPES.PipeSouce;

    //########################################################
    //########################################################

    public override bool BreakDistance => true;
    public override bool NeedsPower => true;

    //########################################################
    //########################################################

}
