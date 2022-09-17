using System;
using System.Collections.Generic;

namespace NodeFacilitator
{

    //################################################################
    //################################################################

    public class ScheduledTick : IComparer<ScheduledTick>, System.IComparable<ScheduledTick>
    {

        public ITickable Object;
        public ulong TickTime;
        public ulong TickStart = 42;

        private readonly Random rng = new Random();

        public int Compare(ScheduledTick x, ScheduledTick y)
        {
            var rv = x.TickTime.CompareTo(y.TickTime);
            if (rv == 0) rv = x.TickStart.CompareTo(y.TickStart);
            if (rv == 0) rv = x.Object.WorldPos.Equals(y.Object.WorldPos) ? 0 : -1;
            return rv;
        }

        public int CompareTo(ScheduledTick b)
        {
            return Compare(this, b);
        }

        public ScheduledTick(ulong start, ulong time, ITickable tickable)
        {
            // Log.Warning("Tick start {0}", start);
            TickStart = start;
            TickTime = time;
            Object = tickable;
        }

        public ScheduledTick(ulong offset, ITickable tickable)
        {
            TickStart = GameTimer.Instance.ticks;
            // Log.Out("Scheduling Tick from {0}", TickStart);
            TickTime = TickStart + offset;
            TickTime += (ulong)rng.Next(0, 30);
            Object = tickable;
        }

    }

    //################################################################
    //################################################################

}