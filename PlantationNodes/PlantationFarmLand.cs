using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{

    public class PlantationFarmLand : NodeBlock<BlockPlantationFarmLand>, IFarmLand
    {

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick => 5;

        public override uint StorageID => 8;

        //########################################################
        // Custom Data Attributes for Node
        //########################################################

        public float CurrentRain { get; set; }

        public float WaterFactor { get; set; }

        public float SoilFactor { get; set; }

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IComposter> Composters { get; }
            = new HashSet<IComposter>();

        public HashSet<IWell> Wells { get; }
            = new HashSet<IWell>();

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationFarmLand(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            CurrentRain = 0f;
            WaterFactor = 0.5f;
            SoilFactor = 1.0f;
        }

        public PlantationFarmLand(BinaryReader br)
            : base(br)
        {
            CurrentRain = 0f; // Not saved
            WaterFactor = br.ReadSingle();
            SoilFactor = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(WaterFactor);
            bw.Write(SoilFactor);
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
            return string.Format("Soil: {0:.00}, Water: {1:.00}\nWells: {2}",
                SoilFactor, WaterFactor, Wells.Count);
        }

        //########################################################
        //########################################################

        public override bool Tick(ulong delta)
        {
           
            var scale = NodeManager.TimeScale(delta);

            // This gives a variation from 0.4 to 0.8
            float want = Mathf.Pow(0.5f, 1f / Wells.Count) * 0.8f;
            Log.Out("Wanting {0}", want);
            if (WaterFactor > 0.5) want *= WaterFactor;
            var taken = WellHelper.ConsumeFluids(Wells, want * scale);
            Log.Out("Taken {0} , inv {1}", taken, taken / scale);
            var score = taken / WaterFactor / scale * 3;

            // Consume some rain (or maximal all of it)
            //float rain = Mathf.Min(scale * 10f, 1f);
            //taken += CurrentRain * scale * rain;
            //CurrentRain -= CurrentRain * scale * rain;

            WaterFactor = WellHelper.MaxFluids(
                WaterFactor, score, delta, 12000);

            // Enforce maximum and minimum values
            if (WaterFactor > 2f) WaterFactor = 2f;
            else if (WaterFactor < 0.03f) WaterFactor = 0.03f;


            Log.Out("Tick Farm Land");
            return true;
        }

    }
}