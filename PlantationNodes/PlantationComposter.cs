using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeManager
{
    public class PlantationComposter : LootBlock<BlockComposter>, IComposter, ILootChest
    {

        public bool IsInReach(Vector3i target)
            => ReachHelper.IsInReach(this, target);

        //########################################################
        // Config settings (move to block)
        //########################################################

        public Vector3i BlockReach { get => BLOCK.BlockReach; set => BLOCK.BlockReach = value; }
        public Vector3i ReachOffset { get => BLOCK.ReachOffset; set => BLOCK.ReachOffset = value; }
        public Color BoundHelperColor { get => BLOCK.BoundHelperColor; set => BLOCK.BoundHelperColor = value; }
        public Color ReachHelperColor { get => BLOCK.ReachHelperColor; set => BLOCK.ReachHelperColor = value; }

        public Vector3i RotatedReach => FullRotation.Rotate(Rotation, BLOCK.BlockReach);
        public Vector3i RotatedOffset { get {
                Log.Warning("Get rotated offset {0} {1}", BV.type, BLOCK);
                return FullRotation.Rotate(Rotation, BLOCK.ReachOffset);
            } }

        public Vector3i Dimensions => BLOCK.multiBlockPos?.dim ?? Vector3i.one;

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick => 5;

        public override uint StorageID => 12;

        public float FillState { get; set; } = 0;
        public float MaxFillState { get; set; } = 5;

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<ISoil> Soils { get; } = new HashSet<ISoil>();

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationComposter(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            FillState = 2;
        }

        public PlantationComposter(BinaryReader br)
            : base(br)
        {
            FillState = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(FillState);
        }

        //########################################################
        // Implementation to integrate with manager
        // Setup data to allow queries where needed
        //########################################################

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveComposter(this);
            manager?.AddComposter(this);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return string.Format("Composter {0} for {1}", FillState, Soils.Count);
        }

        //########################################################
        // Tick to check if more compost is due
        //########################################################

        public override bool Tick(ulong delta)
        {

            // Log.Warning("Tick composter");

            if (Manager == null)
            {
                Log.Out("No manager");
                // base.Tick does check
            }
            if (!base.Tick(delta))
                return false;



            if (FillState > MaxFillState - 1)
            {
                //Log.Out("Still enough fill material {0} vs {1}",
                //    FillState, MaxFillState);
                return true;
            }

            var inv = Manager.GetChest(WorldPos);
            if (inv == null)
            {
                Log.Out("Inventory not found!?");
                return true;
            }
            var action = new ExecuteChestModification();
            //Log.Out("Inventory {0}", inv.Length);
            for (int i = 0; i < inv.Length; i++)
            {
                if (inv[i].count <= 0) continue;
                ItemClass ic = inv[i]?.itemValue?.ItemClass;
                Log.Warning("check material {0}, {1} => {2}",
                    inv[i]?.itemValue, ic, ic?.MadeOfMaterial);
                if (ic?.MadeOfMaterial?.ForgeCategory == "plants")
                {
                    FillState += 1f; // constant exchange factor
                    Log.Warning("Added Fill");
                    action.AddChange(inv[i]?.itemValue, -1);
                }
            }
            if (action.Changes.Count > 0)
            {
                action.Setup(WorldPos);
                Manager.ToMainThread.Enqueue(action);
            }
            return true;
        }

        public void AddLink(ISoil soil)
        {
            throw new System.NotImplementedException();
        }
    }
}