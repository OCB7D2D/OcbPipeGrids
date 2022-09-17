using System.Collections.Concurrent;
using System.Diagnostics;

namespace NodeFacilitator
{
    public class NodeManagerWorker : NodeManager
    {

        //#####################################################
        //#####################################################

        protected readonly BlockingCollection<IActionWorker> ForWorker;

        protected readonly System.Threading.Thread Thread;

        //#####################################################
        //#####################################################

        public NodeManagerWorker(
            BlockingCollection<IActionWorker> input,
            BlockingCollection<IActionMother> output,
            BlockingCollection<IMessageClient> clients)
            : base(output, clients)
        {
            ForWorker = input;
            Thread = new System.Threading.Thread(
                new System.Threading.ThreadStart(ThreadProc));
        }

        //#####################################################
        // Main functions to pass messages around
        // Can be called from worker and mother side!
        //#####################################################

        public void AnswerToClient(IMessageClient response)
        {
            ToClients.Add(response);
        }

        public void PushToServer(IActionWorker action)
        {
            ForWorker.Add(action);
        }

        //#####################################################
        // Main functions to bring up/down the thread
        // Must only be called from the mother side!
        //#####################################################

        public void Start()
        {
            Thread.Start();
        }

        public void Stop()
        {
            PushToServer(new ActionStopManager());
            Thread.Join(); // Waiting for it to quit
            // ToDo: Add timeout and force shutdown?
            // Note: already works pretty flawlessly
        }

        //#####################################################
        // Background thread will voluntarely quit if this
        // flag is set and exit the endless process loop.
        //#####################################################

        protected volatile bool Running = true;

        // Should be safe to call from either side
        public void SendStopSignal()
        {
            Running = false;
        }

        //#####################################################
        // Main processing thread running in the background
        // Keep it ticking, consume and proccess actions from
        // the main thread and send out updates when required.
        // Although we could just go beserk on a background CPU,
        // we still try to play nicely with resources avaialble.
        // Some poeple may still be one single or dual cores!
        //#####################################################

        // Keep track and report "CPU over-usage" spikes
        // Does have a little overhead attached to it!?
        readonly Stopwatch Timer = new Stopwatch();

        public void ThreadProc()
        {
            // Load data once thread starts up
            LoadData();

            // Go into worker loop
            // Until stop is signaled
            while (Running)
            {
                Timer.Reset();
                Timer.Start();
                long milsecs;
                // Consume messages for worker thread
                while (Timer.ElapsedMilliseconds < 25 &&
                    ForWorker.TryTake(out IActionWorker msg, 25))
                {
                    try
                    {
                        milsecs = Timer.ElapsedMilliseconds;
                        msg.ProcessOnWorker(this);
                        if (Timer.ElapsedMilliseconds - milsecs > 8)
                            Log.Out("Task too {0}ms (Queued: {1})",
                                Timer.ElapsedMilliseconds,
                                ForWorker.Count);
                    }
                    catch (System.Exception err)
                    {
                        Log.Error("NodeManager processing had error: {0}", err);
                    }
                }
                // Listen to abort flag
                // Doing a clean shut-down
                if (!Running) break;
                // Part to visual state listeners
                try
                {
                    milsecs = Timer.ElapsedMilliseconds;
                    DriveTick();
                    if (Timer.ElapsedMilliseconds - milsecs > 28)
                        Log.Out("Ticks took {0}ms (Schedules: {1})",
                            Timer.ElapsedMilliseconds,
                            Count);
                    milsecs = Timer.ElapsedMilliseconds;
                    TickVisualStateListeners();
                    if (Timer.ElapsedMilliseconds - milsecs > 28)
                        Log.Out("Visuals took {0}ms (Listeners: {1})",
                            Timer.ElapsedMilliseconds,
                            VisualStateListeners.Count);
                }
                catch (System.Exception err)
                {
                    Log.Error("NodeManager tick had error: {0}", err);
                }
                Timer.Stop();
            }
            // Save data on exit
            SaveData();
            Cleanup();
        }

    }
}
