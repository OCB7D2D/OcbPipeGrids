using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLData.Parsers;

namespace NodeManager
{
    class BlockConfig
    {
		static public void InitReservoir(IBlockReservoir block)
		{
			if (block.BLK.Properties.Contains("MaxFillState")) block.MaxFillState =
				float.Parse(block.BLK.Properties.GetString("MaxFillState"));
			if (block.BLK.Properties.Contains("FluidType")) block.FluidType =
				ushort.Parse(block.BLK.Properties.GetString("FluidType"));
		}

		public static void InitPlant(IPlantBlock block)
		{
			var properties = block.BLK.Properties;
			block.SoilMaintenance.Init(properties, "Soil");
			block.WaterMaintenance.Init(properties, "Water");
			if (properties.Contains("GrowthMaintenanceFactor")) block.GrowthMaintenanceFactor =
				float.Parse(properties.GetString("GrowthMaintenanceFactor")) / 1200f;
			if (properties.Contains("IllnessEffect")) block.IllnessEffect =
					properties.GetString("IllnessEffect").Trim();
		}

        public static void InitWell(IWellBlock block)
        {
			var properties = block.BLK.Properties;
			if (properties.Contains("FromGround")) block.FromGround =
				float.Parse(properties.GetString("FromGround")) / 1000f;
			if (properties.Contains("FromFreeSky")) block.FromFreeSky =
				float.Parse(properties.GetString("FromFreeSky")) / 1000f;
			if (properties.Contains("FromWetSurface")) block.FromWetSurface =
				float.Parse(properties.GetString("FromWetSurface")) / 1000f;
			if (properties.Contains("FromSnowfall")) block.FromSnowfall =
				float.Parse(properties.GetString("FromSnowfall")) / 1000f;
			if (properties.Contains("FromRainfall")) block.FromRainfall =
				float.Parse(properties.GetString("FromRainfall")) / 1000f;
			if (properties.Contains("FromIrrigation")) block.FromIrrigation =
				float.Parse(properties.GetString("FromIrrigation")) / 1000f;
			if (properties.Contains("MaxWaterLevel")) block.MaxWaterLevel =
				float.Parse(properties.GetString("MaxWaterLevel"));
		}

		public static void InitReacher(IReacherBlock block)
		{
			var properties = block.BLK.Properties;
			if (properties.Contains("BlockReach")) block.BlockReach =
				Vector3i.Parse(properties.GetString("BlockReach"));
			if (properties.Contains("ReachOffset")) block.ReachOffset =
				Vector3i.Parse(properties.GetString("ReachOffset"));
			if (properties.Contains("BoundHelperColor")) block.BoundHelperColor =
				StringParsers.ParseColor32(properties.GetString("BoundHelperColor"));
			if (properties.Contains("ReachHelperColor")) block.ReachHelperColor =
				StringParsers.ParseColor32(properties.GetString("ReachHelperColor"));
		}

		private static uint SetSideMask(byte face, uint side, uint mask)
			=> mask | (1U << (int)(4 * face + side));

		static public void InitConnection(IBlockConnection block)
		{
			if (block.BLK.Properties.Contains("MultiBlockPipe"))
			{
				block.MultiBlockPipe = block.BLK
					.Properties.GetBool("MultiBlockPipe");
			}
			if (block.BLK.Properties.Contains("PipeDiameter")) block.PipeDiameter =
				byte.Parse(block.BLK.Properties.GetString("PipeDiameter"));

			if (block.BLK.Properties.Contains("PipeConnectors"))
			{
				block.ConnectMask = 0; // Reset the mask first
				string[] connectors = block.BLK.Properties.GetString("PipeConnectors").ToLower()
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
		

    }
}
