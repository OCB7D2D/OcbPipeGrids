using NodeManager;
using System;
using UnityEngine;

public class BlockPipeWell : ImpBlockPipeReservoirUnpowered
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
	public float MaxWaterLevel = 150f;

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
		if (Properties.Contains("MaxWaterLevel")) FromIrrigation =
			float.Parse(Properties.GetString("MaxWaterLevel"));
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

	public void UpdateBlockEntity(BlockEntityData entity, float level)
	{
		if (entity == null) return;
		if (entity.transform == null) return;
		if (entity.transform.Find("WaterLevel") is Transform transform)
		{
			Vector3 position = transform.localPosition;
			position.y = 0.33f / MaxWaterLevel * level - 0.49f;
			//Log.Out("Update water level to {0}", position.y);
			transform.localPosition = position;
		}
	}

	public void OnWaterLevelChanged(Vector3i position, float level)
	{
		if (GameManager.Instance.World is WorldBase world)
		{
			if (world.GetChunkFromWorldPos(position) is Chunk chunk)
			{
				UpdateBlockEntity(chunk.GetBlockEntity(position), level);
			}
		}
	}

	public override void CreateGridItem(Vector3i position, BlockValue bv)
	{
		Log.Out("Create Well item");
		var action = new ActionAddWell();
		action.Setup(position, bv);
		NodeManagerInterface.SendToServer(action);
	}

	public override void RemoveGridItem(Vector3i position)
	{
		var action = new ActionRemoveWell();
		action.Setup(position);
		NodeManagerInterface.SendToServer(action);
	}


	public override void OnBlockAdded(
	  WorldBase _world,
	  Chunk _chunk,
	  Vector3i _blockPos,
	  BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_world.IsRemote()) return; // Only execute on server
		_world.GetWBT().AddScheduledBlockUpdate(
			_chunk.ClrIdx, _blockPos,
			blockID, GetTickRate());
	}

	double RefreshRate = 0.005;

	public override ulong GetTickRate() => (ulong)(RefreshRate * 20.0 * 60.0);

	public override bool UpdateTick(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue,
		bool _bRandomTick,
		ulong _ticksIfLoaded,
		GameRandom _rnd)
	{
		_world.GetWBT().AddScheduledBlockUpdate(
			_clrIdx, _blockPos, blockID, GetTickRate());
		var query = new MsgWaterLevelQuery();
		query.Setup(_blockPos);
		query.AddWater = 0f;
		var weather = WeatherManager.Instance;
		query.AddWater += FromWetSurface * weather.GetCurrentWetSurfaceValue();
		query.AddWater += FromSnowfall * weather.GetCurrentSnowfallValue();
		query.AddWater += FromRainfall * weather.GetCurrentRainfallValue();
		NodeManagerInterface.SendToServer(query);
		return true;
	}

}
