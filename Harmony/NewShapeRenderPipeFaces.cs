using HarmonyLib;
using UnityEngine;

namespace NodeManager
{
	// Fix issue with powered blocks of e.g. ShapeNew
	// They do not come with a BlockTransform for obvious reasons
	// But we also only need that BlockTransform for its position
	// Not sure why they didn't use `TileEntity.ToWorldPos()`!??
	[HarmonyPatch(typeof(BlockShapeNew))]
	[HarmonyPatch("isRenderFace")]
	public class BlockShapeNew_IsRenderFace
	{
		public static void Postfix(
			BlockValue _blockValue,
			BlockFace _face,
			BlockValue _adjBlockValue,
			int[,] ___convertRotationCached,
			ref bool __result)
		{
			if (__result == false) return;
			__result = !ConnectionHelper.CanConnect(
				_blockValue, _face, _adjBlockValue);
		}
	}

}
