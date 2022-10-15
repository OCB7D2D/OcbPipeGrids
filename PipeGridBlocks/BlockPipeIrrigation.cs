using NodeManager;
using UnityEngine;

class BlockPipeIrrigation : ImpBlockGridNodePowered, IReacherBlock
{

	public IBlockNode IBLK => this;

	public Vector3i BlockReach { get; set; } = Vector3i.zero;
	public Vector3i ReachOffset { get; set; } = Vector3i.zero;
	public Color BoundHelperColor { get; set; } = new Color32(160, 82, 45, 255);
	public Color ReachHelperColor { get; set; } = new Color32(160, 82, 45, 255);

	public override bool BreakDistance => true;
	public override bool NeedsPower => true;

	public override void Init()
	{
		base.Init();
		// Parse block XML properties
		ReachHelper.InitBlock(this);
	}

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
