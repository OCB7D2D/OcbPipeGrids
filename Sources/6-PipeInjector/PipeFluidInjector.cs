using System;
using System.Collections.Generic;
using System.IO;

namespace NodeFacilitator
{
    class PipeFluidInjector : PipeReservoir
    {
        public static new TYPES NodeType = TYPES.PipeFluidInjector;
        public override uint StorageID => (uint)NodeType;

        public PipeFluidInjector(BinaryReader br) : base(br)
        {
        }

        public PipeFluidInjector(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
        }

        static readonly HarmonyFieldProxy<Dictionary<TileEntity, int>> FieldLockedTileEntities =
            new HarmonyFieldProxy<Dictionary<TileEntity, int>>(typeof(GameManager), "lockedTileEntities");

        public override bool Tick(ulong delta)
        {
            if (!base.Tick(delta)) return false;

            // Tell main thread to remove items from chest
            //  only works if the chest is actually loaded
            //  but also only case we need the main thread
            // If TE is not loaded, we use the cached state
            var GM = GameManager.Instance;
            var world = GM.World as World;
            var te = world.GetTileEntity(0, WorldPos);
            if (te != null)
            {
                var locks = FieldLockedTileEntities.Get(GameManager.Instance);
                if (locks.ContainsKey(te))
                {
                    Log.Out("Chest is locked, postpone tick");
                    return true;
                }
            }


            // Skip if FillState is still at Maximum
            if (FillLevel > MaxFillState - 1) return true;
            // Reset FluidType if empty
            if (FillLevel <= 0) FluidType = 0;
            // Try to get (cached) chest from Manager
            // Will sync automatically behind the scenes
            // This doesn't work when chest is locked
            // Can lead to ghost filling being created
            if (Manager.GetChest(WorldPos) is ItemStack[] inv)
            {
                Log.Out("Tick Injector");
                var action = new ExecuteChestModification();
                // Log.Out("Inventory {0}", inv.Length);
                for (int n = 0; n < 12; n++)
                {
                    if (FillLevel > MaxFillState - 1) continue;
                    for (int i = 0; i < inv.Length; i++)
                    {
                        if (inv[i].count <= 0) continue;
                        ItemClass ic = inv[i]?.itemValue?.ItemClass;
                        //Log.Warning("check material {0}, {1} => {2}",
                        //    inv[i]?.itemValue, ic, ic?.Name);
                        short take = (short)-Math.Min(inv[i].count, 1);
                        if ((FillLevel < 1e-3 || FluidType == 1) &&
                            ic?.Name == "drinkJarRiverWater")
                        {
                            // Fluid is murky water
                            if (AddFillLevel(-take, 1))
                            {
                                inv[i].count += take;
                                action.AddChange(inv[i]?.itemValue, take);
                            }
                            else Log.Warning("Could not inject murky");
                            break;
                        }
                        else if ((FillLevel < 1e-3 || FluidType == 2) &&
                            ic?.Name == "drinkJarBoiledWater")
                        {
                            // Fluid is clean water
                            if (AddFillLevel(-take, 2))
                            {
                                inv[i].count += take;
                                // Log.Warning("Taking Water Jar[{1}] {0}", take, i);
                                action.AddChange(inv[i]?.itemValue, take);
                            }
                            else Log.Warning("Could not inject water");
                            break;
                        }
                        else if ((FillLevel < 1e-3 || FluidType == 3) &&
                            ic/*?*/.Name.StartsWith("resourcePesticide"))
                        {
                            // Fluid is pesticide
                            if (AddFillLevel(-take, 3))
                            {
                                inv[i].count += take;
                                action.AddChange(inv[i]?.itemValue, take);
                            }
                            else Log.Warning("Could not inject pesticide");
                            break;
                        }
                    }
                }
                if (action.Changes.Count > 0)
                {
                    action.Setup(WorldPos);
                    Manager.PushToMother(action);
                }
                // Log.Out(" && Injected Water");
            }

            return true;
        }
    }

}