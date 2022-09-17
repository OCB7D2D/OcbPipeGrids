// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeFacilitator;

public class BlockWaterBoiler : BlockRemoteDescPowered, IRotationSimpleBlock
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeWaterBoiler;

	//########################################################
	//########################################################

	public Vector3i InputPosition;
	public Vector3i OutputPosition;


	public override void Init()
    {
        base.Init();
		// Parse optional block XML setting properties
		if (Properties.Contains("InputPosition")) InputPosition = StringParsers
				.ParseVector3i(Properties.GetString("InputPosition"));
		if (Properties.Contains("OutputPosition")) OutputPosition = StringParsers
				.ParseVector3i(Properties.GetString("OutputPosition"));
	}

}
