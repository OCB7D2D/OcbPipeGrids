using NodeManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

// Sorted ticks to dispatch work in chunks
// You need to drive it from time to time
// Will make sure to not over utilize CPU
public class GlobalTicker // : SingletonInstance<GlobalTicker>
{

    private int MaxScheduleCallsPerTick = 24;

    // A set of ticks pre-sorted to dispatch in batches for less CPU strain
    private readonly SortedSet<ScheduledTick>
        Scheduled = new SortedSet<ScheduledTick>();

    private readonly Random rng = new Random();

    public int Count { get => Scheduled.Count; }

    public ScheduledTick Schedule(ulong ticks, ITickable tickable)
    {
        // Randomize tick a little bit
        ticks += (ulong)rng.Next(0, 70);
        // Log.Out("+++++ Scheduled in {0}", ticks);
        ScheduledTick scheduled = new ScheduledTick(ticks, tickable);

        // Log.Out("Scheduling a new tick at {0}", scheduled.TickStart);

        // Scheduled.Remove(scheduled);
        Scheduled.Add(scheduled);
        // Log.Out("Scheduled in {2} (left growing: {0}, ticks: {1})",
        //     Instance.Growing.Count, Instance.ScheduledTicks.Count, ticks);
        return scheduled;
    }

    public bool Unschedule(ScheduledTick scheduled)
    {
        return Scheduled.Remove(scheduled);
    }

    public void DriveTick()
    {
        int done = 0;
        // ToDo: should be marked volatile!?
        // Not sure this is thread safe enough
        var tick = GameTimer.Instance.ticks;
        int todo = Utils.FastMin(Scheduled.Count,
            MaxScheduleCallsPerTick);
        while (Scheduled.Count != 0)
        {
            if (done > todo) break;
            var scheduled = Scheduled.First();
            if (scheduled.TickTime > tick) break;
            Scheduled.Remove(scheduled);
            ulong delta = tick - scheduled.TickStart;
            scheduled.Object.Tick(delta);
            ulong iv = scheduled.Object.NextTick;
            if (iv != 0) Schedule(iv, scheduled.Object);
            done += 1;
        }
    }

}
