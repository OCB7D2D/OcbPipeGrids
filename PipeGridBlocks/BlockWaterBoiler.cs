// Basic connection block for pipe grids, e.g. simple pipes
// All other block will probably want to inherit from this
using NodeManager;

public class BlockWaterBoiler : BlockRemotePowered
{

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

	public override void CreateGridItem(Vector3i position, BlockValue bv) 
	{
		var action = new ActionAddWaterBoiler();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
    {
		var action = new ActionRemoveWaterBoiler();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

}
