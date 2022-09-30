using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NodeManager;

public class OcbPipeGrids : IModApi
{

	// Entry class for A20 patching
	public void InitMod(Mod mod)
	{
		Log.Out("Loading OCB Pipe Grids Patch: " + GetType().ToString());
		var harmony = new Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		ModEvents.GameUpdate.RegisterHandler(GameUpdate);

		ReflectionHelpers.FindTypesImplementingBase(typeof(NetPackage), (System.Action<System.Type>)(_type => Log.Out("Package {0} vs {1}", _type.Name, _type)));

        NodeManager.NodeManager.RegisterFactory(1, (br) => new PipeConnection(br));
        NodeManager.NodeManager.RegisterFactory(2, (br) => new PipePump(br));
        NodeManager.NodeManager.RegisterFactory(3, (br) => new PipeIrrigation(br));
        NodeManager.NodeManager.RegisterFactory(4, (br) => new PipeSource(br));

		NodeManager.NodeManager.RegisterFactory(9, (br) => new PipeWell(br));

		NodeManager.NodeManager.RegisterFactory(11, (br) => new PlantationGrowing(br));
		NodeManager.NodeManager.RegisterFactory(12, (br) => new PlantationComposter(br));

	}

	private void GameUpdate()
    {
		if (!NodeManagerInterface.HasInstance) return;
		NodeManagerInterface.Instance.Update();
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
		public static void Postfix(Vector3i _focusBlockPos, Transform ___transformFocusCubePrefab)
		{
			if (!NodeManagerInterface.Instance.Mother.HasPendingCanConnect(_focusBlockPos)) return;
			// Log.Out("Update displaced");
			foreach (Renderer child in ___transformFocusCubePrefab.GetComponentsInChildren<Renderer>())
				child.material.SetColor("_Color", Color.magenta);
		}
	}

}
