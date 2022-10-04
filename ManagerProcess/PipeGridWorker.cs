﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace NodeManager
{
    public class PipeGridWorker : IfaceGridNodeManaged
    {

        private readonly ConcurrentQueue<IActionWorker> Input;
        private readonly ConcurrentQueue<IActionClient> Output;

        public NodeManager Manager { get; private set; }

        public PipeGridWorker(
            ConcurrentQueue<IActionWorker> queue,
            ConcurrentQueue<IActionClient> output)
        {
            Input = queue; Output = output;
            Manager = new NodeManager(output);
        }

        internal string GetCustomDesc(Vector3i position)
        {
            if (Manager.TryGetNode(position, out var node))
                return node.GetCustomDescription();
            return "Error: PipeNode not found";
        }

        public void AnswerToClient(IRemoteResponse response)
        {
            if (response.RecipientEntityId == -1)
            {
                Output.Enqueue(response);
            }
            else
            {
                NetPackage pkg = response.CreateNetPackage();
                ClientInfo client = ConnectionManager.Instance
                    .Clients.ForEntityId(response.RecipientEntityId);
                client.SendPackage(pkg);
            }

        }

        public void SendToServer(IActionWorker action)
        {
            Input.Enqueue(action);
        }

        private bool Running = true;

        internal void SendStopSignal()
        {
            Running = false;
        }

        Stopwatch Timer = new Stopwatch();

        // The thread procedure performs the task, such as formatting
        // and printing a document.
        public void ThreadProc()
        {
            Manager.LoadData();

            // Go into worker loop
            // Until stop signaled
            while (Running)
            {
                Timer.Reset();
                Timer.Start();
                // ToDo: Implement proper clock
                while (Input.TryDequeue(
                    out IActionWorker msg))
                {
                    try
                    {
                        msg.ProcessOnWorker(this);
                    }
                    catch (System.Exception err)
                    {
                        Log.Error("NodeManager processing had error: {0}", err);
                    }
                }
                if (!Running) break;
                try
                {
                    Manager.DriveTick();
                }
                catch (System.Exception err)
                {
                    Log.Error("NodeManager tick had error: {0}", err);
                }
                Timer.Stop();
                if (Timer.ElapsedMilliseconds > 2)
                    Log.Out("MS elapsed: {0}", Timer.ElapsedMilliseconds);
                Thread.Sleep(25);
            }
            Manager.SaveData();
            // Will save data
            Manager.Cleanup();
        }

    }
}