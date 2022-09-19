using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NodeManager
{
    public abstract class PipeReservoir : PipeConnection, IPoweredNode
    {
        public float MaxFillState => 150f;

        public float FillState { get; set; } = 0f;

        public override ulong NextTick => 15;

        // public override bool BreakDistance => true;

        public bool IsPowered { get; set; }

        public PipeReservoir(Vector3i position, BlockValue bv)
            : base(position, bv) { }

        public PipeReservoir(
            BinaryReader br)
        : base(br)
        {
            FillState = br.ReadSingle();
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(FillState);
        }

        public override string GetCustomDescription()
        {
            return string.Format("Reservoir {0}/{1}",
                FillState, MaxFillState);
        }

        List<PipeReservoir> intake = new List<PipeReservoir>();
        List<PipeReservoir> outtake = new List<PipeReservoir>();
        List<PipeReservoir> reservoirs = new List<PipeReservoir>();

        public override void Tick(ulong delta)
        {
            base.Tick(delta);

            if (!IsPowered) return;
            // Prepare variables
            reservoirs.Clear();
            float levels = 0;

            // Propagate through the pipe network
            PropagateThroughGrid(null, (cur, prev) =>
            {
                // Check if connection is a reservoir
                if (cur is PipeReservoir reservoir)
                {
                    if (reservoir.IsPowered)
                    {
                        // Sum up all water levels
                        levels += reservoir.FillState;
                        reservoirs.Add(reservoir);
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
                // Log.Out(" adding by {0}", needed * out_factor);
            }

            // Only reduce to max amount after pumping
            FillState = System.Math.Min(
                MaxFillState, FillState);
        }
    }
}
