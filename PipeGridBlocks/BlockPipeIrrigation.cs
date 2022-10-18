using NodeManager;
using UnityEngine;

class BlockPipeIrrigation : ImpBlockGridNodePowered, IReacherBlock
{

	//########################################################
	//########################################################

	public override TYPES NodeType => TYPES.PipeIrrigation;

	//########################################################
	//########################################################

	public IBlockNode IBLK => this;
	public IReacherBlock RBLK => this;

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
		BlockConfig.InitReacher(this);
	}

}
