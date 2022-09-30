﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NodeManager
{
    public class PipeReservoir : PipeConnection, IPoweredNode
    {
        public float MaxFillState => 150f;

        public float FillState { get; set; } = 0f;

        public ushort FluidType { get; protected set; } = 0;

        public override ulong NextTick => 60;

        // public override bool BreakDistance => true;

        public bool IsPowered { get; set; }

        public PipeReservoir(Vector3i position, BlockValue bv)
            : base(position, bv) { }

        public PipeReservoir(
            BinaryReader br)
        : base(br)
        {
            FillState = br.ReadSingle();
            FluidType = br.ReadUInt16();
        }

        public virtual void SetFluidType(ushort type)
        {
            if (FluidType != type)
            {
                if (FillState > 0)
                {
                    Log.Warning("Reseting Fluid");
                    FillState = 0f;
                }
                FluidType = type;
            }
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(FillState);
            bw.Write(FluidType);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Reservoir {0}/{1} ({2})",
                FillState, MaxFillState, FluidType);
        }

        List<PipeReservoir> intake = new List<PipeReservoir>();
        List<PipeReservoir> outtake = new List<PipeReservoir>();
        List<PipeReservoir> reservoirs = new List<PipeReservoir>();

        public override bool Tick(ulong delta)
        {
            base.Tick(delta);

            if (!IsPowered) return true;
            // First need to be filled a little
            // Happens at source or when filled
            if (FillState == 0) return true;
            // Prepare variables
            reservoirs.Clear();
            float levels = 0;


            // Propagate through the pipe network
            PropagateThroughGrid(null, (cur, prev) =>
            {
                // Check if connection is a reservoir
                if (cur is PipeReservoir reservoir)
                {
                    if (FluidType == reservoir.FluidType ||
                        reservoir.FillState == 0)
                    {
                        if (reservoir.IsPowered)
                        {
                            // Sum up all water levels
                            levels += reservoir.FillState;
                            reservoirs.Add(reservoir);
                        }
                    }
                    // Abort search at leafs
                    return cur != this;
                }
                // Continue search
                return false;
            });

            // Calculate what the wanted level would be
            float wanted = levels / reservoirs.Count;
            wanted = System.Math.Min(MaxFillState, wanted);

            Log.Out("Found {0} Reservoirs with avg level {1}",
                reservoirs.Count, wanted);

            intake.Clear();
            outtake.Clear();
            float intakes = 0f;
            float outtakes = 0f;

            foreach (var reservoir in reservoirs)
            {
                // Either take or push fluid to node
                if (reservoir.FillState > wanted)
                {
                    intake.Add(reservoir);
                    intakes += reservoir.FillState - wanted;
                }
                else if (reservoir.FillState < wanted)
                {
                    outtake.Add(reservoir);
                    outtakes += wanted - reservoir.FillState;
                }
            }

            Log.Out("Have {0}/{1} intake and {2}/{3} outtake",
                intakes, intake.Count, outtakes, outtake.Count);

            // Calculate how much we are able to pump in and out
            float pump = System.Math.Min(0.08f * delta,
                System.Math.Min(intakes, outtakes));

            // Log.Out("Pumping at {0}", pump);

            // Check if we have enough to pump
            float in_factor = pump / intakes;
            foreach (var from in intake)
            {
                float available = from.FillState - wanted;
                from.FillState -= available * in_factor;
                from.FillState = System.Math.Min(
                    from.MaxFillState, from.FillState);
                // Log.Out(" reduce by {0}", available * in_factor);
            }


            float out_factor = pump / outtakes;
            foreach (var into in outtake)
            {
                float needed = wanted - into.FillState;
                into.FillState += needed * out_factor;
                into.FillState = System.Math.Min(
                    into.MaxFillState, into.FillState);
                if (into.FluidType != FluidType)
                    into.SetFluidType(FluidType);
                // Log.Out(" adding by {0}", needed * out_factor);
            }

            // Only reduce to max amount after pumping
            FillState = System.Math.Min(
                MaxFillState, FillState);

            return true;
        }
    }
}
