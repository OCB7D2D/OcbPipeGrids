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

    public interface IPlant : ISunLight, ITickable, IfaceGridNodeManaged
    {
        HashSet<IPlant> Plants { get; }
        HashSet<IComposter> Composters { get; }
    }

    public abstract class PlantationBase : NodeBlock<BlockPlantationGrowing>, IPlant
    {

        // public List<IPlant> Plants { get; } = new List<IPlant>();

        public HashSet<IPlant> Plants { get; } = new HashSet<IPlant>();
        public HashSet<IComposter> Composters { get; } = new HashSet<IComposter>();

        public byte CurrentSunLight { get; set; } = 0;

        public PlantationBase(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationBase(BinaryReader br)
            : base(br)
        {
        }

    }

        public class PlantationGrowing : PlantationBase
    {


        public override ulong NextTick => 5;

        public override uint StorageID => 11;

        public byte CurrentFertility { get; set; } = 0;

        public float CurrentRain { get; set; } = 0;

        public float GrowProgress = 0;

        public float WaterFactor = 0.5f;

        public float SoilFactor = 1f;

        public float HealthFactor = 1f;

        public float SickFactor = 0.01f;

        private PipeReservoir Reservoir = null;

        // public byte Flags = 0;

        // public float FromGround => BLK != null ? BLK.FromGround : 0.08f / 1000f;
        // public float FromFreeSky => BLK != null ? BLK.FromFreeSky : 0.25f / 1000f;
        // public float FromWetSurface => BLK != null ? BLK.FromWetSurface : 0.15f / 1000f;
        // public float FromSnowfall => BLK != null ? BLK.FromSnowfall : 0.4f / 1000f;
        // public float FromRainfall => BLK != null ? BLK.FromRainfall : 0.8f / 1000f;
        // public float FromIrrigation => BLK != null ? BLK.FromIrrigation : 5f / 1000f;
        // public float MaxWaterLevel => BLK != null ? BLK.MaxWaterLevel : 150f;

        // Keep a list of pumps to get water from?
        // Add pumps from different grids (sources)?
        internal readonly HashSet<PipeWell> Wells
            = new HashSet<PipeWell>();

        // internal readonly HashSet<PlantationComposter> Composters
        //     = new HashSet<PlantationComposter>();

        public PlantationGrowing(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationGrowing(BinaryReader br)
            : base(br)
        {
            WaterFactor = br.ReadSingle();
            SoilFactor = br.ReadSingle();
            HealthFactor = br.ReadSingle();
            GrowProgress = br.ReadSingle();
            CurrentSunLight = br.ReadByte();
            CurrentFertility = br.ReadByte();
            CurrentRain = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(WaterFactor);
            bw.Write(SoilFactor);
            bw.Write(HealthFactor);
            bw.Write(GrowProgress);
            bw.Write(CurrentSunLight);
            bw.Write(CurrentFertility);
            bw.Write(CurrentRain);
            // bw.Write(Flags);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Plant Growing {0:.00}\nWater: {1:.00}, Light: {2}\nFert: {3}, Rain: {4:.00}\nWells: {5}, Sick: {6}\nPlants: {7}, Reservoir: {8:.00}\nSoil: {9:.00}, Health: {10:.00}, Sick: {11:.00}\nComposters: {12}",
                GrowProgress, WaterFactor, CurrentSunLight, CurrentFertility, CurrentRain, Wells.Count, BV.meta2, Plants.Count, Reservoir?.FillState, SoilFactor, HealthFactor, SickFactor, Composters.Count);
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            //Log.Out("Attach Man {0} {1} {2}",
            //    ID, Manager, manager);
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemovePlantGrowing(this);
            manager?.AddPlantGrowing(this);
        }

        public override void OnAfterLoad()
        {
            base.OnAfterLoad();
            Manager.TryGetNode(
                WorldPos + Vector3i.down,
                out Reservoir);
        }

        static readonly HarmonyFieldProxy<BlockValue>
            FieldNextPlant = new HarmonyFieldProxy<BlockValue>(
                typeof(BlockPlantGrowing), "nextPlant");

        public void DoSicknessCheck(ulong delta)
        {

            // HealthFactor

            // Composter
            if (Manager == null)
            {
                Log.Warning("Plant without Manager");
                return;
            }

            if (UnityEngine.Random.value > 0.95f)
            {
                int illness = BlockHelper.GetIllness(BV);
                if (illness < 7)
                {
                    BlockHelper.SetIllness(ref BV, illness + 1);
                    Log.Warning("Plant now ill {0}", BV.meta2);
                    var action = new ExecuteBlockChange();
                    action.Setup(WorldPos, BV);
                    // Log.Out("Enqueue to manager {0}", Manager);
                    Manager.ToMainThread.Enqueue(action);
                }
            }

            else if (UnityEngine.Random.value < 0.45f)
            {
                int illness = BlockHelper.GetIllness(BV);
                if (illness > 0)
                {
                    Log.Warning("Plant has healed {0}", WorldPos);
                    BlockHelper.SetIllness(ref BV, illness - 1);
                    var action = new ExecuteBlockChange();
                    action.Setup(WorldPos, BV);
                    // Log.Out("Enqueue to manager {0}", Manager);
                    Manager.ToMainThread.Enqueue(action);
                }
            }
        }

        private float MixMe(float waterFactor, float newFactor, ulong delta)
        {
            ulong span = 12000;
            waterFactor = waterFactor * (span - delta) +
                newFactor * (delta);
            waterFactor /= span;
            return waterFactor;
        }

        public void DoWaterCheck(ulong delta)
        {

            float factor = delta / 60000f;
            // Check if plant needs water to grow
            // Should be the only water grid API use

            // 3% without water
            // GrowthFactor = 0.03f;
            // HasWater = 0.0f;
            float consumedWater = 0;

            // Add water from current rain
            consumedWater += CurrentRain * factor;
            // Consume the rain (once unloaded)
            CurrentRain -= factor;

            if (Reservoir != null)
            {
                float quality = 1f;
                switch (Reservoir.FluidType)
                {
                    case 2: quality = 2f; break;
                    case 3: quality = 3f; break;
                    default: quality = 1f; break;
                }

                float wanted = 0.5f * factor * WaterFactor;
                consumedWater += Reservoir.ConsumeFluid(wanted);

                float newFactor = consumedWater / wanted * quality;

                WaterFactor = MixMe(WaterFactor, newFactor, delta);

                //WaterFactor += consumedWater > 0 ?
                //    delta / 2000f * consumedWater :
                //    -2f * delta / 2000f;

                if (WaterFactor > 5f) WaterFactor = 5f;
                else if (WaterFactor < 0.03f) WaterFactor = 0.03f;

            }
            // Plants on ground can only get murky water for soil
            // Reason is that natural soil will make it murky again
            else
            {


                // See if we can consume any water  
                foreach (PipeWell well in Wells)
                {

                    if (well.ConsumeWater(0.01f * delta * WaterFactor / 100f))
                        consumedWater += WaterFactor;
                }
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

        }

        public void DoSoilQualityCheck(ulong delta)
        {
            float factor = delta / 50f;
            // Log.Out("Delta is {0}", delta); // around 50?
            if (Composters.Count == 0) return;
            float wanting = 100f - SoilFactor;
            if (wanting > 0.4f * factor) wanting = 0.4f * factor
                    + 0.2f * Composters.Count * factor;
            wanting /= Composters.Count;
            float taken = 0f;
            foreach (IComposter composter in Composters)
            {
                var asd = composter as PlantationComposter;
                if (composter.GrowProgress < wanting)
                {
                    taken += composter.GrowProgress;
                    composter.GrowProgress = 0;
                }
                else
                {
                    composter.GrowProgress -= wanting;
                    taken += wanting;
                }
            }
            SoilFactor += taken - 0.01f * factor;
        }

        public override bool Tick(ulong delta)
        {
            //Log.Out("1Tick plant {0}", ID);
            if (!base.Tick(delta))
                return false;
            //Log.Out("2Tick plant {0}", ID);

            DoSoilQualityCheck(delta);

            DoWaterCheck(delta);

            DoSicknessCheck(delta);


            // HasWater = WaterFactor;

            GrowProgress += WaterFactor / 60f * delta * CurrentSunLight;
            //Log.Out("Ticked {0} (loaded: {1}, progress: {2:0}%, light: {3})",
            //    GetBlock().GetBlockName(),
            //    GetIsLoaded(), GrowProgress * 100f, light);
            // if (GrowProgress < 1f) RegisterScheduled();
            // else GrowToNext(world, PlantManager.Instance);

            // Grow to next phase
            if (GrowProgress * 0.001f > 100f)
            {
                Log.Warning("Grow into {0}", FieldNextPlant.Get(BLOCK).Block.GetBlockName());

                BlockValue next = FieldNextPlant.Get(BLOCK);

                next.rotation = Rotation;
                var action = new ExecuteBlockChange();
                action.Setup(WorldPos, next);
                if (Manager == null)
                {
                    Log.Warning("Plant withouzt Manager");
                    return false;
                }
                var manager = Manager; // Remember
                Log.Out("Enqueue to manager {0}", Manager);
                Manager.ToMainThread.Enqueue(action);
                Manager.RemoveManagedNode(WorldPos);
                // AttachToManager(null);

                if (next.Block is BlockPlantGrowing)
                {
                    // Create the next plant right away, in order to
                    // keep background processing running. Otherwise
                    // loaded chunks would trigger remove/add events,
                    // but unloaded blocks will not do that until they
                    // are loaded by their chunks again. We still need
                    // to be idempotent in regard to adding nodes, as
                    // we will still get the added/removed events.
                    Log.Warning("|| Plant moved to grow step");
                    Log.Out("Create new Plant to grow");
                    new PlantationGrowing(WorldPos, next)
                    {
                        WaterFactor = WaterFactor,
                        CurrentRain = CurrentRain,
                        CurrentSunLight = CurrentSunLight,
                        CurrentFertility = CurrentFertility
                    }
                    .AttachToManager(manager);
                }
                // if (next.Block is BlockPlantHarvestable)
                // {
                // }
                else
                {
                    Log.Warning("|| Plant moved to final step");
                }



                return false;
            }
            // else if next plant is harvestable
            return true;
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