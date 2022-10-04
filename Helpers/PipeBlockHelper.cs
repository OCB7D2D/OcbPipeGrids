using System;
using XMLData.Parsers;

public static class PipeBlockHelper
{

	static public void InitBlock(IBlockReservoir block)
	{
		if (block.Block.Properties.Contains("MaxFillState")) block.MaxFillState =
			float.Parse(block.Block.Properties.GetString("MaxFillState"));
		if (block.Block.Properties.Contains("FluidType")) block.FluidType =
			ushort.Parse(block.Block.Properties.GetString("FluidType"));
	}

	static public void InitBlock(IBlockConnection block)
	{
		if (block.Block.Properties.Contains("MultiBlockPipe"))
		{
			block.MultiBlockPipe = block.Block
				.Properties.GetBool("MultiBlockPipe");
		}
		if (block.Block.Properties.Contains("PipeDiameter")) block.PipeDiameter =
			byte.Parse(block.Block.Properties.GetString("PipeDiameter"));

		if (block.Block.Properties.Contains("PipeConnectors"))
		{
			block.ConnectMask = 0; // Reset the mask first
			string[] connectors = block.Block.Properties.GetString("PipeConnectors").ToLower()
				.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
			foreach (string connector in connectors)
			{
				var parts = connector.Split(new char[] { ':' }, 2);
				if (parts.Length == 0) continue; // Play safe
				if (parts.Length == 2)
                {
					// First gather generic face mask
                    byte face = (byte)EnumParser.Parse<
						FullRotation.Face>(parts[0].Trim());
					block.ConnectMask |= (byte)(1 << face);
					// Then gather specific side masks
					uint side = uint.Parse(parts[1]);
					if (side < 1 || side > 4) throw new
						Exception("Invalid Side (1-4)");
					block.SideMask = SetSideMask(face,
						side - 1, block.SideMask);
					Log.Warning("====> SideMask {0}", block.SideMask);
				}
				else
                {
					block.ConnectMask |= (byte)(1 << (byte)EnumParser
						.Parse<FullRotation.Face>(connector.Trim()));
				}
			}
		}
	}

	private static uint SetSideMask(byte face, uint side, uint mask)
		=> mask | (1U << (int)(4 * face + side));

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
