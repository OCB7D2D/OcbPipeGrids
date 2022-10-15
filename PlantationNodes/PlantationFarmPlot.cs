using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{

    public class PlantationFarmPlot : PipeReservoir, IFarmPlot
    {

        public IPlant Plant { get; set; } = null;
        public byte CurrentSunLight { get; set; } = 0;

        public override ulong NextTick => 5;

        public override uint StorageID => 10;

        public float CurrentRain { get; set; } = 0;

        public float WaterState { get; set; } = 0.5f;

        public float SoilState { get; set; } = 1f;

        public HashSet<IComposter> Composters { get; } = new HashSet<IComposter>();

        public void AddLink(IComposter composter)
        {
            composter.FarmPlots.Add(this);
        }

        public PlantationFarmPlot(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationFarmPlot(BinaryReader br)
            : base(br)
        {
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

        public override string GetCustomDescription()
        {
            return string.Format("Soil: {0:.00}, Water: {1:.00}\n{2}",
                SoilState, WaterState, base.GetCustomDescription());
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            //Log.Out("Attach Man {0} {1} {2}",
            //    ID, Manager, manager);
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveFarmPlot(this);
            manager?.AddFarmPlot(this);
        }

        public override bool Tick(ulong delta)
        {

            var factor = delta / 100f;

            if (Manager == null)
            {
                Log.Out("No manager");
                return true;
            }
            if (!base.Tick(delta))
                return false;


            var chest = Manager.GetChest(WorldPos);
            if (chest == null) {
                return true;
            }
            var action = new ExecuteChestModification();
            for (int i = 0; i < chest.Length; i++)
            {
                if (chest[i].count <= 0) continue;
                var ic = chest[i]?.itemValue?.ItemClass;
                if (ic?.MadeOfMaterial?.ForgeCategory == "plants")
                {
                    //GrowProgress += 1f * factor;
                    action.AddChange(chest[i]?.itemValue, -1);
                }
            }
            action.Setup(WorldPos);
            Manager.ToMainThread.Enqueue(action);
            return true;
        }

    }
}