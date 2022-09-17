using System.Collections.Generic;
using UnityEngine;

namespace NodeFacilitator
{

    public interface IWorldPos : IEqualityComparer<IWorldPos>
    {
        Vector3i WorldPos { get; }
    }

    public interface IManagerNode : IWorldPos
    {

    }

    public interface IWorldRotated : IWorldPos
    {
        byte Rotation { get; }
    }

    public interface IWorldLink<T> : IWorldRotated
    {
        void AddLink(T item);
    }

    public struct MaintenanceOptions
    {
        public float MaintenanceFactor;
        public float MaintenanceExponent;
        public float ImprovementFactor;

        public MaintenanceOptions(
            float MaintenanceFactor,
            float MaintenanceExponent,
            float ImprovementFactor)
        {
            this.MaintenanceFactor = MaintenanceFactor / 1200f;
            this.MaintenanceExponent = MaintenanceExponent;
            this.ImprovementFactor = ImprovementFactor / 1200f;
        }

        public void Init(DynamicProperties properties, string prefix)
        {
            if (properties.Contains($"{prefix}ImprovementFactor")) ImprovementFactor =
                float.Parse(properties.GetString($"{prefix}ImprovementFactor")) / 1200f;
            if (properties.Contains($"{prefix}MaintenanceFactor")) MaintenanceFactor =
                float.Parse(properties.GetString($"{prefix}MaintenanceFactor")) / 1200f;
            if (properties.Contains($"{prefix}MaintenanceExponent")) MaintenanceExponent =
                float.Parse(properties.GetString($"{prefix}MaintenanceExponent"));
        }

    }

    public struct RangeOptions
    {
        public float Min;
        public float Max;

        public RangeOptions(
            float Min = 0f,
            float Max = 1f)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public void Init(DynamicProperties properties, string prefix)
        {
            if (properties.Contains($"{prefix}RangeMin")) Min =
                float.Parse(properties.GetString($"{prefix}RangeMin"));
            if (properties.Contains($"{prefix}RangeMax")) Max =
                float.Parse(properties.GetString($"{prefix}RangeMax"));
        }

    }

    public interface ISprinklerBlock : IReacherBlock
    {

    }

    // Item that has reach (potentially rotated)
    public interface IWellBlock : IBlockNode
    {
        float FromGround { get; set; }
        float FromFreeSky { get; set; }
        float FromWetSurface { get; set; }
        float FromSnowfall { get; set; }
        float FromRainfall { get; set; }
        float FromIrrigation { get; set; }
        float MaxWaterLevel { get; set; }
    }



    public interface IPlantGrowingBlock
    {
        Block BLK { get; }
        Vector2 PlantScaleMin { get; set; }
        Vector2 PlantScaleFactor { get; set; }

    }

    public interface IPlantBlock : IBlockNode
    {

        float FixedOffset { get; set; }


        MaintenanceOptions SoilMaintenance { get; set; }
        MaintenanceOptions WaterMaintenance { get; set; }
        MaintenanceOptions SprinklerMaintenance { get; set; }
        // float SoilMaintenanceFactor { get; set; }
        // float SoilMaintenanceExponent { get; set; }
        // float SoilImprovementFactor { get; set; }
        // float WaterMaintenanceFactor { get; set; }
        // float WaterMaintenanceExponent { get; set; }
        // float WaterImprovementFactor { get; set; }
        float GrowthMaintenanceFactor { get; set; }
        float LightMaintenance { get; set; }
        string IllnessEffect { get; set; }
    }

    public interface IFilledState : INodeReset
    {
        ushort FluidType { get; set; }
        float MaxFillState { get; set; }
        float FillLevel { get; set; }
    }

    public interface IFilledBlock : IBlockNode
    {
        void UpdateVisualState(Vector3i position, float state);
        float MaxFillState { get; set; }
    }

