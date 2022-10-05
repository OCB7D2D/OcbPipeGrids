using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{
    public class PlantationFarmPlot : NodeBlock<BlockPlantationFarmPlot>, IFarmPlot
    {

        static int IDs = 0;

        public int ID = IDs++;

        public override ulong NextTick => 5;

        public override uint StorageID => 10;

        public float GrowProgress { get; set; } = 0;

        public HashSet<IPlant> Plants { get; } = new HashSet<IPlant>();

        public PlantationFarmPlot(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationFarmPlot(BinaryReader br)
            : base(br)
        {
            // GrowProgress = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            // bw.Write(GrowProgress);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Farm Plot {0}", GrowProgress);
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
                    GrowProgress += 1f * factor;
                    action.AddChange(chest[i]?.itemValue, -1);
                }
            }
            action.Setup(WorldPos);
            Manager.ToMainThread.Enqueue(action);
            return true;
        }


    }
}