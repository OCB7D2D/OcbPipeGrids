using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{
    public class NodeManagerInterface : SingletonInstance<NodeManagerInterface>
    {

        static public bool HasClient => !GameManager.IsDedicatedServer;
        static public bool HasServer => ConnectionManager.Instance.IsServer;

        // static public IfacePipeGridAPI Interface => Instance.API;

        public ConcurrentQueue<IActionWorker> ToWorker
            = new ConcurrentQueue<IActionWorker>();
        public ConcurrentQueue<IActionClient> ToMother
            = new ConcurrentQueue<IActionClient>();

        public NodeManagerRunner Worker { get; } = null;
        public NodeManagerMother Mother { get; } = null;
        
        public NodeManagerInterface()
        {
            if (HasClient) Mother = new NodeManagerMother(ToWorker, ToMother);
            if (HasServer) Worker = new NodeManagerRunner(ToWorker, ToMother);
        }

        internal void Init()
        {
            if (HasServer) Worker.Start();
        }


        internal void Update()
        {

            while (ToMother.TryDequeue(
                out IActionClient response))
            {
                if (response.RecipientEntityId == -1)
                {
                    response.ProcessOnMainThread(Mother);

                }
                else
                {
                    Log.Out("?????? Send response to client {0}",
                        response.RecipientEntityId);
                }
                // ConnectionManager.Instance.Clients.ForEntityId(nameInfoToPlayerId)

            }

            //Client.Tick();
        }

        internal void Cleanup()
        {
            if (HasServer) Worker.Stop();
            // Client.Cleanup();
            instance = null;
        }

        public static void SendToServer(IRemoteQuery query)
        {
            if (HasServer)
            {
                Instance.ToWorker.Enqueue(query);
            }
            else
            {
                Log.Out("Send package to server");
                NetPackage pkg = query.CreateNetPackage();
                ConnectionManager.Instance.SendToServer(pkg);
            }
        }

    }
}
