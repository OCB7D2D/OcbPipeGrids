using NodeManager;

public class BlockPlantationFarmPlot : BlockPipeReservoir
{

	//########################################################
	//########################################################

	public MaintenanceOptions WaterMaintenance = new
		MaintenanceOptions(0.01f, 1.25f, 0.1f);
	public MaintenanceOptions SoilMaintenance = new
		MaintenanceOptions(0.01f, 1.25f, 0.1f);

	public RangeOptions WaterRange = new RangeOptions(0.02f, 3f);
	public RangeOptions SoilRange = new RangeOptions(0.03f, 5f);

	//########################################################
	// Parse custom properties on init
	//########################################################

	public override void Init()
	{
		base.Init();
		// Initialize maintenance options
		SoilMaintenance.Init(Properties, "Soil");
		WaterMaintenance.Init(Properties, "Water");
	}

	//########################################################
	// Implementation for Grid Manager
	//########################################################

	// public override void CreateGridItem(Vector3i position, BlockValue bv)
	// {
	// 	var action = new ActionAddFarmPlot();
	// 	action.Setup(position, bv);
	// 	NodeManagerInterface.SendToServer(action);
	// }
	// 
	// public override void RemoveGridItem(Vector3i position)
	// {
	// 	var action = new ActionRemoveFarmPlot();
	// 	action.Setup(position);
	// 	NodeManagerInterface.SendToServer(action);
	// }

	//########################################################
	//########################################################

}
