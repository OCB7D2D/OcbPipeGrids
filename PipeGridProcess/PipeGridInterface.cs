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
        
        // public IfacePipeGridAPI API { get; }

        public PipeGridInterface()
        {
            if (HasClient) Client = new PipeGridClient();
            if (HasServer) Runner = new PipeGridRunner(Input, Output);
            // API = Client;
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
                // Now for who is that response
                // For our local player
                // Or for some remote client
                Log.Out("Got a repsonse on main thread");


                if (response.EntityId == -1)
                {
                    Log.Out("Gotcha for local");

                    response.ProcessOnClient(Client);

                }
                else
                {
                    Log.Out("Send response to client {0}",
                        response.EntityId);
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
                NetPackage pkg = query.CreateNetPackage();
                ConnectionManager.Instance.SendToServer(pkg);
            }
        }
    }
}
