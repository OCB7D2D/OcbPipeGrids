using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class PipeIrrigation : PipePump, IPoweredNode, IIrrigator
    {

        public IReacherBlock RBLK => BLOCK;

        public static new TYPES NodeType = TYPES.PipeIrrigation;
        public override uint StorageID => (uint)TYPES.PipeIrrigation;

        //########################################################
        // Config settings from block
        //########################################################

        // Ideally we could inherit from PipePump to pass the
        // block class further down, but it seems not easy.
        new readonly BlockIrrigation BLOCK;

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick => 30;

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IWell> Wells { get; }
            = new HashSet<IWell>();

        public void AddLink(IWell well)
        {
            Wells.Add(well);
            well.Irrigators.Add(this);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PipeIrrigation(Vector3i position, BlockValue bv) : base(position, bv)
        {
            GetBlock(out BLOCK);
        }


        public PipeIrrigation(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Irrigation");
            GetBlock(out BLOCK);
        }


        public override string GetCustomDescription()
        {
            return string.Format("Irrigation {0}",
                base.GetCustomDescription());
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveIrrigation(this);
            manager?.AddIrrigation(this);
        }

        protected override void UpdateGrid(PipeGrid grid)
        {
            base.UpdateGrid(grid);
            Log.Out("Update irrigation grid");
        }

        public override bool Tick(ulong delta)
        {
            Log.Warning("Tick irrigation");
            return base.Tick(delta);
        }

    }
}