    public struct ReachConfig
    {
        public Vector3i BlockReach { get; set; }
        public Vector3i ReachSideway { get; set; }
        public Vector3i ReachUpside { get; set; }
        public Vector3i ReachOffset { get; set; }
        public Vector3i OffsetSideway { get; set; }
        public Vector3i OffsetUpside { get; set; }
    }

    public interface IReacherBlock : IBlockNode
    {
        Vector3i Dimensions { get; }
        ReachConfig Reach { get; set; }
        Color BoundHelperColor { get; set; }
        Color ReachHelperColor { get; set; }
    }

    public interface IReacher : IWorldRotated
    {
        IReacherBlock RBLK { get; }
    }



    public interface IReservoir : IFilled
    {
        ushort FluidType { get; }
        void SetFluidType(ushort type);
    }


    public interface IFillStateVisual
    {

        void OnFillStateChanged(Vector3i position, float level);

        // ushort FluidType { get; }
    }

    public interface IFilled
    {
        float FillLevel { get; set; }

        // float AddWater { get; set; }
        // ushort FluidType { get; }
    }

    public interface IHasPlant { IPlant Plant { get; set; } }
    public interface IHasPower { bool IsPowered { get; set; } }
    public interface IHasWells : IWorldLink<IWell> { HashSet<IWell> Wells { get; } }
    public interface IHasPlants : IWorldLink<IPlant> { HashSet<IPlant> Plants { get; } }
    public interface IHasGrowLights : IWorldLink<IGrowLight> { HashSet<IGrowLight> GrowLights { get; } }
    public interface IHasIrrigator : IWorldLink<IIrrigator> { HashSet<IIrrigator> Irrigators { get; } }
    public interface IHasSprinklers : IWorldLink<ISprinkler> { HashSet<ISprinkler> Sprinklers { get; } }
    public interface IHasComposters : IWorldLink<IComposter> { HashSet<IComposter> Composters { get; } }
    public interface IHasFarmLands : IWorldLink<IFarmLand> { HashSet<IFarmLand> FarmLands { get; } }
    public interface IHasFarmPlots : IWorldLink<IFarmPlot> { HashSet<IFarmPlot> FarmPlots { get; } }
    // public interface IHasFarmSoil : IWorldLink<IFarmSoil> { HashSet<IFarmSoil> Soils { get; } }

    // The composter has soils linked that will use compost from it

    public interface ISprinkler : IHasPlants, IReservoir, IReacher, IEqualityComparer<IWorldPos> { }
    public interface IGrowLight : IHasFarmPlots, IReacher, IEqualityComparer<IWorldPos> { }
    
    public interface IComposter : IHasFarmLands, IHasFarmPlots, IFilled, IReacher, IEqualityComparer<IWorldPos> {}


    // So far a well is pretty much the same interface as the composter, so use it
    // It additionally has the SunLight and the linked irrigators we consume from
    public interface IWell : IHasIrrigator, IHasFarmLands, IFilled, IReacher, ISunLight, IEqualityComparer<IWorldPos>
    {
    }

    public interface IFarmLand : IFarmSoil, IHasWells, IWorldRotated { }

    public interface IFarmPlot : IFarmSoil, IFilled, IHasGrowLights, IWorldRotated
    {
        // float WaterState { get; }
        // float SoilState { get; }

    }
    
    public interface IFarmSoil : IHasPlant, IHasComposters,
        /*ISunLight, */ ITickable, IfaceGridNodeManaged, IEqualityComparer<IWorldPos>
    {
        float WaterState { get; }
        float SoilState { get; }
    }



    public interface IPlant : IWorldRotated, IHasPlants, IHasSprinklers, ISunLight, ITickable, IStateNode, IfaceGridNodeManaged, IEqualityComparer<IWorldPos>
    {
        int Illness { get; }
        float HealthFactor { get; }
        void ChangeHealth(int change);
    }

    public interface IIrrigator : IHasWells, IFilled, IReacher, IHasPower, ITickable, IfaceGridNodeManaged, IEqualityComparer<IWorldPos>
    {

    }


}