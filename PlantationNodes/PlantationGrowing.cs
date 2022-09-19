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
    public class PlantationGrowing : NodeBlock<BlockPlantationGrowing>, ISunLight
    {
        public override ulong NextTick => 5;

        public override uint StorageID => 11;

        public byte CurrentSunLight { get; set; } = 0;

        public byte CurrentFertilityLevel { get; set; } = 0;


        public float GrowProgress = 0;

        public float WaterFactor = 0.01f;


        // public float FromGround => BLK != null ? BLK.FromGround : 0.08f / 1000f;
        // public float FromFreeSky => BLK != null ? BLK.FromFreeSky : 0.25f / 1000f;
        // public float FromWetSurface => BLK != null ? BLK.FromWetSurface : 0.15f / 1000f;
        // public float FromSnowfall => BLK != null ? BLK.FromSnowfall : 0.4f / 1000f;
        // public float FromRainfall => BLK != null ? BLK.FromRainfall : 0.8f / 1000f;
        // public float FromIrrigation => BLK != null ? BLK.FromIrrigation : 5f / 1000f;
        // public float MaxWaterLevel => BLK != null ? BLK.MaxWaterLevel : 150f;

        // Keep a list of pumps to get water from?
        // Add pumps from different grids (sources)?
        readonly HashSet<PipeIrrigation> Irrigators
            = new HashSet<PipeIrrigation>();

        public PlantationGrowing(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationGrowing(BinaryReader br)
            : base(br)
        {
            WaterFactor = br.ReadSingle();
            GrowProgress = br.ReadSingle();
            CurrentSunLight = br.ReadByte();
            CurrentFertilityLevel = br.ReadByte();
            Log.Out("------- LOADDED PLANT {0}", WorldPos);
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(WaterFactor);
            bw.Write(GrowProgress);
            bw.Write(CurrentSunLight);
            bw.Write(CurrentFertilityLevel);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Plant Growing {0}\nWater: {1}, Light: {2}, Fert: {3}",
                GrowProgress, WaterFactor, CurrentSunLight, CurrentFertilityLevel);
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemovePlantGrowing(WorldPos);
            manager?.AddPlantGrowing(this);
        }

        public override void Tick(ulong delta)
        {
            Log.Out("Tick plant");
            base.Tick(delta);

            bool NeedsWater = true;


            // Check if plant needs water to grow
            // Should be the only water grid API use
            if (NeedsWater)
            {
                // 3% without water
                // GrowthFactor = 0.03f;
                // HasWater = 0.0f;
                float consumedWater = 0;
                // See if we can consume any water
                // foreach (PipeWell well in Wells)
                // {
                // 
                //     if (well.ConsumeWater(0.01f * delta * WaterFactor / 100f))
                //         consumedWater += WaterFactor;
                // }
                //Log.Out("Tick of {0} vs {1}", GameTimer.Instance.ticks, StartTick);
                //Log.Out("COnsumed Water {3}ms -> {0} of {1} of {2}", consumedWater,
                //    Mathf.Pow(delta / 2000f, 1.15f),
                //    Wells.Count, delta);
                WaterFactor += consumedWater > 0 ?
                    delta / 2000f * consumedWater :
                    -2f * delta / 2000f;
                if (WaterFactor > 2f) WaterFactor = 2f;
                else if (WaterFactor < 0.03f) WaterFactor = 0.03f;
            }
            else
            {
                WaterFactor = 1f; // Base value
            }

            // HasWater = WaterFactor;

            GrowProgress += WaterFactor / 60f * delta * CurrentSunLight;
            //Log.Out("Ticked {0} (loaded: {1}, progress: {2:0}%, light: {3})",
            //    GetBlock().GetBlockName(),
            //    GetIsLoaded(), GrowProgress * 100f, light);
            // if (GrowProgress < 1f) RegisterScheduled();
            // else GrowToNext(world, PlantManager.Instance);

        }



        /*

            internal void AddIrrigation(PipeIrrigation irrigation)
            {
                Irrigators.Add(irrigation);
            }

            internal void RemoveIrrigation(PipeIrrigation irrigation)
            {
                Irrigators.Remove(irrigation);
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
            */
    }
}