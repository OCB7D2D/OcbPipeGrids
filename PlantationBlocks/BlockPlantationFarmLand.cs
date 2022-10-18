using NodeManager;

public class BlockPlantationFarmLand : BlockRemote
{

    //########################################################
    //########################################################

    public override TYPES NodeType => TYPES.PlantationFarmLand;

    //########################################################
    //########################################################

    public MaintenanceOptions WaterMaintenance = new
        MaintenanceOptions(0.01f, 1.25f, 0.1f);
    public MaintenanceOptions SoilMaintenance = new
        MaintenanceOptions(0.01f, 1.25f, 0.1f);

    public RangeOptions WaterRange = new RangeOptions(0.02f, 2f);
    public RangeOptions SoilRange = new RangeOptions(0.03f, 3f);

    //########################################################
    // Start copy of custom textures code
    //########################################################

    // Parsed blend settings for this block
    private CustomTerrain.Blend Blending
        = new CustomTerrain.Blend();

    // Assign a fantasy id to block blend
    private int VirtualID = -1;

    //########################################################
    // Parse custom properties on init
    //########################################################

    public override void Init()
    {
        base.Init();
        // Initialize maintenance options
        SoilMaintenance.Init(Properties, "Soil");
        WaterMaintenance.Init(Properties, "Water");
        // Initialize terrain blend once
        VirtualID = CustomTerrain.
            Init(this, ref Blending);
    }

    //########################################################
    //########################################################

}
