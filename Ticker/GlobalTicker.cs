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
        // Scheduled.RemoveWhere(scheduled =>
        //     scheduled.Object.Manager == null);
        while (Scheduled.Count != 0)
        {
            // Check if we reached our limit
            // Will process more on next loop
            if (done > todo) break;
            // Get the first item from sorted list
            var scheduled = Scheduled.First();
            // Check if we have reached time limit
            // Only process ticks that are really due
            if (scheduled.TickTime > tick) break;
            // Remove the item from the scheduler
            Scheduled.Remove(scheduled);
            // Check if Scheduled object was reset
            // Use it to abort ticking in another way
            if (scheduled.Object.Scheduled == null) return;
            // Reset object now, not yet re-scheduled
            scheduled.Object.Scheduled = null;
            // Calculate the tick delta for this item
            ulong delta = tick - scheduled.TickStart;
            // Execute the main Tick function
            // Reschedule if Tick returns true
            if (scheduled.Object.Tick(delta))
            {
                // Query base object for next ticker
                ulong iv = scheduled.Object.NextTick;
                // Re-schedule ticker if requested
                if (iv != 0) scheduled.Object.Scheduled
                    = Schedule(iv, scheduled.Object);
            }
            // Account work
            done += 1;
        }
    }

}
