using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NodeManager
{
    public class PipeWell : PipeBlock<BlockPipeWell>, ISunLight
    {
        public override ulong NextTick => 2;

        public override uint StorageID => 9;

        public byte CurrentSunLight { get; set; } = 0;

        public float WaterAvailable = 0;

        public float AddWater = 0;

        public float FromGround => BLK != null ? BLK.FromGround : 0.08f / 1000f;
        public float FromFreeSky => BLK != null ? BLK.FromFreeSky : 0.25f / 1000f;
        public float FromWetSurface => BLK != null ? BLK.FromWetSurface : 0.15f / 1000f;
        public float FromSnowfall => BLK != null ? BLK.FromSnowfall : 0.4f / 1000f;
        public float FromRainfall => BLK != null ? BLK.FromRainfall : 0.8f / 1000f;
        public float FromIrrigation => BLK != null ? BLK.FromIrrigation : 5f / 1000f;
        public float MaxWaterLevel => BLK != null ? BLK.MaxWaterLevel : 150f;

        // Keep a list of pumps to get water from?
        // Add pumps from different grids (sources)?
        readonly HashSet<PipeIrrigation> Irrigators
            = new HashSet<PipeIrrigation>();

        public PipeWell(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PipeWell(BinaryReader br)
            : base(br)
        {
            WaterAvailable = br.ReadSingle();
            CurrentSunLight = br.ReadByte();
            Log.Out("------- LOADDED {0}", WorldPos);
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(WaterAvailable);
            bw.Write(CurrentSunLight);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Available: {0}, Sun: {1}, Sources: {2}, Add: {3}",
                WaterAvailable, CurrentSunLight, Irrigators.Count, AddWater);
        }

        internal void AddIrrigation(PipeIrrigation irrigation)
        {
            Irrigators.Add(irrigation);
        }

        internal void RemoveIrrigation(PipeIrrigation irrigation)
        {
            Irrigators.Remove(irrigation);
        }
        protected override void OnManagerAttached(NodeManager manager)
        {
            base.OnManagerAttached(manager);
            // Manager?.RemoveWell(WorldPos);
            // manager.AddWell(this);
        }

        public override void Tick(ulong delta)
        {
            base.Tick(delta);

            // Log.Out("Ticked the well");
            if (WaterAvailable >= MaxWaterLevel) return;

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
                // Quadratic fall-off
                CurrentSunLight * CurrentSunLight / 225f;
            // Some free for all
            fill += FromGround;

            // Outside effects that add water
            // E.g. weather at loaded blocks
            fill += AddWater;
            AddWater /= 2;

            // Add Biome weather bonus if loaded
            //if (BlockHelper.IsLoaded(WorldPos))
            //{
            //    var weather = WeatherManager.Instance;
            //    fill += FromWetSurface * weather.GetCurrentWetSurfaceValue();
            //    fill += FromSnowfall * weather.GetCurrentSnowfallValue();
            //    fill += FromRainfall * weather.GetCurrentRainfallValue();
            //}

            // Add factor now
            fill *= delta;

            // Fill well from water outputs
            // ToDo: Only fill once per grid?
            foreach (var irrigator in Irrigators)
            {
                if (irrigator == null) continue;
                if (!irrigator.IsPowered) continue;
                float take = FromIrrigation * delta;
                take = System.Math.Min(take, irrigator.FillState);
                irrigator.FillState -= take;
                fill += take;
            }

            // Add water to well
            FillWater(fill);

        }

        public bool ConsumeWater(float amount)
        {
            if (WaterAvailable < amount) return false;
            WaterAvailable -= amount;
            return true;
        }

        public void FillWater(float amount)
        {
            if (amount <= 0) return;
            if (WaterAvailable > MaxWaterLevel)
            {
                WaterAvailable = MaxWaterLevel;
                // UpdateWaterLevel();
            }
            else if (WaterAvailable < MaxWaterLevel)
            {
                WaterAvailable += amount;
                if (WaterAvailable > MaxWaterLevel)
                    WaterAvailable = MaxWaterLevel;
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
                float req = WaterAvailable - MaxWaterLevel;
                int buckets = (int)Mathf.Ceil(req / factor);
                buckets = MathUtils.Min(count, buckets);
                FillWater(buckets * -factor);
                return buckets;
            }
            else if (factor > 0)
            {
                int buckets = (int)(WaterAvailable / factor);
                buckets = MathUtils.Min(count, buckets);
                ConsumeWater(buckets * factor);
                return buckets;
            }
            return 0;
        }

    }
}