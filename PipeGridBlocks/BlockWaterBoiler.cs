// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeManager;

public class BlockWaterBoiler : BlockRemotePowered
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
		// Only allow simple rotations (avoid code errors)
		AllowedRotations = EBlockRotationClasses.No45;
		AllowedRotations &= ~EBlockRotationClasses.Advanced;
	}

}
