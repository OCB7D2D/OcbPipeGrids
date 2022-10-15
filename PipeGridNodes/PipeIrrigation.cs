using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{
    public class PipeIrrigation : PipePump, IPoweredNode, IIrrigator
    {

        //########################################################
        // Config settings from block
        //########################################################

        // Ideally we could inherit from PipePump to pass the
        // block class further down, but it seems not easy.
        new readonly BlockPipeIrrigation BLOCK;

        public Vector3i BlockReach { get => BLOCK.BlockReach; set => BLOCK.BlockReach = value; }
        public Vector3i ReachOffset { get => BLOCK.ReachOffset; set => BLOCK.ReachOffset = value; }
        public Color BoundHelperColor { get => BLOCK.BoundHelperColor; set => BLOCK.BoundHelperColor = value; }
        public Color ReachHelperColor { get => BLOCK.ReachHelperColor; set => BLOCK.ReachHelperColor = value; }
        public Vector3i RotatedReach => FullRotation.Rotate(Rotation, BLOCK.BlockReach);
        public Vector3i RotatedOffset => FullRotation.Rotate(Rotation, BLOCK.ReachOffset);
        public Vector3i Dimensions => BLOCK.multiBlockPos?.dim ?? Vector3i.one;
        public bool IsInReach(Vector3i target) => ReachHelper.IsInReach(this, target);

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick => 30;

        public override uint StorageID => 3;

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

        public PipeIrrigation(Vector3i position, BlockValue bv)
            : base(position, bv)
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
