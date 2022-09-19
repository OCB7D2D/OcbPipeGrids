using HarmonyLib;

namespace NodeManager
{
    class WaterExchange
    {

		[HarmonyPatch(typeof(ItemActionExchangeItem))]
		[HarmonyPatch("OnHoldingUpdate")]
		public class ItemActionExchangeItem_OnHoldingUpdate
		{
			public static bool Prefix(
				ItemActionExchangeItem __instance,
				ItemActionData _actionData,
				ref string ___changeItemToItem,
				ref string ___changeBlockTo,
				ref string ___doBlockAction,
				ref BlockValue ___hitLiquidBlock,
				ref Vector3i ___hitLiquidPos)
			{
				if (_actionData.lastUseTime == 0.0 || __instance.IsActionRunning(_actionData))
					return false;
				Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
				Log.Out("DO action {0} {1} {2}", ___doBlockAction, ___hitLiquidBlock, ___hitLiquidBlock.Block);

				if (___hitLiquidBlock.Block is BlockPipeWell well)
				{
					QuestEventManager.Current.ExchangedFromItem(_actionData.invData.itemStack);
					ItemValue oldItem = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
					ItemValue newItem = ItemClass.GetItem(___changeItemToItem);
					var wellMasterPos = !___hitLiquidBlock.ischild ? ___hitLiquidPos :
						___hitLiquidPos + ___hitLiquidBlock.parent;

					int factor = 1;

					switch (___doBlockAction)
					{
						case "deplete1":
							factor = 1;
							break;
						case "deplete3":
							factor = 50;
							break;
						case "fill1":
							factor = -1;
							break;
						case "fill3":
							factor = -50;
							break;
					}

					int count = _actionData.invData.holdingEntity.inventory.holdingCount;

					Log.Warning("============= {0}", count);

					var query = new MsgWaterExchangeQuery();
					query.Setup(wellMasterPos);
					query.Factor = factor;
					query.HoldingCount = count;
					query.OldItemType = oldItem.type;
					query.NewItemType = newItem.type;
					query.InventorySlot = _actionData.invData.slotIdx;
					// _actionData.invData.slotIdx
					NodeManagerInterface.SendToServer(query);

					//int exchanged = filling
					//	? well.FillWater(wellMasterPos, holding, factor)
					//	: well.ConsumeWater(wellMasterPos, holding, factor);
					/*
					int exchanged = 10;

					Log.Out("Requested {0} (*{1}) and exchanged {2}", holding, factor, exchanged);

					// exchanged /= factor;

					Log.Out("Adjusted for factor {0}", exchanged);

					if (exchanged == holding)
					{
						// Replace to complete stack we are currently holding
						_actionData.invData.holdingEntity.inventory.SetItem(
							_actionData.invData.slotIdx, new ItemStack(newItem, holding));
					}
					else
					{
						// Reduce stack we are currently holding partially
						_actionData.invData.holdingEntity.inventory.SetItem(
							_actionData.invData.slotIdx, new ItemStack(oldItem, holding - exchanged));
						// Try to distribute wherever we have space
						var stack = new ItemStack(newItem, exchanged);

						if (stack.count > 0) _actionData.invData.holdingEntity.bag.TryStackItem(0, stack);
						if (stack.count > 0) _actionData.invData.holdingEntity.inventory.TryStackItem(0, stack);
						if (stack.count > 0 && _actionData.invData.holdingEntity.bag.AddItem(stack)) stack.count = 0;
						if (stack.count > 0 && _actionData.invData.holdingEntity.inventory.AddItem(stack)) stack.count = 0;
						if (stack.count > 0) _actionData.invData.world.GetGameManager()
							.ItemDropServer(stack, ___hitLiquidPos + Vector3i.up, Vector3i.zero);
					}
					Log.Out("Exchange water from block");
					*/
					_actionData.lastUseTime = 0.0f;
					return false;
				}
				else
				{
					// QuestEventManager.Current.ExchangedFromItem(_actionData.invData.itemStack);
					// ItemValue _itemValue = ItemClass.GetItem(___changeItemToItem);
					// _actionData.invData.holdingEntity.inventory.SetItem(_actionData.invData.slotIdx, new ItemStack(_itemValue, _actionData.invData.holdingEntity.inventory.holdingCount));
					// if (___doBlockAction != null && GameManager.Instance.World.IsWater(___hitLiquidPos))
					// 	___hitLiquidBlock.Block.DoExchangeAction((WorldBase)_actionData.invData.world, 0, ___hitLiquidPos, ___hitLiquidBlock, ___doBlockAction, _actionData.invData.holdingEntity.inventory.holdingCount);
					// if (___changeBlockTo == null)
					// 	return false;
					// BlockValue blockValue = ItemClass.GetItem(___changeBlockTo).ToBlockValue();
					// _actionData.invData.world.SetBlockRPC(blockPos, blockValue);
				}
				return true;
			}

			private static void StackItems(Bag bag, ItemStack stack)
			{
				bag.AddItem(stack);
			}

			private static void StackItems(Inventory inv, ref ItemStack stack)
			{
				inv.AddItem(stack);
			}

		}

	}
}
