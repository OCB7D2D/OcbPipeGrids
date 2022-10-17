using NodeManager;
using UnityEngine;

public class BlockPlantationSprinkler : BlockRemote, IReacherBlock
{

    //########################################################
    //########################################################

    public Vector3i BlockReach { get; set; } = Vector3i.zero;
    public Vector3i ReachOffset { get; set; } = Vector3i.zero;
    public Color BoundHelperColor { get; set; } = new Color32(160, 82, 45, 255);
    public Color ReachHelperColor { get; set; } = new Color32(160, 82, 45, 255);

    //########################################################
    //########################################################

    // public MaintenanceOptions WaterMaintenance = new
    //     MaintenanceOptions(0.01f, 1.25f, 0.1f);
    // public MaintenanceOptions SoilMaintenance = new
    //     MaintenanceOptions(0.01f, 1.25f, 0.1f);
    // 
    // public RangeOptions WaterRange = new RangeOptions(0.02f, 2f);
    // public RangeOptions SoilRange = new RangeOptions(0.03f, 3f);

    //########################################################
    // Parse custom properties on init
    //########################################################

    public override void Init()
    {
        base.Init();
        // Initialize maintenance options
        // SoilMaintenance.Init(Properties, "Soil");
        // WaterMaintenance.Init(Properties, "Water");
    }
    
    //########################################################
    //########################################################

}
