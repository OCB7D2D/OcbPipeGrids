using System;
using System.Xml;
using UnityEngine;
using XMLData.Parsers;

namespace NodeFacilitator
{

	// Helper functions to parse block settings

	static partial class BlockConfig
	{

        public static void UpdateReacher(IReacherBlock reacher, ref int max, int off = 0)
        {
            UpdateReacher(reacher.Reach.BlockReach, reacher.Reach.ReachOffset, reacher.Dimensions, ref max, off);
            UpdateReacher(reacher.Reach.ReachUpside, reacher.Reach.OffsetUpside, reacher.Dimensions, ref max, off);
            UpdateReacher(reacher.Reach.ReachSideway, reacher.Reach.OffsetSideway, reacher.Dimensions, ref max, off);
        }

        private static void UpdateReacher(Vector3i reach, Vector3i offset, Vector3i dim, ref int max, int off)
        {
            max = Math.Max(reach.x + Math.Abs(offset.x) + Math.Abs(dim.x / 2) + off, max);
            max = Math.Max(reach.y + Math.Abs(offset.y) + Math.Abs(dim.y / 2) + off, max);
            max = Math.Max(reach.z + Math.Abs(offset.z) + Math.Abs(dim.z / 2) + off, max);
        }
        
		static public void InitBlock(Block block)
		{
            var plant = block as IPlantBlock;
            var grow = block as IPlantGrowingBlock;
            var reacher = block as IReacherBlock;
            var filled = block as IFilledBlock;
            var sprinkler = block as ISprinklerBlock;
            var well = block as IWellBlock;

            if (plant != null) InitPlantBase(plant);
            if (grow != null) InitPlantGrowing(grow);
            if (reacher != null) InitReacher(reacher);
            if (filled != null) InitFilled(filled);
            if (well != null) InitWell(well);

            if (well != null && reacher != null) UpdateReacher(
                reacher, ref NodeManager.WellToSoilReach);
            if (reacher != null && sprinkler != null) UpdateReacher(
                reacher, ref NodeManager.SprinklerToSoilReach);
            if (reacher != null && block is BlockComposter) UpdateReacher(
                reacher, ref NodeManager.ComposterToSoilReach);
            if (reacher != null && block is BlockIrrigation) UpdateReacher(
                reacher, ref NodeManager.IrrigatorToWellReach);
            if (reacher != null && block is BlockPlantationGrowLight) UpdateReacher(
                reacher, ref NodeManager.GrowLightToPlantReach, 1);
            
        }

        static public void InitReservoir(IBlockReservoir block)
		{
			if (block.BLK.Properties.Contains("MaxFillState")) block.MaxFillState =
				float.Parse(block.BLK.Properties.GetString("MaxFillState"));
			//if (block.BLK.Properties.Contains("FluidType")) block.FluidType =
			//	ushort.Parse(block.BLK.Properties.GetString("FluidType"));
		}

		private static void InitPlantGrowing(IPlantGrowingBlock block)
		{
            var properties = block.BLK.Properties;
            block.PlantScaleMin = BlockHelper.ParseVector2(properties, "PlantScaleMin", Vector2.one);
            block.PlantScaleFactor = BlockHelper.ParseVector2(properties, "PlantScaleFactor", Vector2.zero);
        }

        private static void InitPlantBase(IPlantBlock block)
		{
			var properties = block.BLK.Properties;
			block.SoilMaintenance.Init(properties, "Soil");
			block.WaterMaintenance.Init(properties, "Water");
			block.SprinklerMaintenance.Init(properties, "Water");
			if (properties.Contains("GrowthMaintenanceFactor")) block.GrowthMaintenanceFactor =
				float.Parse(properties.GetString("GrowthMaintenanceFactor")) / 1200f;
			if (properties.Contains("IllnessEffect")) block.IllnessEffect =
					properties.GetString("IllnessEffect").Trim();
		}

		private static void InitWell(IWellBlock block)
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
		public static void InitFilled(IFilledBlock block)
		{
			var properties = block.BLK.Properties;
			if (properties.Contains("MaxFillState")) block.MaxFillState =
				float.Parse(properties.GetString("MaxFillState"));
		}

		public static void InitReacher(IReacherBlock block)
		{
			var properties = block.BLK.Properties;
			var reach = block.Reach;

			reach.BlockReach = Vector3i.zero;
			reach.ReachOffset = Vector3i.zero;

			if (properties.Contains("BlockReach")) reach.BlockReach =
				Vector3i.Parse(properties.GetString("BlockReach"));

			reach.ReachSideway = reach.BlockReach;
			reach.ReachUpside = reach.BlockReach;
			if (properties.Contains("ReachSideway")) reach.ReachSideway =
				Vector3i.Parse(properties.GetString("ReachSideway"));
			if (properties.Contains("ReachUpside")) reach.ReachUpside =
				Vector3i.Parse(properties.GetString("ReachUpside"));

			if (properties.Contains("ReachOffset")) reach.ReachOffset =
				Vector3i.Parse(properties.GetString("ReachOffset"));

			reach.OffsetSideway = reach.ReachOffset;
			reach.OffsetUpside = reach.ReachOffset;
			if (properties.Contains("OffsetSideway")) reach.OffsetSideway =
				Vector3i.Parse(properties.GetString("OffsetSideway"));
			if (properties.Contains("OffsetUpside")) reach.OffsetUpside =
				Vector3i.Parse(properties.GetString("OffsetUpside"));

			block.Reach = reach;

			if (properties.Contains("BoundHelperColor")) block.BoundHelperColor =
				StringParsers.ParseColor32(properties.GetString("BoundHelperColor"));
			if (properties.Contains("ReachHelperColor")) block.ReachHelperColor =
				StringParsers.ParseColor32(properties.GetString("ReachHelperColor"));
		}

		private static uint SetSideMask(byte face, uint side, uint mask)
			=> mask | (1U << (int)(4 * face + side));

		static public void InitConnection(IBlockConnection block)
		{

            if (block.BLK.Properties.Contains("BreakSegment")) block.BreakSegment =
                bool.Parse(block.BLK.Properties.GetString("BreakSegment"));

            if (block.BLK.Properties.Contains("PipeDiameter"))
			{
                block.PipeDiameter = 0;// Reset the mask first
                foreach (var diameter in block.BLK.Properties.GetString("PipeDiameter")
                    .Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
						block.PipeDiameter |= (byte)(1 << byte.Parse(diameter));
            }

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
					}
					else
					{
						block.ConnectMask |= (byte)(1 << (byte)EnumParser
							.Parse<FullRotation.Face>(connector.Trim()));
					}
				}
			}

            if (block is IRotationSimpleBlock) block.BLK.AllowedRotations = EBlockRotationClasses.Basic90;
            else if (block is IRotationLimitedBlock) block.BLK.AllowedRotations = EBlockRotationClasses.No45;
        }

    }
}
