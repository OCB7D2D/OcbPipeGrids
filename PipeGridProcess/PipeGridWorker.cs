using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PipeManager
{
    public class PipeGridWorker : IfaceGridNodeManaged
    {

        private readonly ConcurrentQueue<IActionServer> Input;
        private readonly ConcurrentQueue<IActionClient> Output;

        public PipeGridManager Manager { get; private set; }

        public PipeGridWorker(
            ConcurrentQueue<IActionServer> queue,
            ConcurrentQueue<IActionClient> output)
        {
            Input = queue; Output = output;
            Manager = new PipeGridManager();
        }

        internal string GetCustomDesc(Vector3i position)
        {
            if (Manager.TryGetNode(position, out var node))
                return node.GetCustomDescription();
            return "This is my response";
        }

        public void AnswerToClient(IRemoteResponse response)
        {
            Log.Out("Answer to client {0}", response.RecipientEntityId);
            if (response.RecipientEntityId == -1)
            {
                Output.Enqueue(response);
            }
            else
            {
                Log.Out("Send package to client");
                NetPackage pkg = response.CreateNetPackage();
                ClientInfo client = ConnectionManager.Instance
                    .Clients.ForEntityId(response.RecipientEntityId);
                client.SendPackage(pkg);
            }

        }

        public void SendToServer(IActionServer action)
        {
            Input.Enqueue(action);
        }

        private bool Running = true;

        internal void SendStopSignal()
        {
            Running = false;
        }

        // The thread procedure performs the task, such as formatting
        // and printing a document.
        public void ThreadProc()
        {
            Manager.LoadData();
            // Go into worker loop
            // Until stop signaled
            while (Running)
            {
                // ToDo: Implement proper clock
                while (Input.TryDequeue(
                    out IActionServer msg))
                {
                    msg.ProcessOnServer(this);
                }
                if (!Running) break;
                Manager.DriveTick(Output);
                Thread.Sleep(25);
            }
            Manager.SaveData();
            // Will save data
            Manager.Cleanup();
        }

    }
}