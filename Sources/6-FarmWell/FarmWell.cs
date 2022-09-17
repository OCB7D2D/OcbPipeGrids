using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{

    // Denotes a Block that can receive
    // `ItemActionExchangeInteraction`
    public interface IBlockExchangeItems
    {
        bool AcceptItem(ItemClass type);
    }

    public interface IExchangeItems
    {
        int AskExchangeCount(int type, int count);
        int ExecuteExchange(int type, int count);
    }

    public interface IExchangeFluids
    {
        float FillLevel { get; set; }
        int ExchangeItem(int count, int factor);
    }

    public class FarmWell : NodeBlock<BlockFarmWell>, IWell, IStateNode, IExchangeFluids //, IWorldLink<PipeIrrigation>
    {
        public IReacherBlock RBLK => BLOCK;

        public static TYPES NodeType = TYPES.FarmWell;
        public override uint StorageID => (uint)TYPES.FarmWell;

        public ushort FluidType => 1;

        public override bool RequiresBiome => true;

        //########################################################
        // Settings for Well (from block)
        //########################################################

        public float FromGround => BLOCK != null ? BLOCK.FromGround : 0.08f / 1000f;
        public float FromFreeSky => BLOCK != null ? BLOCK.FromFreeSky : 0.25f / 1000f;
        public float FromWetSurface => BLOCK != null ? BLOCK.FromWetSurface : 0.15f / 1000f;
        public float FromSnowSurface => BLOCK != null ? BLOCK.FromSnowSurface : 0.15f / 1000f;
        public float FromSnowfall => BLOCK != null ? BLOCK.FromSnowfall : 0.4f / 1000f;
        public float FromRainfall => BLOCK != null ? BLOCK.FromRainfall : 0.8f / 1000f;
        public float FromIrrigation => BLOCK != null ? BLOCK.FromIrrigation : 5f / 1000f;
        public float MaxWaterLevel => BLOCK != null ? BLOCK.MaxWaterLevel : 150f;

        //########################################################
        // Implementation for `IFilled` interface
        //########################################################

        public float FillLevel { get; set; } = 0;
        public byte CurrentSunLight { get; set; } = 0;

        //########################################################
        // Setup for node manager implementation
        //########################################################
        
        public override ulong NextTick => 30;

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IIrrigator> Irrigators { get; }
            = new HashSet<IIrrigator>();

        public void AddLink(IIrrigator irrigator)
        {
            Irrigators.Add(irrigator);
            irrigator.Wells.Add(this);
        }

        public HashSet<IFarmLand> FarmLands { get; }
            = new HashSet<IFarmLand>();

        public void AddLink(IFarmLand soil)
        {
            FarmLands.Add(soil);
            soil.Wells.Add(this);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public FarmWell(Vector3i position, BlockValue bv) : base(position, bv)
        {
        }

        public FarmWell(BinaryReader br)
            : base(br)
        {
            FillLevel = br.ReadSingle();
            CurrentSunLight = br.ReadByte();
            Log.Out("------- LOADDED {0}", WorldPos);
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(FillLevel);
            bw.Write(CurrentSunLight);
        }

        //########################################################
        // Implementation to integrate with manager
        // Setup data to allow queries where needed
        //########################################################

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveWell(this);
            manager?.AddWell(this);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return string.Format("Available: {0}, Sun: {1}, Sources: {2}, Add: {3}\nIrrigators: {4}, Soils: {5}",
                FillLevel, CurrentSunLight, Irrigators.Count, AddWater, Irrigators.Count, FarmLands.Count);
        }

        //########################################################
        //########################################################


        public override bool Tick(ulong delta)
        {
            base.Tick(delta);

            // Log.Out("Ticked the well");
            if (FillLevel >= MaxWaterLevel) return true;

            // Check if chunk is loaded to update light level
            // This determines if rain fall can reach the well
            //if (world.GetChunkFromWorldPos(WorldPos) is Chunk chunk)
            //{
            //    // Once chunk is unloaded this stays constant
            //    // Assuming nobody is able to change anything
            //    SunLight = chunk.GetLight(
            //        WorldPos.x, WorldPos.y, WorldPos.z,
            //        Chunk.LIGHT_TYPE.SUN);
            //}

            var Manager = NodeManagerInterface.Instance;
            //Log.Out("FooBar Manager {0}", Manager);
            //Log.Out("Tick Well {0} => {1}", BiomeID,
            //    Manager.GetBiomeParticleRain(BiomeID));

            //asd.Worker.Manager.BiomeWeathers

            // Fill well from "free sky"
            float fill = FromFreeSky *
                // Quadratic fall-off from "free sky"
                CurrentSunLight * CurrentSunLight / 225f;
            // Some free for all
            fill += FromGround;

            // Add factor now
            fill *= delta;

            // Outside effects that add water
            // E.g. weather at loaded blocks
            fill += AddWater / 8f;
            AddWater *= 7f / 8f;

            // Add dynamic params according to Biomes
            fill += FromRainfall * Manager.GetBiomeParticleRain(BiomeID);
            fill += FromSnowfall * Manager.GetBiomeParticleSnow(BiomeID);
            fill += FromWetSurface * Manager.GetBiomeSurfaceWet(BiomeID);
            fill += FromSnowSurface * Manager.GetBiomeSurfaceSnow(BiomeID);

            // Fill well from water outputs
            // ToDo: Only fill once per grid?
            foreach (var irrigator in Irrigators)
            {
                if (irrigator == null) continue;
                if (!irrigator.IsPowered) continue;
                float take = FromIrrigation * delta;
                take = Math.Min(take, irrigator.FillLevel);
                irrigator.FillLevel -= take;
                fill += take;
            }

            // Add water to well
            FillWater(fill);

            return true;
        }

        //########################################################
        //########################################################


        public float AddWater { get; set; } = 0;







        public bool ConsumeWater(float amount)
        {
            if (FillLevel < amount) return false;
            FillLevel -= amount;
            return true;
        }

        public void FillWater(float amount)
        {
            if (amount <= 0) return;
            if (FillLevel > MaxWaterLevel)
            {
                FillLevel = MaxWaterLevel;
                // UpdateWaterLevel();
            }
            else if (FillLevel < MaxWaterLevel)
            {
                FillLevel += amount;
                if (FillLevel > MaxWaterLevel)
                    FillLevel = MaxWaterLevel;
                // UpdateWaterLevel();
            }
        }

        public static int DivideAndRoundUp(int divident, int divisor)
        {
            int result = (divident / divisor);
            if (divident % divisor != 0) result++;
            return result;
        }

        public int ExchangeItem(int count, int factor)
        {
            if (factor < 0)
            {
                float req = FillLevel - MaxWaterLevel;
                int buckets = (int)Mathf.Ceil(req / factor);
                buckets = MathUtils.Min(count, buckets);
                FillWater(buckets * -factor);
                return buckets;
            }
            else if (factor > 0)
            {
                int buckets = (int)(FillLevel / factor);
                buckets = MathUtils.Min(count, buckets);
                ConsumeWater(buckets * factor);
                return buckets;
            }
            return 0;
        }

        //########################################################
        //########################################################

        public IVisualState GetState()
        {
            return new FilledState()
            {
                BID = BLOCK.blockID,
                Position = WorldPos,
                FillState = FillLevel,
            };
        }

        //########################################################
        //########################################################

    }
}