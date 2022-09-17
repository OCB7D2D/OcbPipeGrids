using NodeFacilitator;
using UnityEngine;

class ItemActionExchangeInteraction : ItemAction
{
    public override void ReadFrom(DynamicProperties props)
    {
        base.ReadFrom(props);
    }

    public override void OnHoldingUpdate(ItemActionData _actionData)
    {
        if (_actionData.lastUseTime == 0.0) return;
        if (IsAnimationRunning(_actionData)) return;
        // Server answer was pending, but has come back by now
        if (PendingResponse == null) return;
        if (IsActionPending == false) return;

        IsActionPending = false;
        //_actionData.lastUseTime = 0f;

        MsgExchangeQuery query = new MsgExchangeQuery();
        var evt = MsgExchangeQuery.ExchangeItems.Exchange;
        Log.Out("Send final exchange to server {0}", PendingResponse.Position);
        query.Setup(PendingResponse.Position, evt,
            PendingResponse.ItemType, PendingResponse.ItemCount);

        var inv = _actionData.invData.holdingEntity.inventory;
        Log.Out("Decrementing Holding Stuff now {0}", PendingResponse.ItemCount);

        // THis may invoke stop holding item!!!
        if (inv.DecHoldingItem(PendingResponse.ItemCount))
        {
            NodeManagerInterface.SendToWorker(query);
            Log.Out("Send package to server for fill action");
        }

        PendingResponse = null;

        //BlockValue BV = _actionData.invData.world.GetBlock(blockPos);
        //NodeManager.BlockHelper.GetIllness(BV)
        //
        //BlockValue blockValue = ItemClass.GetItem(this.changeBlockTo).ToBlockValue();
        //_actionData.invData.world.SetBlockRPC(blockPos, blockValue);

        // base.OnHoldingUpdate(_actionData);
    }

    public bool IsActionPending { get; private set; } = false;
    private MsgExchangeResponse PendingResponse = null;

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        if (!_bReleased || IsActionRunning(_actionData)) return;
        Vector3i pos = _actionData.invData.hitInfo.hit.blockPos;
        BlockValue bv = _actionData.invData.hitInfo.hit.blockValue;
        // Resolve child to parent
        if (bv.ischild)
        {
            pos = bv.Block.multiBlockPos.GetParentPos(pos, bv);
            bv = _actionData.invData.world.GetBlock(pos);
        }

        if(bv.Block is IBlockExchangeItems exchanger)
        {
            // We don't know how much the thing can actually hold
            MsgExchangeQuery.ExchangeItems evt = MsgExchangeQuery.ExchangeItems.Query;
            ItemClass item = _actionData.invData.holdingEntity.inventory.holdingItem;
            if (!exchanger.AcceptItem(item)) return; // Check for valid holding type
            int count = _actionData.invData.holdingEntity.inventory.holdingCount;
            MsgExchangeQuery action = new MsgExchangeQuery();
            action.Setup(pos, evt, item.Id, count);
            NodeManagerInterface.SendToWorker(action);
            IsActionPending = true;
        }
        else
        {
            return;
        }

        ItemInventoryData invData = _actionData.invData;
        Ray lookRay = invData.holdingEntity.GetLookRay();
        lookRay.origin += lookRay.direction.normalized * 0.5f;
        _actionData.lastUseTime = Time.time;
        Log.Out("Start right arm use animation");
        invData.holdingEntity.RightArmAnimationUse = true;
    }

    public bool IsAnimationRunning(ItemActionData _actionData)
        => Time.time - _actionData.lastUseTime < Delay;

    public override bool IsActionRunning(ItemActionData _actionData)
        => IsAnimationRunning(_actionData) || IsActionPending;

    public override void StopHolding(ItemActionData _data)
    {
        Log.Out("!!! Stop Holding {0}", IsActionRunning(_data));
        base.StopHolding(_data);
        _data.lastUseTime = 0.0f;
        IsActionPending = false;
        PendingResponse = null;
    }

    // This is called when we get back the info from the server
    // We may still want to wait for the animation to finsish
    // Otherwise we can actually try to remove items from hand
    // The decision will be made in the update call
    public void ProcessServerResponse(MsgExchangeResponse msg)
    {
        if (!IsActionPending) return;
        PendingResponse = msg;
    }



}
