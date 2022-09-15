using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PipeManager;

public class OcbPipeGrids : IModApi
{

	// Entry class for A20 patching
	public void InitMod(Mod mod)
	{
		Log.Out("Loading OCB Pipe Grids Patch: " + GetType().ToString());
		var harmony = new Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		ModEvents.GameUpdate.RegisterHandler(GameUpdate);

		PipeGridManager.RegisterFactory(1, (br) => new PipeConnection(br));
		PipeGridManager.RegisterFactory(2, (br) => new PipePump(br));
		PipeGridManager.RegisterFactory(3, (br) => new PipeIrrigation(br));

	}

	private void GameUpdate()
    {
		if (!PipeGridInterface.HasInstance) return;
		PipeGridInterface.Instance.Update();
    }

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
			PipeGridInterface.Instance.Init();
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
			if (!PipeGridInterface.HasInstance) return;
			if (ConnectionManager.Instance.IsServer)
				PipeGridInterface.Instance.Cleanup();
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
			if (!PipeGridInterface.Instance.Client.HasPendingCanConnect(_focusBlockPos)) return;
			foreach (Renderer child in ___transformFocusCubePrefab.GetComponentsInChildren<Renderer>())
				child.material.SetColor("_Color", Color.magenta);
		}
	}

}
