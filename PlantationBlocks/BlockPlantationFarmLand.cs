using NodeManager;

public class BlockPlantationFarmLand : BlockRemote
{

    //########################################################
    // Start copy of custom textures code
    //########################################################

    // Parsed blend settings for this block
    private CustomTerrain.Blend Blending
        = new CustomTerrain.Blend();

    // Assign a fantasy id to block blend
    private int VirtualID = -1;

    // Parse custom properties on init
    public override void Init()
    {
        base.Init();
        // Initialize terrain blend once
        VirtualID = CustomTerrain.
            Init(this, ref Blending);
    }
    
    //########################################################
    // Implementation for Grid Manager
    //########################################################

    public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		var action = new ActionAddFarmLand();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveFarmLand();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}


}
