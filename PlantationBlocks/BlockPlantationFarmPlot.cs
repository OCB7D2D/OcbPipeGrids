using NodeManager;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlantationFarmPlot : BlockPipeReservoir
{

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddFarmPlot();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveFarmPlot();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}


}
