using XMLData.Parsers;

public static class PipeBlockHelper
{

	static public void InitBlock(IBlockConnection block)
	{
		if (block.Block.Properties.Contains("PipeConnectors"))
		{
			block.ConnectMask = 0; // Reset the mask first
			string[] connectors = block.Block.Properties.GetString("PipeConnectors").ToLower()
				.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
			foreach (string connector in connectors)
			{
				block.ConnectMask |= (byte)(1 << (byte)EnumParser
					.Parse<FullRotation.Side>(connector.Trim()));
			}
		}
	}

	public static bool CanConnect(byte ConnectMask, byte side, byte rotation)
	{
		side = FullRotation.InvSide(side, rotation);
		return (ConnectMask & (byte)(1 << side)) != 0;
	}

    public static void OnBlockAdded(IBlockConnection block, Vector3i pos, BlockValue bv)
    {
		// Only process main block nodes
		if (bv.ischild) return;
		// Dispatch to virtual implementation
		block.CreateGridItem(pos, bv);
	}

    public static void OnBlockRemoved(IBlockConnection block, Vector3i pos, BlockValue bv)
    {
		// Only process main block nodes
		if (bv.ischild) return;
		// Dispatch to virtual implementation
		block.RemoveGridItem(pos);
	}


}
