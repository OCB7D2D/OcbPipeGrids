using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NodeManager
{
    public class PipeFluidConverter : PipePump, IPoweredNode
    {

        public static new TYPES NodeType = TYPES.PipeFluidConverter;
        public override uint StorageID => (uint)TYPES.PipeFluidConverter;

        public override ulong NextTick => 30;

        public PipeFluidConverter(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            // Clean Water
            SetFluidType(2);
        }


        public PipeFluidConverter(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Fluid Converter");
            // Clean Water
            SetFluidType(2);
        }

        // Keep a list of plants that get water from us.
        internal readonly HashSet<PipeWell> Wells
            = new HashSet<PipeWell>();

        public override string GetCustomDescription()
        {
            return string.Format("Irrigation {0}",
                base.GetCustomDescription());
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            // Manager?.RemoveFluidConverer(this);
            // manager?.AddFluidConverter(this);
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
