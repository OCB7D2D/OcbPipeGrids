using System.Collections.Concurrent;

namespace NodeManager
{
    public class NodeManagerRunner
    {

        private readonly PipeGridWorker Worker;

        private readonly System.Threading.Thread Thread;

        public NodeManagerRunner(
            ConcurrentQueue<IActionServer> input,
            ConcurrentQueue<IActionClient> output)
        {
            Worker = new PipeGridWorker(input, output);
            Thread = new System.Threading.Thread(
                new System.Threading.ThreadStart(
                    Worker.ThreadProc));
        }

        public void Start()
        {
            Thread.Start();
        }

        public void Stop()
        {
            Worker.SendToServer(new ActionStopManager());
            Thread.Join(); // Waiting for it to quit
            // ToDo: Add timeout and force shutdown?
            // Note: already works pretty flawlessly
        }

    }
}
