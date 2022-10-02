﻿using NodeManager;

class BlockPipeIrrigation : ImpBlockGridNodePowered
{

	public override bool BreakDistance => true;
	public override bool NeedsPower => true;

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddIrrigation();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveIrrigation();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}

}
