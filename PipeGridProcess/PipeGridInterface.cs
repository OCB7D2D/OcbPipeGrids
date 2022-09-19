using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeManager
{
    public class PipeGridInterface : SingletonInstance<PipeGridInterface>
    {

        static public bool HasClient => !GameManager.IsDedicatedServer;
        static public bool HasServer => ConnectionManager.Instance.IsServer;

        // static public IfacePipeGridAPI Interface => Instance.API;

        public ConcurrentQueue<IActionServer> Input
            = new ConcurrentQueue<IActionServer>();
        public ConcurrentQueue<IActionClient> Output
            = new ConcurrentQueue<IActionClient>();

        public PipeGridRunner Runner { get; } = null;
        public PipeGridClient Client { get; } = null;
        
        public PipeGridInterface()
        {
            if (HasClient) Client = new PipeGridClient(Input, Output);
            if (HasServer) Runner = new PipeGridRunner(Input, Output);
        }

        internal void Init()
        {
            if (HasServer) Runner.Start();
        }


        internal void Update()
        {

            while (Output.TryDequeue(
                out IActionClient response))
            {
                if (response.RecipientEntityId == -1)
                {
                    response.ProcessOnClient(Client);

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
            if (HasServer) Runner.Stop();
            // Client.Cleanup();
            instance = null;
        }

        public static void SendToServer(IRemoteQuery query)
        {
            if (HasServer)
            {
                Instance.Input.Enqueue(query);
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
