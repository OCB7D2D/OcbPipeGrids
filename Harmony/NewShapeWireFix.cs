using HarmonyLib;
using UnityEngine;

namespace NodeManager
{
	// Fix issue with powered blocks of e.g. ShapeNew
	// They do not come with a BlockTransform for obvious reasons
	// But we also only need that BlockTransform for its position
	// Not sure why they didn't use `TileEntity.ToWorldPos()`!??
	[HarmonyPatch(typeof(TileEntityPowered))]
	[HarmonyPatch("get_BlockTransform")]
	public class TileEntityPowered_GetBlockTransform
	{
		static readonly GameObject GO = new GameObject();
		static readonly Vector3 Offset = Vector3.one / 2;

		public static void Postfix(
			TileEntityPowered __instance,
			ref Transform __result)
		{
			if (__result != null) return;
			Vector3 pos = __instance.ToWorldPos();
			pos += Offset - Origin.position;
			GO.transform?.SetPositionAndRotation(
				pos, Quaternion.identity);
			__result = GO.transform;
		}
	}

}
