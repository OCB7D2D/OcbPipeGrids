using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class SprinklerIndoor : PipeReservoir, ISprinkler
    {

        public static new TYPES NodeType = TYPES.SprinklerIndoor;
        public override uint StorageID => (uint)NodeType;

        public new BlockSprinklerIndoor BLOCK = null;
        public IReacherBlock RBLK => BLOCK;

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

        public SprinklerIndoor(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        public SprinklerIndoor(BinaryReader br)
            : base(br)
        {
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
            Log.Error("Read the sprinkler {0}", BV.meta2);
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
                FillLevel, Plants.Count);
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
                if (FillLevel <= 0.02) enable = false;
                if (BlockHelper.GetEnabled(BV) != enable)
                {
                    Log.Out("Adjust sprinkler {0}", BV.meta2);
                    BlockHelper.SetEnabled(ref BV, enable);
                    Log.Out("Adjusted sprinkler {0}", BV.meta2);
                    var action = new ExecuteBlockChange();
                    action.Setup(WorldPos, BV);
                    Manager.PushToMother(action);
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