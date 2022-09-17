using PipeManager;

class BlockPipeWell : ImpBlockPipeReservoirUnpowered
{

	//########################################################
	// Implementation for block specialization
	//########################################################

	public float FromGround = 0.08f / 1000f;
	public float FromFreeSky = 0.25f / 1000f;
	public float FromWetSurface = 0.15f / 1000f;
	public float FromSnowfall = 0.4f / 1000f;
	public float FromRainfall = 0.8f / 1000f;
	public float FromIrrigation = 5f / 1000f;

	public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		if (Properties.Contains("FromGround")) FromGround =
			float.Parse(Properties.GetString("FromGround")) / 1000f;
		if (Properties.Contains("FromFreeSky")) FromFreeSky =
			float.Parse(Properties.GetString("FromFreeSky")) / 1000f;
		if (Properties.Contains("FromWetSurface")) FromWetSurface =
			float.Parse(Properties.GetString("FromWetSurface")) / 1000f;
		if (Properties.Contains("FromSnowfall")) FromSnowfall =
			float.Parse(Properties.GetString("FromSnowfall")) / 1000f;
		if (Properties.Contains("FromRainfall")) FromRainfall =
			float.Parse(Properties.GetString("FromRainfall")) / 1000f;
		if (Properties.Contains("FromIrrigation")) FromIrrigation =
			float.Parse(Properties.GetString("FromIrrigation")) / 1000f;
	}

	//########################################################
	// Update visual well fill state after activation
	//########################################################

	public override void OnBlockEntityTransformAfterActivated(
		WorldBase _world, Vector3i _blockPos, int _cIdx,
		BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(
			_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (_blockValue.ischild) return;
		//if (PipeGridManager.Instance.TryGetNode(
		//	_blockPos, out PipeGridWell well))
		//{
		//	well.UpdateBlockEntity(_ebcd);
		//}
	}


	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		//var action = new ActionAddWell();
		//action.Setup(position, bv, ConnectMask);
		//PipeGridInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		//var action = new ActionRemoveWell();
		//action.Setup(position);
		//PipeGridInterface.SendToServer(action);
	}

}
