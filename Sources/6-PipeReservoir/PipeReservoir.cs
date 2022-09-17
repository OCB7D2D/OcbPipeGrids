using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class PipeReservoir : PipeConnection, IPoweredNode, IFilledState
    {

        public static new TYPES NodeType = TYPES.PipeReservoir;
        public override uint StorageID => (uint)NodeType;

        public float MaxFillState { get; set; } = 150f;

        public float MinFillState = 0f;

        public bool DisableTick = false;

        private float level = 0f;
        public float FillLevel
        {
            get => level;
            set
            {
                if (value < 1e-3 && level >= 1e-3)
                {
                    level = 0;
                    Grid?.CheckFluidType();
                }
                else if (value >= 1e-3 && level < 1e-3)
                {
                    level = value;
                    Grid?.CheckFluidType();
                }
                else
                {
                    level = value;
                }
            } }

        protected bool AddFillLevel(float change, byte type)
        {
            if (Grid.FluidType != FluidType)
                Log.Warning("Types out of sync");
            if (FluidType == 0)
                FluidType = type;
            if (Grid.FluidType == 0)
                Grid.FluidType = type;
            if (Grid.FluidType != type) return false;
            else if (FluidType != type) return false;
            FillLevel += change;
            if (FillLevel > MaxFillState)
                FillLevel = MaxFillState;
            return true;
        }

        public void Reset()
        {
            FillLevel = 0f;
            FluidType = DefaultFluidType;
            Log.Warning("Reset reservoir 2");
            // Grid?.CheckFluidType();
        }
        public ushort FluidType { get; set; } = 0;

        public ushort DefaultFluidType { get; set; } = 0;

        public override ulong NextTick => 60;

        // public override bool BreakSegment => true;

        public bool IsPowered { get; set; } = true;

        public override void ParseBlockConfig()
        {
            if (BV.Block.Properties.Contains("FluidType")) DefaultFluidType =
                ushort.Parse(BV.Block.Properties.GetString("FluidType"));
            if (BV.Block.Properties.Contains("MaxFillState")) MaxFillState =
                float.Parse(BV.Block.Properties.GetString("MaxFillState"));
            if (BV.Block.Properties.Contains("DisableTick")) DisableTick =
                bool.Parse(BV.Block.Properties.GetString("DisableTick"));
            FluidType = DefaultFluidType;
        }

        public PipeReservoir(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            //if (BV.Block.Properties.Contains("IsPowered")) IsPowered =
            //    bool.Parse(BV.Block.Properties.GetString("IsPowered"));
        }

        public PipeReservoir(
            BinaryReader br)
        : base(br)
        {
            FillLevel = br.ReadSingle();
            FluidType = br.ReadUInt16();
        }

        public virtual void SetFluidType(ushort type)
        {
            if (FluidType != type)
            {
                if (FillLevel > 0)
                {
                    if (FluidType == 0)
                        Log.Warning("Reseting FillState of arbitrary fluid");
                    Log.Warning("Reseting Fluid and clear reservoir fill state");
                    FillLevel = 0f;
                }
                FluidType = type;
            }
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            if (Grid == null) return;
            if (Grid.FluidType != 0 || FluidType != 0)
                if (Grid.FluidType != FluidType)
                    Log.Error("Invalid grid join");
            Grid.FluidType = FluidType;
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(FillLevel);
            bw.Write(FluidType);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Reservoir {0}/{1} ({2}) - {3}",
                FillLevel, MaxFillState, FluidType, Grid);
        }

        protected override void UpdateGrid(PipeGrid grid)
        {
            base.UpdateGrid(grid);
            if (grid == null) return;
            if (grid.FluidType == 0)
                grid.FluidType = FluidType;
            else if (FluidType == 0)
                FluidType = grid.FluidType;
            if (grid.FluidType != FluidType)
                Log.Error("FluidType Mismatch!");
        }

        protected List<PipeReservoir> intake = new List<PipeReservoir>();
        protected List<PipeReservoir> outtake = new List<PipeReservoir>();
        protected List<PipeReservoir> reservoirs = new List<PipeReservoir>();

        public override bool Tick(ulong delta)
        {
            // Log.Out("Tick PipePump at {0} ({1})", WorldPos, IsPowered);
            if (!base.Tick(delta)) return false;
            if (float.IsNaN(FillLevel)) FillLevel = 0;
            //if (DisableTick) return false;
            if (!IsPowered) return true;
            // First need to be filled a little
            // Happens at source or when filled
            // if (FillLevel < 1e-8) return true;
            //if (Grid.FluidType == 0) return true;
            // Prepare variables
            reservoirs.Clear();
            float levels = 0;

            // How much can we pump maximum
            float capacity = 0.08f * delta;
            // Log.Out("Tick PipePump 2");

            if (FillLevel < 1e-3)
                Reset();

            // Determine which fluid type to push around

            // Propagate through the pipe network
            PropagateThroughGrid(null, (cur, prev) =>
            {
                // Check if connection is a reservoir
                if (cur is PipeReservoir reservoir)
                {
                    if (reservoir.FillLevel < 1e-3)
                    {
                        Log.Warning("Reset reservoir 1");
                        //reservoir.FillLevel = 0;
                        //reservoir.FluidType = 0;
                        reservoir.Reset();
                    }
                    if (FluidType == reservoir.FluidType ||
                        reservoir.FluidType == 0 ||
                        FluidType == 0)
                    {
                        if (reservoir.IsPowered)
                        {

                            if (reservoir.FillLevel > 0)
                            {
                                // This must have a fluid type
                                if (reservoir.FluidType == 0)
                                {
                                    Log.Error("Filled without type!???");
                                    reservoir.FillLevel = 0; // Reset fill level
                                }
                                else if (FluidType == 0) FluidType = reservoir.FluidType;
                                if (FluidType != reservoir.FluidType)
                                    Log.Error("Weird stuff is this");
                            }

                            // Check does not scale very well.
                            // Starts to get worse around 32 nodes
                            // Just don't build to comples segments
                            if (!reservoirs.Contains(reservoir))
                            {
                                // Sum up all water levels
                                levels += reservoir.FillLevel;
                                reservoirs.Add(reservoir);
                            }
                            else
                            {
                                // Log.Warning("Skip the fucker");
                            }
                        }
                        else
                        {
                            Log.Out("Skip non powered reservoir");
                        }
                    }
                    else
                    {
                        Log.Warning("Type mismatch {0} vs {1}",
                            FluidType, reservoir.FluidType);
                    }
                    // Abort search at leafs
                    return prev != null;
                }
                // Continue search
                return DisableTick;
            });

            //Log.Out("Tick PipePump X {0} reservoirs at level {1}", reservoirs.Count, levels);

            // Call segment ticker for specialization
            // E.g. towers will try to fill itself up
            TickSegment(capacity, reservoirs);

            // float before = 0;
            // foreach (var asd in reservoirs)
            //     before += asd.FillLevel;

            // Log.Out("- After levels {0}", before);

            // Calculate what the wanted level would be
            float average = levels / reservoirs.Count;
            average = Mathf.Min(MaxFillState, average);

            // Log.Out("Found {0} Reservoirs with avg level {1} (sum {2})",
            //     reservoirs.Count, average, levels);

            intake.Clear();
            outtake.Clear();
            float intakes = 0f;
            float outtakes = 0f;

            foreach (var reservoir in reservoirs)
            {
                // Either take or push fluid to node
                if (reservoir.FillLevel > average)
                {
                    intake.Add(reservoir);
                    intakes += reservoir.FillLevel - average;
                }
                else if (reservoir.FillLevel < average)
                {
                    outtake.Add(reservoir);
                    outtakes += average - reservoir.FillLevel;
                }
            }

            if (intakes == 0) return true;
            if (outtakes == 0) return true;

            // Log.Out("Have {0}/{1} intake and {2}/{3} outtake",
            //     intakes, intake.Count, outtakes, outtake.Count);

            // Calculate how much we are able to pump in and out
            float pump = System.Math.Min(capacity,
                System.Math.Min(intakes, outtakes));

            // Log.Out("Can pump maximum of {0}", pump);


            float taken = 0;
            float out_factor = pump / outtakes;
            foreach (var into in outtake)
            {
                float needed = average - into.FillLevel;
                float wanting = into.MaxFillState - into.FillLevel;
                if (needed > wanting) needed = wanting;
                taken += needed * out_factor;
                if (into.FluidType != FluidType) into.SetFluidType(FluidType);
                into.FillLevel += needed * out_factor;
                float bf = into.FillLevel;
                into.FillLevel = Math.Min(into.MaxFillState, into.FillLevel);
                into.FillLevel = Math.Max(into.MinFillState, into.FillLevel);
                if (bf != into.FillLevel) Log.Warning("Adjusted taken");
                // Log.Out(" Taking {0}", needed * out_factor);
            }

            // Adjust to actual intake
            float in_factor = taken / intakes;
            foreach (var from in intake)
            {
                float available = from.FillLevel - average;
                float reduce = available * in_factor;
                from.FillLevel -= reduce;
                float bf = from.FillLevel;
                from.FillLevel = Math.Min(from.MaxFillState, from.FillLevel);
                from.FillLevel = Math.Max(from.MinFillState, from.FillLevel);
                if (bf != from.FillLevel) Log.Warning("Adjusted Reducer");
                // Log.Out(" reduce by {0}", reduce);
            }


            float obf = FillLevel;
            // Only reduce to max amount after pumping
            FillLevel = Math.Min(MaxFillState, FillLevel);
            FillLevel = Math.Max(MinFillState, FillLevel);
            if (obf != FillLevel) Log.Warning("Adjusted FillStae");

            if (FillLevel < 0) FillLevel = 0;

            // float sum = 0;
            // foreach (var asd in reservoirs)
            //     sum += asd.FillLevel;
            // Log.Out("= Final sum {0}", sum);

            //if (before != sum) Log.Error(
            //    "FillState sum before {0} and after {1}",
            //    before, sum);

            return true;
        }

        private List<PipeReservoir> GetReservoirs()
        {
            return reservoirs;
        }
        
        protected virtual void TickSegment(float capacity, List<PipeReservoir> reservoirs)
        {
            // No default implementation
        }

        internal float ConsumeFluid(float wanted, bool fully = false)
        {
            // Check if we have not enough to fill up
            if (fully && wanted > FillLevel) return 0;
            // Test if we want more than having
            if (wanted > FillLevel)
                wanted = FillLevel;
            // Consume what we wanted
            FillLevel -= wanted;
            return wanted;
        }
    }
}
