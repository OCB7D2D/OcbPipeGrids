using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class PlantationGrowLight : NodeBlock<BlockPlantationGrowLight>, IGrowLight
    {

        public static TYPES NodeType = TYPES.PlantationLight;
        public override uint StorageID => (uint)NodeType;

        public new BlockPlantationGrowLight BLOCK = null;
        public IReacherBlock RBLK => BLOCK;

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick =>
            (ulong)Random.Range(30, 40);

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IFarmPlot> FarmPlots { get; }
            = new HashSet<IFarmPlot>();

        public void AddLink(IFarmPlot soil)
        {
            FarmPlots.Add(soil);
            soil.GrowLights.Add(this);
        }

        public override void ParseBlockConfig()
        {
            GetBlock(out BLOCK);
            base.ParseBlockConfig();
        }


        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationGrowLight(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationGrowLight(BinaryReader br)
            : base(br)
        {
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
        }

        //########################################################
        // Implementation to integrate with manager
        // Setup data to allow queries where needed
        //########################################################

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveGrowLight(this);
            manager?.AddGrowLight(this);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return string.Format("Grow Light");
        }

        //########################################################
        //########################################################

        // sint waited = -400;

        public override bool Tick(ulong delta)
        {

            BlockHelper.SetEnabled2(ref BV, true);

            // Log.Out("Tick Composter {0}", delta);
            // Abort ticking if Manager is null
            if (!base.Tick(delta)) return false;
            return true;

            // waited += (int)delta;
            //     
            // if (waited > 0)
            // {
            //     var enable = !BlockHelper.GetEnabled(BV);
            //     BlockHelper.SetEnabled(ref BV, enable);
            //     BlockHelper.SetEnabled2(ref BV, enable);
            //     var action = new ExecuteBlockChange();
            //     action.Setup(WorldPos, BV);
            //     Manager.ToMainThread.Add(action);
            //     if (enable) waited = -300;
            //     else waited = -500;
            // }
            // 
            // // Keep ticking
            // return true;
        }

        //########################################################
        //########################################################
    }
}