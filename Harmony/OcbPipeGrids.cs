using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NodeFacilitator;
using Unity.Collections;

public class OcbPipeGrids : IModApi
{

	bool Registered = false;

	// Entry class for A20 patching
	public void InitMod(Mod mod)
	{
		Log.Out("Loading OCB Pipe Grids Patch: " + GetType().ToString());
		var harmony = new Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());

		ModEvents.GameStartDone.RegisterHandler(GameStartDone);
		ModEvents.GameShutdown.RegisterHandler(GameShutdown);
		ModEvents.GameUpdate.RegisterHandler(GameUpdate);

		// ReflectionHelpers.FindTypesImplementingBase(typeof(NetPackage), (System.Action<System.Type>)(_type => Log.Out("Package {0} vs {1}", _type.Name, _type)));

		NodeFacilitator.NodeManager.RegisterFactory((uint)PipeConnection.NodeType, (br) => new PipeConnection(br), (pos, bv) => new PipeConnection(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PipePump.NodeType, (br) => new PipePump(br), (pos, bv) => new PipePump(pos, bv));
        NodeFacilitator.NodeManager.RegisterFactory((uint)PipeDrain.NodeType, (br) => new PipeDrain(br), (pos, bv) => new PipeDrain(pos, bv));
        NodeFacilitator.NodeManager.RegisterFactory((uint)PipeTank.NodeType, (br) => new PipeTank(br), (pos, bv) => new PipeTank(pos, bv));
        NodeFacilitator.NodeManager.RegisterFactory((uint)PipeIrrigation.NodeType, (br) => new PipeIrrigation(br), (pos, bv) => new PipeIrrigation(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PipeSource.NodeType, (br) => new PipeSource(br), (pos, bv) => new PipeSource(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PipeWaterBoiler.NodeType, (br) => new PipeWaterBoiler(br), (pos, bv) => new PipeWaterBoiler(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PipeFluidConverter.NodeType, (br) => new PipeFluidConverter(br), (pos, bv) => new PipeFluidConverter(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PipeReservoir.NodeType, (br) => new PipeReservoir(br), (pos, bv) => new PipeReservoir(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PipeFluidInjector.NodeType, (br) => new PipeFluidInjector(br), (pos, bv) => new PipeFluidInjector(pos, bv));
        NodeFacilitator.NodeManager.RegisterFactory((uint)PlantationSprinkler.NodeType, (br) => new PlantationSprinkler(br), (pos, bv) => new PlantationSprinkler(pos, bv));
        NodeFacilitator.NodeManager.RegisterFactory((uint)SprinklerIndoor.NodeType, (br) => new SprinklerIndoor(br), (pos, bv) => new SprinklerIndoor(pos, bv));
      

        NodeFacilitator.NodeManager.RegisterFactory((uint)FarmWell.NodeType, (br) => new FarmWell(br), (pos, bv) => new FarmWell(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PlantationFarmLand.NodeType, (br) => new PlantationFarmLand(br), (pos, bv) => new PlantationFarmLand(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PlantationFarmPlot.NodeType, (br) => new PlantationFarmPlot(br), (pos, bv) => new PlantationFarmPlot(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PlantationGrowing.NodeType, (br) => new PlantationGrowing(br), (pos, bv) => new PlantationGrowing(pos, bv));
		NodeFacilitator.NodeManager.RegisterFactory((uint)PlantationComposter.NodeType, (br) => new PlantationComposter(br), (pos, bv) => new PlantationComposter(pos, bv));
        NodeFacilitator.NodeManager.RegisterFactory((uint)PlantationGrowLight.NodeType, (br) => new PlantationGrowLight(br), (pos, bv) => new PlantationGrowLight(pos, bv));

    }

    public void GameStartDone()
	{
		if (Registered) return;
		Registered = true;


		Log.Warning("WellToSoilReach => {0}", NodeFacilitator.NodeManager.WellToSoilReach);
		Log.Warning("GrowLightToPlantReach => {0}", NodeFacilitator.NodeManager.GrowLightToPlantReach);
		Log.Warning("ComposterToSoilReach => {0}", NodeFacilitator.NodeManager.ComposterToSoilReach);
		Log.Warning("IrrigatorToWellReach => {0}", NodeFacilitator.NodeManager.IrrigatorToWellReach);
		Log.Warning("SprinklerToSoilReach => {0}", NodeFacilitator.NodeManager.SprinklerToSoilReach);

	}

	public void GameShutdown()
	{
		if (!Registered) return;
		CustomTerrain.GameShutdown();
		Registered = false;
	}

	private void GameUpdate()
    {
		if (!NodeManagerInterface.HasInstance) return;
		NodeManagerInterface.Instance.Update();
    }

	/*
	[HarmonyPatch(typeof(BlockCampfire))]
	[HarmonyPatch("checkParticles")]
	public class checkParticles
	{
		static bool Prefix()
		{
			return false;
		}
	}
	*/


	[HarmonyPatch(typeof(BlockPlacement))]
	[HarmonyPatch("supports45DegreeRotations")]
	public class supports45DegreeRotations
	{
		static void Postfix(BlockValue _bv, ref bool __result)
		{
			Block block = _bv.Block;
			if (block is IRotationLimitedBlock)
            {
				__result = (block.AllowedRotations & EBlockRotationClasses.Basic45) > 0;
				// Log.Out("Support 45 {0} {1}", __result, block.shape);
			}


			
			// Log.Out("+++++++++ Enable Particle and register it");
			// Create the instance and start it
			// Will make sure to only create server
			// and client parts only when necessary 
			// Log.Warning("Added Ticker At {0}",
			// 	_wbte.scheduledTime);
			// NodeManagerInterface.Instance.Init();
		}
	}

	
	[HarmonyPatch(typeof(OriginParticles))]
	[HarmonyPatch("OnEnable")]
	public class RepositionParticlesOnEnable
	{
		static void Prefix()
		{




			// Log.Out("+++++++++ Enable Particle and register it");
			// Create the instance and start it
			// Will make sure to only create server
			// and client parts only when necessary 
			// Log.Warning("Added Ticker At {0}",
			// 	_wbte.scheduledTime);
			// NodeManagerInterface.Instance.Init();
		}
	}
	

	[HarmonyPatch(typeof(Origin))]
	[HarmonyPatch("RepositionParticles")]
	public class RepositionParticles
	{
		static bool Prefix(Vector3 _deltaV)
		{
			return true;
			/*
			Log.Out("---------- Reposition particles is called");


			for (int index1 = Origin.particleSystemTs.Count - 1; index1 >= 0; --index1)
			{
				Transform particleSystemT = Origin.particleSystemTs[index1];
				if (!(bool)(UnityEngine.Object)particleSystemT)
				{
					Origin.particleSystemTs.RemoveAt(index1);
				}
				else
				{
					Log.Out("!!!!!!!!! Reposition {0}", particleSystemT);
					ParticleSystem[] componentsInChildren = particleSystemT.GetComponentsInChildren<ParticleSystem>();
					for (int index2 = componentsInChildren.Length - 1; index2 >= 0; --index2)
					{
						ParticleSystem particleSystem = componentsInChildren[index2];
						Log.Out("     ===!!!!!!!!! Repositioned {0} {1}", particleSystem.isPlaying, particleSystem.main.simulationSpace);
							// particleSystem.transform.position
						if (particleSystem.isPlaying && particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World)
						{
							var array = new NativeArray<ParticleSystem.Particle>(512, Allocator.Persistent);
							int particles = particleSystem.GetParticles(array);
							for (int index3 = 0; index3 < particles; ++index3)
							{
								ParticleSystem.Particle particle = array[index3];
								Log.Out("        !!!!!!!!! Repositioned {0}", particle);
								particle.position += _deltaV;
								array[index3] = particle;
							}
							particleSystem.SetParticles(array, particles);
							particleSystem.Simulate(0.0f, false, false);
							particleSystem.Play(false);
						}
					}
				}
			}
			return false;

			foreach (var asd in Origin.particleSystemTs)
            {
				// Log.Out("!!!!!!!!! Reposition {0}", asd);
            }
			*/
			// Create the instance and start it
			// Will make sure to only create server
			// and client parts only when necessary 
			// Log.Warning("Added Ticker At {0}",
			// 	_wbte.scheduledTime);
			// NodeManagerInterface.Instance.Init();
		}
	}

	
		/*
	// Hook into `VehicleManager.Init`
	[HarmonyPatch(typeof(WorldBlockTicker))]
	[HarmonyPatch("add")]
	public class WorldBlockTickerAdd
	{
		static void Prefix(WorldBlockTickerEntry _wbte)
		{
			// Create the instance and start it
			// Will make sure to only create server
			// and client parts only when necessary 
			// Log.Warning("Added Ticker At {0}",
			// 	_wbte.scheduledTime);
			// NodeManagerInterface.Instance.Init();
		}
	}

	// Hook into `VehicleManager.Init`
	[HarmonyPatch(typeof(WorldBlockTicker))]
	[HarmonyPatch("execute")]
	public class WorldBlockTickerExec
	{
		static void Prefix(World ___world, WorldBlockTickerEntry _wbte, GameRandom _rnd, ulong _ticksIfLoaded)
		{
			// Create the instance and start it
			// Will make sure to only create server
			// and client parts only when necessary 
			Log.Warning("Execute Ticker At {0} => {1}",
				_wbte.scheduledTime, _ticksIfLoaded);
			BlockValue block = ___world.GetBlock(_wbte.clrIdx, _wbte.worldPos);
			Log.Warning(" ==== {0} vs {1}", block.type, _wbte.blockID);	


			// NodeManagerInterface.Instance.Init();
		}
	}
*/
		// Hook into `VehicleManager.Init`



	// Hook into `VehicleManager.Init`
	[HarmonyPatch(typeof(VehicleManager))]
	[HarmonyPatch("Init")]
	public class PipeGridManagerInit
	{
		static void Prefix()
		{
			// Create the instance and start it
			// Will make sure to only create server
			// and client parts only when necessary 
			NodeManagerInterface.Instance.Init();
		}
	}

	// Hook `GameManager.SaveAndCleanupWorld`
	// We want to run in the middle, so we just
	// piggy-back another call we know is there
	[HarmonyPatch(typeof(VehicleManager))]
    [HarmonyPatch("Cleanup")]
    public class PipeGridManagerCleanup
	{
        static void Prefix()
        {
			if (!NodeManagerInterface.HasInstance) return;
			if (ConnectionManager.Instance.IsServer)
				NodeManagerInterface.Instance.Cleanup();
        }
    }

	// Give users some feedback if placing is blocked by missing
	// info from the PipeGridManager (applies for all clients)
	[HarmonyPatch(typeof(RenderDisplacedCube))]
	[HarmonyPatch("update0")]
	public class RenderDisplacedCube_Update
	{
		public static void Postfix(RenderDisplacedCube __instance,
			BlockValue _holdingBlockValue, Vector3i _focusBlockPos,
			Transform ___transformFocusCubePrefab)
		{
			if (NodeManagerInterface.Instance.Client.HasPendingCanConnect(_focusBlockPos))
            {
				foreach (Renderer child in ___transformFocusCubePrefab.GetComponentsInChildren<Renderer>())
					child.material.SetColor("_Color", Color.magenta);
			}
			if (_holdingBlockValue.Block is IBoundHelper helper)
            {
				var scale = ___transformFocusCubePrefab.localScale;
				//scale.x += helper.BlockReach.x * 5.08f;
				//scale.z += helper.BlockReach.z * 5.08f;
				___transformFocusCubePrefab.localScale = scale;
				// ___transformFocusCubePrefab.localScale +=
				// 	Vector3.one * helper.BlockReach * 5.08f;
			}
		}
	}

}
