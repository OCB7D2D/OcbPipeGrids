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

    public abstract class PlantationBase : NodeBlock<BlockPlantationGrowing>, IPlant
    {


        public float HealthFactor { get; set; } = 6.75f;

        private byte sunlight = 0;


        public int Illness => BlockHelper.GetIllness(BV);

        protected void OnHealthFactorChange()
        {
            HealthFactor = Math.Max(0, Math.Min(7, HealthFactor));
            int illness = (int)(7 - HealthFactor); // round down
            if (Illness == illness) return; // nothing changed
            Log.Warning("Plant illness changed from {0} to {1}", Illness, illness);
            var action = new ExecuteBlockChange();
            BlockHelper.SetIllness(ref BV, illness);
            action.Setup(WorldPos, BV);
            Manager.ToMainThread.Enqueue(action);
        }

        public void ChangeHealth(int change)
        {
            HealthFactor += change;
            OnHealthFactorChange();
        }
            // SetIllness(BlockHelper.GetIllness(BV) + change);

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IPlant> Plants { get; }
            = new HashSet<IPlant>();

        public void AddLink(IPlant plant)
        {
            Plants.Add(plant);
            plant.Plants.Add(this);
        }

        public HashSet<ISprinkler> Sprinklers { get; }
            = new HashSet<ISprinkler>();

        public void AddLink(ISprinkler sprinkler)
        {
            Log.Out(" Link sprinkly planty");
            Sprinklers.Add(sprinkler);
            sprinkler.Plants.Add(this);
        }

        public override bool Tick(ulong delta)
        {
            if (!base.Tick(delta)) return false;
            TickSprinkler(delta, BLOCK.SprinklerMaintenance);
            return true;
        }

        public float SprinklerState { get; set; } = 0.0f;
        public float PesticideState { get; set; } = 0.0f;
        public abstract byte CurrentSunLight { get; set; }

        public void TickSprinkler(ulong delta,
           MaintenanceOptions options)
        {
            SprinklerState = PlantHelper.TickFactor(delta,
                Sprinklers.Where(x => x.FluidType == 2).ToList(),
                options, SprinklerState, BLOCK.SprinklerRange, 10f);
            PesticideState = PlantHelper.TickFactor(delta,
                Sprinklers.Where(x => x.FluidType == 3).ToList(),
                options, PesticideState, BLOCK.SprinklerRange, 10f);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationBase(Vector3i position, BlockValue bv)
            : base(position, bv) { }

        public PlantationBase(BinaryReader br)
            : base(br) { }

        //########################################################
        //########################################################
    }

    public class PlantationGrowing : PlantationBase
    {

        byte sunlight = 0;
        public override byte CurrentSunLight
        {
            get { return Reservoir == null ? sunlight : (byte)12; }
            set { sunlight = value; }
        }

        public static TYPES NodeType = TYPES.PlantationGrowing;
        public override uint StorageID => (uint)TYPES.PlantationGrowing;

        public ulong Alive = 0;

        public override ulong NextTick => 50;

        public byte CurrentFertility { get; set; } = 0;

        public float CurrentRain { get; set; } = 0;

        public float GrowProgress = 0;

        // public float WaterState = 0.5f;

        public float MinSoilState = 0.03f;
        public float MaxGroundSoil = 3.0f;

        public float MinWaterState = 0.03f;
        public float MaxGroundWater = 2.0f;
        public float MaxWaterState = 5.0f;

        public float SoilState =>
            Reservoir != null ? Reservoir.SoilState :
            FarmLand != null ? FarmLand.SoilState :
            0f;

        public float WaterState =>
            Reservoir != null ? Reservoir.WaterState :
            FarmLand != null ? FarmLand.WaterState :
            0f;

        // public float MinSoilFactor = 0.07f;
        // public float MaxSoilFactor = 3.0f;

        public float SickFactor = 0.01f;

        PlantationFarmLand land;
        PlantationFarmPlot plot;
        private PlantationFarmPlot Reservoir
        {
            get => plot; set
            {
                if (plot != null)
                    plot.Plant = null;
                plot = value;
                if (plot != null)
                    plot.Plant = this;
            }
        }

        private PlantationFarmLand FarmLand
        {
            get => land; set
            {
                if (land != null)
                    land.Plant = null;
                land = value;
                if (land != null)
                    land.Plant = this;
            }
        }


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

        // internal readonly HashSet<PlantationComposter> Composters
        //     = new HashSet<PlantationComposter>();
        private void UpdateIllnesCheck()
            => NextIllnesCheck += UnityEngine.Random
                .Range(900f, 1100f);

        public PlantationGrowing(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            HealthFactor = 6.5f - BlockHelper.GetIllness(bv);
            HealthFactor = Mathf.Max(0f, Mathf.Min(7f, HealthFactor));
            UpdateIllnesCheck();
        }

        public PlantationGrowing(BinaryReader br)
            : base(br)
        {
            SprinklerState = br.ReadSingle();
            PesticideState = br.ReadSingle();
            HealthFactor = br.ReadSingle();
            GrowProgress = br.ReadSingle();
            CurrentSunLight = br.ReadByte();
            CurrentFertility = br.ReadByte();
            CurrentRain = br.ReadSingle();
            Alive = br.ReadUInt64();
            UpdateIllnesCheck();
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            
            bw.Write(SprinklerState);
            bw.Write(PesticideState);
            bw.Write(HealthFactor);
            bw.Write(GrowProgress);
            bw.Write(CurrentSunLight);
            bw.Write(CurrentFertility);
            bw.Write(CurrentRain);
            bw.Write(Alive);
            // bw.Write(Flags);
        }
        
        public override string GetCustomDescription()
        {
            return string.Format("Plant Growing {0:0.00}/{5:0.00}h\nWater: {1:0.00}, Soil: {2:0.00}, Light: {10:0.00}\nGrowth: {6:0.00}, Health: {4:0.00}\nPlants: {3}, Sprinkly: {8:0.00}/{7}\nPest: {9:0.00}",
                GrowProgress, WaterState, SoilState, Plants.Count, HealthFactor, Alive / 3000f, GrowFactor, Sprinklers.Count, SprinklerState, PesticideState, CurrentSunLight);
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            //Log.Out("Attach Man {0} {1} {2}",
            //    ID, Manager, manager);
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemovePlantGrowing(this);
            if (manager == null) return;
            manager?.AddPlantGrowing(this);
            if (manager.TryGetNode(
                WorldPos + Vector3i.down,
                out NodeBase node))
            {
                Reservoir = node as PlantationFarmPlot;
                FarmLand = node as PlantationFarmLand;
            }
        }

        public override void OnAfterLoad()
        {
            base.OnAfterLoad();
            if (Manager.TryGetNode(
                WorldPos + Vector3i.down,
                out NodeBase node))
            {
                Reservoir = node as PlantationFarmPlot;
                FarmLand = node as PlantationFarmLand;
            }
        }

        static readonly HarmonyFieldProxy<BlockValue>
            FieldNextPlant = new HarmonyFieldProxy<BlockValue>(
                typeof(BlockPlantGrowing), "nextPlant");

        float NextIllnesCheck = 0f;

        private void DoSicknessCheck2(ulong delta)
        {

            var sr = Mathf.Pow(EasingFunction.EaseInQuad(1f, 0.75f,
                Mathf.InverseLerp(0, 1, SprinklerState)), 1f);

            float sickness = 8f - HealthFactor / 4f;
            // Check plants nearby to spread sickness
            // Check our soil quality for sickness chance
            foreach (var plant in Plants)
            {
                if (plant == this) Log.Error("Own plant in list");
                // Consider close by plant more often than further away
                var dx = Math.Abs(plant.WorldPos.x - WorldPos.x);
                var dz = Math.Abs(plant.WorldPos.z - WorldPos.z);
                // Euclidean might use too much performance?
                float rnd = UnityEngine.Random.value * 5f;
                float dist = dx * dx + dz * dz;
                if (rnd * rnd < dist) continue;
                sickness += 7f - plant.HealthFactor / 4f;
                // considered += 1;
            }

            // Check if plant is randomly getting sick
            if (UnityEngine.Random.value < sickness * sr * delta * 0.01f / 1200f)
            {
                HealthFactor -= 0.0025f * delta;
            }


            HealthFactor = Mathf.Max(0f,
                Mathf.Min(7f, HealthFactor));

            OnHealthFactorChange();
        }

        public void DoSicknessCheck(ulong delta)
        {

            if (Manager == null)
            {
                Log.Warning("Plant without Manager");
                return;
            }

            NextIllnesCheck -= delta;
            if (NextIllnesCheck > 0) return;
            UpdateIllnesCheck();

            float sickness = 0.2f;
            float considered = 1f;

            // Check plants nearby to spread sickness
            // Check our soil quality for sickness chance
            foreach (var plant in Plants)
            {
                // Consider close by plant more often than further away
                var dx = Math.Abs(plant.WorldPos.x - WorldPos.x);
                var dz = Math.Abs(plant.WorldPos.z - WorldPos.z);
                // Euclidean might use too much performance?
                float rnd = UnityEngine.Random.value * 5f;
                float dist = dx * dx + dz * dz;
                Log.Out("  Chance {0} vs rnd {1}",
                    dist, rnd * rnd);
                if (rnd * rnd < dist) continue;
                sickness += plant.Illness / 8f;
                considered += 1;
            }

            if (considered > 0) sickness /= considered;

            // SoilFactor goes from 0 to 3 max (default 1)
            float soilSickness = 1f - SoilState / 2f;

            // 1 = 100% healing, 0 = 0% healing
            float soilHealthy = SoilState / 3f;

            Log.Out("//// Considered {0} plants of {1} with sickness {2} and soil sickness {3} plus soil health {4}",
                considered, Plants.Count, sickness, soilSickness, soilHealthy);


            // Check clear (rain) water (sprinkler) for healing
            // Check soil quality for healing chance

            if (UnityEngine.Random.value < sickness ||
                UnityEngine.Random.value < soilSickness)
            {
                HealthFactor -= delta / 100f;
                Log.Out(" Plant became more ill");
                // int illness = Illness;
                // if (illness < 7)
                // {
                //     BlockHelper.SetIllness(ref BV, illness + 1);
                //     Log.Warning("Plant now ill {0}", BV.meta2);
                //     var action = new ExecuteBlockChange();
                //     action.Setup(WorldPos, BV);
                //     // Log.Out("Enqueue to manager {0}", Manager);
                //     Manager.ToMainThread.Enqueue(action);
                // }
            }

            else if (UnityEngine.Random.value < soilHealthy)
            {
                HealthFactor += delta / 100f;
                Log.Out(" Plant became less ill");

                //int illness = Illness;
                //if (illness > 0)
                //{
                //    Log.Warning("Plant has healed {0}", WorldPos);
                //    BlockHelper.SetIllness(ref BV, illness - 1);
                //    var action = new ExecuteBlockChange();
                //    action.Setup(WorldPos, BV);
                //    // Log.Out("Enqueue to manager {0}", Manager);
                //    Manager.ToMainThread.Enqueue(action);
                //}
            }

            HealthFactor = Mathf.Max(0f,
                Mathf.Min(7f, HealthFactor));
            OnHealthFactorChange();

            // HealthFactor = Illness;
        }

        private float MixMe(float waterFactor, float newFactor, ulong delta)
        {
            ulong span = 12000;
            waterFactor = waterFactor * (span - delta) +
                newFactor * (delta);
            waterFactor /= span;
            return waterFactor;
        }

        public void UpdateSoilAndWater(ulong delta)
        {

            float factor = delta / 60000f;
            float RainFactor = delta / 60000f;
            // Check if plant needs water to grow
            // Should be the only water grid API use

            // 3% without water
            // GrowthFactor = 0.03f;
            // HasWater = 0.0f;
            float consumedWater = 0;

            // Add water from current rain
            consumedWater += CurrentRain * RainFactor;
            // Consume the rain (once unloaded)
            // ToDo: maybe simulate for unloaded?
            CurrentRain -= RainFactor;

            if (Reservoir != null)
            {
                /*
                float quality = 1f;
                switch (Reservoir.FluidType)
                {
                    case 2: quality = 2f; break;
                    case 3: quality = 3f; break;
                    default: quality = 1f; break;
                }

                float wanted = 0.5f * factor * WaterState;
                consumedWater += Reservoir.ConsumeFluid(wanted);

                float newFactor = consumedWater / wanted * quality;

                WaterState = MixMe(WaterState, newFactor, delta);

                //WaterFactor += consumedWater > 0 ?
                //    delta / 2000f * consumedWater :
                //    -2f * delta / 2000f;

                if (WaterState > MaxWaterState)
                    WaterState = MaxWaterState;
                */

                Log.Out("Ticksche Water and Soil");

                CurrentSunLight = 10;

                Reservoir.TickWater(delta, BLOCK.WaterMaintenance);
                Reservoir.TickSoil(delta, BLOCK.SoilMaintenance);
            }
            // Plants on ground can only get murky water for soil
            // Reason is that natural soil will make it murky again
            else if (FarmLand != null)
            {
                FarmLand.TickWater(delta, BLOCK.WaterMaintenance);
                FarmLand.TickSoil(delta, BLOCK.SoilMaintenance);
            }
            else
            {
                // WaterState = 0f;
            }
        }

        private float GrowFactor = 1;

        public override bool Tick(ulong delta)
        {
            // Abort ticking if Manager is null
            if (!base.Tick(delta)) return false;

            Alive += delta;

            UpdateSoilAndWater(delta);

            DoSicknessCheck2(delta);



            // DoSicknessCheck(delta);

            var wf = Mathf.Pow(EasingFunction.EaseInOutQuad(0.05f, 1.5f,
                Mathf.InverseLerp(0.1f, 2f, WaterState)), 0.65f);
            var sf = Mathf.Pow(EasingFunction.EaseInOutQuad(0.25f, 1.75f,
                Mathf.InverseLerp(0.3f, 5f, SoilState)), 0.25f);
            var lf = Mathf.Pow(EasingFunction.EaseInOutQuad(0f, 1.75f,
                Mathf.InverseLerp(5, 25, CurrentSunLight)), 0.75f);

            var hf = Mathf.Pow(EasingFunction.EaseInOutQuad(0f, 1.25f,
                Mathf.InverseLerp(0, 7, HealthFactor)), 0.75f);

            var illness = Illness;

            // Log.Out("Grow W {0}, S {1}, L {2} of {3} ==> {4} * {5}", wf, sf, lf, BLOCK.GrowthRate, wf * sf * lf, hf);

            GrowFactor = Mathf.Pow(wf * sf * lf * hf, 0.85f);

            GrowProgress += (GrowFactor * BLOCK.GrowthRate - BLOCK.GrowthMaintenanceFactor) * delta;
            
            //Log.Out("Ticked {0} (loaded: {1}, progress: {2:0}%, light: {3})",
            //    GetBlock().GetBlockName(),
            //    GetIsLoaded(), GrowProgress * 100f, light);
            // if (GrowProgress < 1f) RegisterScheduled();
            // else GrowToNext(world, PlantManager.Instance);
            
            
            // Grow to next phase
            if (GrowProgress > 100f)
            {
                Log.Warning("Grow into {0}", FieldNextPlant.Get(BLOCK).Block.GetBlockName());

                BlockValue next = FieldNextPlant.Get(BLOCK);


                next.rotation = Rotation;
                // Inherit Illness from parent
                // Subtract a little on growth
                BlockHelper.SetIllness(ref next,
                    Math.Max(0, illness - 2));
                var action = new ExecuteBlockChange();
                action.Setup(WorldPos, next);
                var manager = Manager; // Remember
                Log.Out("Enqueue to manager {0}", Manager);
                Manager.ToMainThread.Enqueue(action);
                // This will reset Manager to null
                Manager.RemoveManagedNode(WorldPos);
                /*
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
                        CurrentRain = CurrentRain,
                        CurrentSunLight = CurrentSunLight,
                        CurrentFertility = CurrentFertility,
                        HealthFactor = Mathf.Min(7f, HealthFactor + 2),
                    }
                    .AttachToManager(manager);
                }
                */
                // if (next.Block is BlockPlantHarvestable)
                // {
                // }
                //else
                //{
                //    Log.Warning("|| Plant moved to final step");
                //}



                return false;
            }
            else if (GrowProgress < -20f)
            {
                // Wither the plant
            }
            // else if next plant is harvestable
            return true;
        }

    }
}