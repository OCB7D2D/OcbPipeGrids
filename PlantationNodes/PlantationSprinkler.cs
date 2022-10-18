﻿using System.Collections.Generic;
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
        public bool IsInReach(Vector3i target, bool check) => ReachHelper.IsInReach(this, target, check);

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick =>
            (ulong)Random.Range(30, 40);

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

        public void AddLink(IPlant plant)
        {
            Log.Out(" Link planty sprinkly");
            Plants.Add(plant);
            plant.Sprinklers.Add(this);
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
            return string.Format("Sprinkler {0} => {1}",
                FillState, Plants.Count);
        }

        //########################################################
        //########################################################

        int waited = -400;

        public override bool Tick(ulong delta)
        {
            // Log.Out("Tick Composter {0}", delta);
            // Abort ticking if Manager is null
            if (!base.Tick(delta)) return false;
            
            waited += (int)delta;
                
            if (waited > 0)
            {
                var enable = !BlockHelper.GetEnabled(BV);
                if (FillState <= 0.02) enable = false;
                if (BlockHelper.GetEnabled(BV) != enable)
                {
                    BlockHelper.SetEnabled(ref BV, enable);
                    var action = new ExecuteBlockChange();
                    action.Setup(WorldPos, BV);
                    Manager.ToMainThread.Enqueue(action);
                }
                if (enable) waited = -300;
                else waited = -500;
            }

            // Keep ticking
            return true;
        }

        //########################################################
        //########################################################
    }
}