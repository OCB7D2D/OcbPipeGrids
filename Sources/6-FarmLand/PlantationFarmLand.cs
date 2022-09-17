using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class PlantationFarmLand : NodeBlock<BlockPlantationFarmLand>, IFarmLand
    {

        public static TYPES NodeType = TYPES.PlantationFarmLand;
        public override uint StorageID => (uint)TYPES.PlantationFarmLand;
        
        //########################################################
        // Config settings (move to block)
        //########################################################

        // public readonly float MaxWaterState = 2f;
        // public readonly float MinWaterState = 0.15f;
        // 
        // private readonly float MaxSoilState = 3f;
        // private readonly float MinSoilState = 0.25f;

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick => 40;

        //########################################################
        //########################################################

        public IPlant Plant { get; set;  } = null;

        //########################################################
        // Custom Data Attributes for Node
        //########################################################

        public byte CurrentSunLight { get; set; } = 0;

        public float CurrentRain { get; set; } = 0;

        public float WaterState { get; set; } = 1;

        public float SoilState { get; set; } = 1;

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IComposter> Composters { get; }
            = new HashSet<IComposter>();

        public void AddLink(IComposter composter)
        {
            Composters.Add(composter);
            composter.FarmLands.Add(this);
        }

        public HashSet<IWell> Wells { get; }
            = new HashSet<IWell>();

        public void AddLink(IWell well)
        {
            Wells.Add(well);
            well.FarmLands.Add(this);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationFarmLand(Vector3i position, BlockValue bv) : base(position, bv)
        {
            CurrentRain = 0f;
            WaterState = 0.5f;
            SoilState = 1.0f;
        }

        public PlantationFarmLand(BinaryReader br)
            : base(br)
        {
            CurrentRain = 0f; // Not saved
            WaterState = br.ReadSingle();
            SoilState = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(WaterState);
            bw.Write(SoilState);
        }

        //########################################################
        // Implementation to integrate with manager
        // Setup data to allow queries where needed
        //########################################################

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveFarmLand(this);
            manager?.AddFarmLand(this);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return string.Format("Soil: {0:.00}, Water: {1:.00}\nWells: {2}, Composts: {3}",
                SoilState, WaterState, Wells.Count, Composters.Count);
        }

        //########################################################
        // Tick only if no plant is yet attached
        //########################################################

        public override bool Tick(ulong delta)
        {
            // Abort ticking if Manager is null
            if (!base.Tick(delta)) return false;
            if (float.IsNaN(SoilState)) SoilState = 0;
            if (float.IsNaN(WaterState)) WaterState = 0;
            // We are ticked by plants if any is there
            // Otherwise tick ourself to fill slowly
            if (Plant == null)
            {
                // Log.Out("Ticking water for soil only");
                // Consume water to keep and improve water state
                TickWater(delta, BLOCK.WaterMaintenance);
                // Consume compost to keep and improve soil state
                TickSoil(delta, BLOCK.SoilMaintenance);
            }
            // Keep ticking
            return true;
        }

        public void TickWater(ulong delta,
            MaintenanceOptions options)
        {
            WaterState = PlantHelper.TickFactor(delta,
                Wells, options, WaterState,
                BLOCK.WaterRange);
        }

        public void TickSoil(ulong delta,
            MaintenanceOptions options)
        {
            SoilState = PlantHelper.TickFactor(delta,
                Composters, options, SoilState,
                BLOCK.SoilRange);
        }

        //########################################################
        //########################################################

    }
}