using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NodeManager;
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

		ReflectionHelpers.FindTypesImplementingBase(typeof(NetPackage), (System.Action<System.Type>)(_type => Log.Out("Package {0} vs {1}", _type.Name, _type)));

		NodeManager.NodeManager.RegisterFactory(1, (br) => new PipeConnection(br), (pos, bv) => new PipeConnection(pos, bv));
		NodeManager.NodeManager.RegisterFactory(2, (br) => new PipePump(br), (pos, bv) => new PipePump(pos, bv));
		NodeManager.NodeManager.RegisterFactory(3, (br) => new PipeIrrigation(br), (pos, bv) => new PipeIrrigation(pos, bv));
		NodeManager.NodeManager.RegisterFactory(4, (br) => new PipeSource(br), (pos, bv) => new PipeSource(pos, bv));
		NodeManager.NodeManager.RegisterFactory(5, (br) => new PipeWaterBoiler(br), (pos, bv) => new PipeWaterBoiler(pos, bv));
		NodeManager.NodeManager.RegisterFactory(6, (br) => new PipeFluidConverter(br), (pos, bv) => new PipeFluidConverter(pos, bv));
		NodeManager.NodeManager.RegisterFactory(7, (br) => new PipeReservoir(br), (pos, bv) => new PipeReservoir(pos, bv));
		NodeManager.NodeManager.RegisterFactory(13, (br) => new PipeFluidInjector(br), (pos, bv) => new PipeFluidInjector(pos, bv));
		NodeManager.NodeManager.RegisterFactory(14, (br) => new PlantationSprinkler(br), (pos, bv) => new PlantationSprinkler(pos, bv));
		NodeManager.NodeManager.RegisterFactory(9, (br) => new PipeWell(br), (pos, bv) => new PipeWell(pos, bv));
		NodeManager.NodeManager.RegisterFactory(8, (br) => new PlantationFarmLand(br), (pos, bv) => new PlantationFarmLand(pos, bv));
		NodeManager.NodeManager.RegisterFactory(10, (br) => new PlantationFarmPlot(br), (pos, bv) => new PlantationFarmPlot(pos, bv));
		NodeManager.NodeManager.RegisterFactory(11, (br) => new PlantationGrowing(br), (pos, bv) => new PlantationGrowing(pos, bv));
		NodeManager.NodeManager.RegisterFactory(12, (br) => new PlantationComposter(br), (pos, bv) => new PlantationComposter(pos, bv));

	}

	public void GameStartDone()
	{
		if (Registered) return;
		Registered = true;
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
				Log.Out("Support 45 {0} {1}", __result, block.shape);
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

	[HarmonyPatch(typeof(PowerConsumer))]
	[HarmonyPatch("IsPoweredChanged")]
	public class IsPoweredChanged
	{
		static void Prefix(
			PowerItem __instance,
			bool newPowered)
		{
			Log.Warning("Set power1 now {0} => {1}", __instance.Position, newPowered);
			if (!NodeManagerInterface.HasInstance) return;
			if (!NodeManagerInterface.HasServer) return;
			var action = new ActionSetPower();
			action.Setup(__instance.Position, newPowered);
			NodeManagerInterface.Instance.ToWorker.Enqueue(action);
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
			if (NodeManagerInterface.Instance.Mother.HasPendingCanConnect(_focusBlockPos))
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
