using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{
    public class PlantationSprinkler : PipeReservoir, ISprinkler
    {

        public static new TYPES NodeType = TYPES.PlantationSprinkler;
        public override uint StorageID => (uint)TYPES.PlantationSprinkler;

        public new BlockPlantationSprinkler BLOCK = null;
        public IReacherBlock RBLK => BLOCK;

        //########################################################
        // Implementation for `IReachable` (redirect to block)
        //########################################################

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

        public override ulong NextTick =>
            (ulong)Random.Range(300, 400);

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IPlant> Plants { get; }
            = new HashSet<IPlant>();

        public override void ParseBlockConfig()
        {
            GetBlock(out BLOCK);
            base.ParseBlockConfig();
        }

        public void AddLink(IPlant soil)
        {
            Plants.Add(soil);
            //soil.Sprinkler.Add(this);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationSprinkler(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public PlantationSprinkler(BinaryReader br)
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
            Manager?.RemoveSprinkler(this);
            manager?.AddSprinkler(this);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return string.Format("Sprinkler {0}", FillState);
        }

        //########################################################
        //########################################################

        public override bool Tick(ulong delta)
        {
            // Log.Out("Tick Composter {0}", delta);
            // Abort ticking if Manager is null
            if (!base.Tick(delta)) return false;
            // Keep ticking
            return true;
        }

        //########################################################
        //########################################################
    }
}