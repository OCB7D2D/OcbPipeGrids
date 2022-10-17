using System;
using XMLData.Parsers;

public static class PipeBlockHelper
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
		block.CreateGridItem(pos, bv);
	}

    public static void OnBlockRemoved(IBlockNode block,
		Vector3i pos, BlockValue bv, bool masterOnly = true)
    {
		// Only process main block nodes?
		if (masterOnly && bv.ischild) return;
		// Dispatch to virtual implementation
		block.RemoveGridItem(pos);
	}


}
