using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{
    class PipeFluidInjector : PipeReservoir
    {
        public static new TYPES NodeType = TYPES.PipeFluidInjector;
        public override uint StorageID => (uint)TYPES.PipeFluidInjector;

        public PipeFluidInjector(BinaryReader br) : base(br)
        {
        }

        public PipeFluidInjector(Vector3i position, BlockValue bv) : base(position, bv)
        {
        }

        public override bool Tick(ulong delta)
        {
            if (!base.Tick(delta)) return false;

            // Skip if FillState is still at Maximum
            if (FillState > MaxFillState - 1) return true;
            // Reset FluidType if empty
            if (FillState <= 0) FluidType = 0;
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
                        inv[i]?.itemValue, ic, ic?.Name);
                    short take = (short)-Math.Min(inv[i].count, 5);
                    if ((FillState == 0 || FluidType == 1) &&
                        ic?.Name == "drinkJarRiverWater")
                    {
                        FillState -= take; // constant exchange factor
                        FluidType = 1; // Fluid is murky water
                        inv[i].count += take;
                        action.AddChange(inv[i]?.itemValue, take);
                    }
                    else if ((FillState == 0 || FluidType == 2) &&
                        ic?.Name == "drinkJarBoiledWater")
                    {
                        FillState -= take; // constant exchange factor
                        FluidType = 2; // Fluid is clean water
                        inv[i].count += take;
                        action.AddChange(inv[i]?.itemValue, take);
                    }
                }
                if (action.Changes.Count > 0)
                {
                    action.Setup(WorldPos);
                    Manager.ToMainThread.Enqueue(action);
                }
            }
            
            return true;
        }
    }

}