using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{
    public class PipeIrrigation : PipePump, IPoweredNode, IIrrigator
    {

        BlockPipeIrrigation BLK;
        public bool IsInReach(Vector3i target)
            => ReachHelper.IsInReach(this, target);

        //########################################################
        // Config settings (move to block)
        //########################################################

        public Vector3i BlockReach { get => BLK.BlockReach; set => BLK.BlockReach = value; }
        public Vector3i ReachOffset { get => BLK.ReachOffset; set => BLK.ReachOffset = value; }
        public Color BoundHelperColor { get => BLK.BoundHelperColor; set => BLK.BoundHelperColor = value; }
        public Color ReachHelperColor { get => BLK.ReachHelperColor; set => BLK.ReachHelperColor = value; }

        public Vector3i RotatedReach => FullRotation.Rotate(Rotation, BLK.BlockReach);
        public Vector3i RotatedOffset
        {
            get
            {
                Log.Warning("Get rotated offset {0} {1}", BV.type, BLOCK);
                return FullRotation.Rotate(Rotation, BLK.ReachOffset);
            }
        }

        public Vector3i Dimensions => BLK.multiBlockPos?.dim ?? Vector3i.one;

        public override ulong NextTick => 30;

        public override uint StorageID => 3;

        public PipeIrrigation(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            GetBlock(out BLK);
        }


        public PipeIrrigation(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Irrigation");
            GetBlock(out BLK);
        }

        // Keep a list of plants that get water from us.
        public HashSet<IWell> Wells { get; }
            = new HashSet<IWell>();


        public void AddLink(IWell well)
        {
            Wells.Add(well);
            well.Irrigators.Add(this);
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
