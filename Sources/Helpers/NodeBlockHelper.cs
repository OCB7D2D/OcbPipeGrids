using NodeFacilitator;
using UnityEngine;
using static LightingAround;

public static class NodeBlockHelper
{

	public static bool CanConnect(byte ConnectMask, byte face, byte rotation)
	{
		face = FullRotation.InvFace(face, rotation);
		return (ConnectMask & (byte)(1 << face)) != 0;
	}

    public static void OnBlockAdded(IBlockNode block,
		Vector3i pos, BlockValue bv, bool masterOnly = true)
    {
        // Only register blocks on server instance
		if (NodeManagerInterface.IsRemoteClient) return;
        // Only process main block nodes?
        if (masterOnly && bv.ischild) return;
        // Dispatch to virtual implementation
        // Log.Out("Action add block {0} (child {1})", pos, bv.ischild);
        if (bv.Block is IRotationLimitedBlock)
            if (bv.rotation > 23)
            {
                Log.Warning("Skip invalid rotation block");
                return;
            }
        var action = new ActionAddBlock();
		action.Setup(block.NodeType, pos, bv); 
		NodeManagerInterface.PushToWorker(action);
	}

    public static void OnBlockRemoved(IBlockNode block,
		Vector3i pos, BlockValue bv, bool masterOnly = true)
    {
        // Only register blocks on server instance
        if (NodeManagerInterface.IsRemoteClient) return;
        // Only process main block nodes?
        if (masterOnly && bv.ischild) return;
		// Dispatch to virtual implementation
		var action = new ActionRemoveBlock();
		action.Setup(block.NodeType, pos);
		NodeManagerInterface.PushToWorker(action);
	}

    public static void OnBlockValueChanged(IBlockNode block,
        Vector3i pos, BlockValue bv, bool masterOnly = true)
    {
        // Only register blocks on server instance
        if (NodeManagerInterface.IsRemoteClient) return;
        // Only process main block nodes?
        if (masterOnly && bv.ischild) return;
        // Dispatch to virtual implementation
        var action = new ActionBlockValueChanged();
        action.Setup(block.NodeType, pos, bv);
        NodeManagerInterface.PushToWorker(action);
    }

    public static float ConvertReservoir(float consume, float factor,
		IFilledState input, IFilledState output)
    {
        consume = Mathf.Min(input.FillLevel, consume);
        float filling = consume * factor;
        if (filling > output.MaxFillState - output.FillLevel)
            filling = output.MaxFillState - output.FillLevel;
        consume = filling / factor;
        input.FillLevel -= consume;
        output.FillLevel += filling;
		return consume;
    }

}
