using NodeFacilitator;

class BlockPipeSource : ImpBlockPipeReservoirPowered
{

    //########################################################
    //########################################################

    public override TYPES NodeType => PipeSource.NodeType;

    //########################################################
    //########################################################

    public override bool BreakSegment => true;
    public override bool NeedsPower => true;

    //########################################################
    //########################################################

}
