﻿using HarmonyLib;
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

	[HarmonyPatch(typeof(ItemActionConnectPower))]
	[HarmonyPatch("OnHoldingUpdate")]

	public class ItemActionConnectPower_OnHoldingUpdate
	{

		public static void Prefix(
			ref ItemActionData _actionData)
		{
			WorldRayHitInfo info = _actionData.invData.hitInfo;
			if (!info.bHitValid) return;
			// Fix hit position if block is a child of a multi-dim block
			BlockValue block = _actionData.invData.hitInfo.hit.blockValue;
			if (!(block.ischild && block.Block is BlockPowered)) return;
			// Adjust the hit info to master block
			info.hit.blockPos += block.parent;
			info.hit.blockValue = _actionData.invData
				.world.GetBlock(info.hit.blockPos);
		}
	}

}
