using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class PlantationComposter : LootBlock<BlockComposter>, IComposter, ILootChest, IStateNode
    {

        public static TYPES NodeType = TYPES.PlantationComposter;
        public override uint StorageID => (uint)TYPES.PlantationComposter;

        public IReacherBlock RBLK => BLOCK;

        // public ushort FluidType => 0;


        //########################################################
        // Implementation for `IFilled` interface
        //########################################################

        public float FillLevel { get; set; } = 0;

        //########################################################
        // Setup for node manager implementation
        //########################################################

        public override ulong NextTick =>
            (ulong)Random.Range(300, 400);

        //########################################################
        // Cross references setup by manager
        //########################################################

        public HashSet<IFarmLand> FarmLands { get; }
            = new HashSet<IFarmLand>();

        public void AddLink(IFarmLand soil)
        {
            FarmLands.Add(soil);
            soil.Composters.Add(this);
        }

        public HashSet<IFarmPlot> FarmPlots { get; }
            = new HashSet<IFarmPlot>();

        public void AddLink(IFarmPlot soil)
        {
            FarmPlots.Add(soil);
            soil.Composters.Add(this);
        }

        //########################################################
        // Implementation for persistence and data exchange
        //########################################################

        public PlantationComposter(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            FillLevel = 2;
        }

        public PlantationComposter(BinaryReader br)
            : base(br)
        {
            FillLevel = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(FillLevel);
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
            return string.Format("Composter {0} for {1}",
                FillLevel, FarmLands.Count + FarmPlots.Count);
        }

        //########################################################
        //########################################################

        public override bool Tick(ulong delta)
        {
            // Log.Out("Tick Composter {0}", delta);
            // Abort ticking if Manager is null
            if (!base.Tick(delta)) return false;
            // Skip if FillState is still at Maximum
            if (FillLevel > BLOCK.MaxFillState - 1) return true;
            // Try to get (cached) chest from Manager
            // Will sync automatically behind the scenes
            if (Manager.GetChest(WorldPos) is ItemStack[] inv)
            {
                var action = new ExecuteChestModification();
                // Log.Out("Inventory {0}", inv.Length);
                for (int i = 0; i < inv.Length; i++)
                {
                    if (inv[i].count <= 0) continue;
                    ItemClass ic = inv[i]?.itemValue?.ItemClass;
                    Log.Warning("check material {0}, {1} => {2}",
                        inv[i]?.itemValue, ic, ic?.MadeOfMaterial);
                    if (ic?.MadeOfMaterial?.ForgeCategory == "plants")
                    {
                        FillLevel += 1f; // constant exchange factor
                        Log.Warning("Added Fill");
                        inv[i].count -= 1;
                        action.AddChange(inv[i]?.itemValue, -1);
                    }
                }
                if (action.Changes.Count > 0)
                {
                    action.Setup(WorldPos);
                    Manager.PushToMother(action);
                }
            }
            return true;
        }

        //########################################################
        //########################################################

        public IVisualState GetState()
        {
            return new FilledState()
            {
                BID = BLOCK.blockID,
                Position = WorldPos,
                FillState = FillLevel,
            };
        }

        //########################################################
        //########################################################

    }
}