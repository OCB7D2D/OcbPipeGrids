using NodeManager;

public class BlockPlantationFarmPlot : BlockPipeReservoir
{

	public override TYPES NodeType => TYPES.PlantationFarmPlot;

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
	//########################################################

}
