using NodeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ItemActionPlantInteraction : ItemAction
{
    public override void ReadFrom(DynamicProperties props)
    {
        base.ReadFrom(props);
    }

    public override void OnHoldingUpdate(ItemActionData _actionData)
    {
        if (_actionData.lastUseTime == 0.0 || IsActionRunning(_actionData)) return;
        Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
        BlockValue BV = _actionData.invData.hitInfo.hit.blockValue;
        if (!(BV.Block is BlockPlantationGrowing growing)) return;
        Log.Out("Interacted with plant, pesticide");
        //BlockValue BV = _actionData.invData.world.GetBlock(blockPos);
        //NodeManager.BlockHelper.GetIllness(BV)
        //
        //BlockValue blockValue = ItemClass.GetItem(this.changeBlockTo).ToBlockValue();
        //_actionData.invData.world.SetBlockRPC(blockPos, blockValue);

        // base.OnHoldingUpdate(_actionData);
    }

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        if (!_bReleased || IsActionRunning(_actionData)) return;
        Vector3i pos = _actionData.invData.hitInfo.hit.blockPos;
        BlockValue bv = _actionData.invData.hitInfo.hit.blockValue;
        if (!(bv.Block is BlockPlantationGrowing)) return;
        _actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
        MsgPlantInteraction action = new MsgPlantInteraction();
        action.Setup(pos, MsgPlantInteraction.PlantInteraction.Heal,
            Mathf.Pow(EasingFunction.EaseInOutQuad(1f, 4f,
                UnityEngine.Random.value), 0.75f));
        NodeManagerInterface.SendToServer(action);

        //BlockHelper.SetIllness(ref bv, Mathf.Max(0,
        //    BlockHelper.GetIllness(bv) - 3));
        //_actionData.invData.world.SetBlockRPC(pos, bv);



        ItemInventoryData invData = _actionData.invData;
        Ray lookRay = invData.holdingEntity.GetLookRay();
        lookRay.origin += lookRay.direction.normalized * 0.5f;
        _actionData.lastUseTime = Time.time;
        invData.holdingEntity.RightArmAnimationUse = true;



        Log.Out("=> Implement action to pesticide");
    }

    public override bool IsActionRunning(ItemActionData _actionData) => Time.time - _actionData.lastUseTime < Delay;

    public override void StopHolding(ItemActionData _data)
    {
        Log.Out("!!! Stop Holding");
        base.StopHolding(_data);
        _data.lastUseTime = 0.0f;
    }

}
