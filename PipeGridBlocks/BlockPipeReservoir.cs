using NodeManager;

public class BlockPipeReservoir : ImpBlockPipeReservoirUnpowered, IRotationLimitedBlock
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeReservoir;

	//########################################################
	//########################################################

	// public override void CreateGridItem(Vector3i position, BlockValue bv)
	// {
	// 	var action = new ActionAddReservoir();
	// 	action.Setup(position, bv);
	// 	NodeManagerInterface.SendToServer(action);
	// }
	// 
	// public override void RemoveGridItem(Vector3i position)
	// {
	// 	var action = new ActionRemoveReservoir();
	// 	action.Setup(position);
	// 	NodeManagerInterface.SendToServer(action);
	// }

}
