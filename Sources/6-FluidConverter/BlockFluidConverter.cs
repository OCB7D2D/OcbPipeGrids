// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeFacilitator;

public class BlockFluidConverter : ImpBlockPipeReservoirPowered, IBlockExchangeItems, IRotationSimpleBlock
{

	//########################################################
	//########################################################
	public override TYPES NodeType => PipeFluidConverter.NodeType;

    //########################################################
    //########################################################
    /*
    public Vector3i OutputPosition1;
    public Vector3i OutputPosition2;

    public override void Init()
    {
        base.Init();
        // Parse optional block XML setting properties
        if (Properties.Contains("OutputPosition1")) OutputPosition1 = StringParsers
                .ParseVector3i(Properties.GetString("OutputPosition1"));
        if (Properties.Contains("OutputPosition2")) OutputPosition2 = StringParsers
                .ParseVector3i(Properties.GetString("OutputPosition2"));
    }
    */

    public bool AcceptItem(ItemClass item)
    {
        string name = item.GetItemName();
        return name == "resourcePesticide"
            || name == "resourceCompost";
    }

}
