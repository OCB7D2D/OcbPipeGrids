using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{

    public class PipeWell : NodeBlock<BlockPipeWell>, IWell //, IWorldLink<PipeIrrigation>
    {
        public IReacherBlock RBLK => BLOCK;

        public static TYPES NodeType = TYPES.PipeWell;
        public override uint StorageID => (uint)TYPES.PipeWell;

        public ushort FluidType => 1;

        //########################################################
        // Settings for Well (from block)
        //########################################################

        public float FromGround => BLOCK != null ? BLOCK.FromGround : 0.08f / 1000f;
        public float FromFreeSky => BLOCK != null ? BLOCK.FromFreeSky : 0.25f / 1000f;
        public float FromWetSurface => BLOCK != null ? BLOCK.FromWetSurface : 0.15f / 1000f;
        public float FromSnowfall => BLOCK != null ? BLOCK.FromSnowfall : 0.4f / 1000f;
        public float FromRainfall => BLOCK != null ? BLOCK.FromRainfall : 0.8f / 1000f;
        public float FromIrrigation => BLOCK != null ? BLOCK.FromIrrigation : 5f / 1000f;
        public float MaxWaterLevel => BLOCK != null ? BLOCK.MaxWaterLevel : 150f;

        //########################################################
        // Implementation for `IFilled` interface
        //########################################################

        public float FillState { get; set; } = 0;
        public byte CurrentSunLight { get; set; } = 0;

        //########################################################
        // Implementation for `IReachable` (redirect to block)
        //########################################################

        public Vector3i BlockReach { get => BLOCK.BlockReach; set => BLOCK.BlockReach = value; }
        public Vector3i ReachOffset { get => BLOCK.ReachOffset; set => BLOCK.ReachOffset = value; }
        public Color BoundHelperColor { get => BLOCK.BoundHelperColor; set => BLOCK.BoundHelperColor = value; }
        public Color ReachHelperColor { get => BLOCK.ReachHelperColor; set => BLOCK.ReachHelperColor = value; }
        public Vector3i RotatedReach => FullRotation.Rotate(Rotation, BLOCK.BlockReach);
        public Vector3i RotatedOffset => FullRotation.Rotate(Rotation, BLOCK.ReachOffset);
        public Vector3i Dimensions => BLOCK.multiBlockPos?.dim ?? Vector3i.one;
        public bool IsInReach(Vector3i target) => ReachHelper.IsInReach(this, target);

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

        public PipeWell(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PipeWell(BinaryReader br)
            : base(br)
        {
            FillState = br.ReadSingle();
            CurrentSunLight = br.ReadByte();
            Log.Out("------- LOADDED {0}", WorldPos);
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(FillState);
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
                FillState, CurrentSunLight, Irrigators.Count, AddWater, Irrigators.Count, FarmLands.Count);
        }

        //########################################################
        //########################################################


        public override bool Tick(ulong delta)
        {
            base.Tick(delta);

            // Log.Out("Ticked the well");
            if (FillState >= MaxWaterLevel) return true;

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

            // Add Biome weather bonus if loaded
            //if (BlockHelper.IsLoaded(WorldPos))
            //{
            //    var weather = WeatherManager.Instance;
            //    fill += FromWetSurface * weather.GetCurrentWetSurfaceValue();
            //    fill += FromSnowfall * weather.GetCurrentSnowfallValue();
            //    fill += FromRainfall * weather.GetCurrentRainfallValue();
            //}


            // Fill well from water outputs
            // ToDo: Only fill once per grid?
            foreach (var irrigator in Irrigators)
            {
                if (irrigator == null) continue;
                if (!irrigator.IsPowered) continue;
                float take = FromIrrigation * delta;
                take = Math.Min(take, irrigator.FillState);
                irrigator.FillState -= take;
                fill += take;
            }

            // Add water to well
            FillWater(fill);

            return true;
        }

        //########################################################
        //########################################################


        public float AddWater = 0;







        public bool ConsumeWater(float amount)
        {
            if (FillState < amount) return false;
            FillState -= amount;
            return true;
        }

        public void FillWater(float amount)
        {
            if (amount <= 0) return;
            if (FillState > MaxWaterLevel)
            {
                FillState = MaxWaterLevel;
                // UpdateWaterLevel();
            }
            else if (FillState < MaxWaterLevel)
            {
                FillState += amount;
                if (FillState > MaxWaterLevel)
                    FillState = MaxWaterLevel;
                // UpdateWaterLevel();
            }
        }

        public static int DivideAndRoundUp(int divident, int divisor)
        {
            int result = (divident / divisor);
            if (divident % divisor != 0) result++;
            return result;
        }

        internal int ExchangeWater(int count, int factor)
        {
            if (factor < 0)
            {
                float req = FillState - MaxWaterLevel;
                int buckets = (int)Mathf.Ceil(req / factor);
                buckets = MathUtils.Min(count, buckets);
                FillWater(buckets * -factor);
                return buckets;
            }
            else if (factor > 0)
            {
                int buckets = (int)(FillState / factor);
                buckets = MathUtils.Min(count, buckets);
                ConsumeWater(buckets * factor);
                return buckets;
            }
            return 0;
        }

    }
}