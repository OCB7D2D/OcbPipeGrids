using NodeManager;
using System;
using XMLData.Parsers;

public static class NodeBlockHelper
{



	private static bool CheckSideMask(byte face, uint side, uint mask)
		=> (mask & (1U << (int)(4 * face + side))) != 0;
	
	public static bool CanConnect(byte ConnectMask, byte face, byte rotation)
	{
		face = FullRotation.InvFace(face, rotation);
		return (ConnectMask & (byte)(1 << face)) != 0;
	}

    public static void OnBlockAdded(IBlockNode block,
		Vector3i pos, BlockValue bv, bool masterOnly = true)
    {
		// Only process main block nodes?
		if (masterOnly && bv.ischild) return;
		// Dispatch to virtual implementation
		var action = new ActionAddBlock();
		action.Setup(block.NodeType, pos, bv);
		NodeManagerInterface.SendToServer(action);
	}

    public static void OnBlockRemoved(IBlockNode block,
		Vector3i pos, BlockValue bv, bool masterOnly = true)
    {
		// Only process main block nodes?
		if (masterOnly && bv.ischild) return;
		// Dispatch to virtual implementation
		var action = new ActionRemoveBlock();
		action.Setup(block.NodeType, pos);
		NodeManagerInterface.SendToServer(action);
	}


}
