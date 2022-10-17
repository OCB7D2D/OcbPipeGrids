using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{

    public class PlantationFarmPlot : PipeReservoir, IFarmPlot
    {

        public static new TYPES NodeType = TYPES.PlantationFarmPlot;
        public override uint StorageID => (uint)TYPES.PlantationFarmPlot;

        //########################################################
        // Config settings (move to block)
        //########################################################

        // Ideally we could inherit from PipePump to pass the
        // block class further down, but it seems not easy.
        new readonly BlockPlantationFarmPlot BLOCK;

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick => 5;

        //########################################################
        // Custom Data Attributes for Node
        //########################################################

        public IPlant Plant { get; set; } = null;

        public byte CurrentSunLight { get; set; } = 0;

        public float CurrentRain { get; set; } = 0;

        public float WaterState { get; set; } = 0.5f;

        public float SoilState { get; set; } = 1f;

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IComposter> Composters { get; }
            = new HashSet<IComposter>();

        public void AddLink(IComposter composter)
        {
            composter.FarmPlots.Add(this);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationFarmPlot(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            GetBlock(out BLOCK);
        }

        public PlantationFarmPlot(BinaryReader br)
            : base(br)
        {
            WaterState = br.ReadSingle();
            SoilState = br.ReadSingle();
            GetBlock(out BLOCK);
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(WaterState);
            bw.Write(SoilState);
        }

        //########################################################
        // Implementation to integrate with manager
        // Setup data to allow queries where needed
        //########################################################

        protected override void OnManagerAttached(NodeManager manager)
        {
            //Log.Out("Attach Man {0} {1} {2}",
            //    ID, Manager, manager);
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveFarmPlot(this);
            manager?.AddFarmPlot(this);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return string.Format("Soil: {0:.00}, Water: {1:.00}\n{2}",
                SoilState, WaterState, base.GetCustomDescription());
        }

        //########################################################
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

        //########################################################
        //########################################################

        public void TickWater(ulong delta,
           MaintenanceOptions options)
        {
            WaterState = PlantHelper.ConsumeFactor(delta,
                this, options, WaterState,
                BLOCK.WaterRange, 10f);
        }

        public void TickSoil(ulong delta,
            MaintenanceOptions options)
        {
            SoilState = PlantHelper.TickFactor(delta,
                Composters, options, SoilState,
                BLOCK.SoilRange, 3f);
        }

        //########################################################
        //########################################################

    }
}