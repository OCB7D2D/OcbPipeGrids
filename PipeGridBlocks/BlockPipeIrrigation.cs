﻿using PipeManager;

class BlockPipeIrrigation : ImpBlockGridNodeUnpowered
{

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddIrrigation();
		action.Setup(position, bv.rotation, ConnectMask);
		PipeGridInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveIrrigation();
		action.Setup(position);
		PipeGridInterface.SendToServer(action);
	}

}
