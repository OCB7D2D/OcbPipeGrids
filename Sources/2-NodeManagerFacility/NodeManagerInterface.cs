using System.Collections.Concurrent;

namespace NodeFacilitator
{
    public partial class NodeManagerInterface : SingletonInstance<NodeManagerInterface>
    {

        //########################################################
        // Static game-mode helper functions
        //########################################################

        static public bool HasClient => !GameManager.IsDedicatedServer;
        static public bool HasServer => ConnectionManager.Instance.IsServer;
        static public bool IsRemoteClient => !ConnectionManager.Instance.IsServer;

        //########################################################
        // Main queues for interprocess communication (IPC)
        //########################################################

        public BlockingCollection<IActionWorker> ToWorker
            = new BlockingCollection<IActionWorker>();
        public BlockingCollection<IActionMother> ForMother
            = new BlockingCollection<IActionMother>();
        public BlockingCollection<IMessageClient> ForClients
            = new BlockingCollection<IMessageClient>();

        //########################################################
        // Accessors for encapsulating the main interfaces.
        // Beware that those might be null on certain modes,
        // e.g. Single-Player/Self-Hosted vs. Dedicated Server.
        //########################################################

        public readonly NodeManagerClient Client;
        private readonly NodeManagerWorker Worker;

        //########################################################
        //########################################################

        public NodeManagerInterface()
        {
            // Client is available whenever a main player is attached
            // True for SP and self-hosted MP, false for dedicated server
            if (HasClient) Client = new NodeManagerClient();
            // The server part is not available at remote clients
            // You need to communicate over network with that part
            // The Mother and Worker facilitate thread communication
            if (HasServer) Worker = new NodeManagerWorker(ToWorker, ForMother, ForClients);
        }

        public void Init()
        {
            // Startup the background thread
            if (HasServer) Worker.Start();
        }

        public void Cleanup()
        {
            // if (HasClient) Client.Cleanup();
            // Stop the background thread
            if (HasServer) Worker.Stop();
            // Cleanup Singleton
            instance = null;
        }

        //########################################################
        //########################################################

        public void Update()
        {
            // Process messages from worker for mother
            // E.g. BlockChanges are first sent to the
            // main thread to update the server state.
            // Changes will be sent to clients from there.
            while (ForMother.TryTake(
                out IActionMother response))
            {
                response.ProcessAtMother(this);
            }
            // Process messages for potential remote clients.
            // This includes all responses for e.g. description
            // or connection requests (clients will cache those).
            while (ForClients.TryTake(
                out IMessageClient response))
            {
                // Check if package is meant for local player
                if (response.RecipientEntityId == -1)
                {
                    // No need to send it over the wire
                    // Just process it here right away!
                    response.ProcessAtClient(Client);
                }
                else
                {
                    // If client is remote, we need the send it
                    NetPackage pkg = response.CreateNetPackage();
                    ClientInfo client = ConnectionManager.Instance
                        .Clients.ForEntityId(response.RecipientEntityId);
                    client.SendPackage(pkg);
                }
            }
            // Send weather and light for blocks we listen to
        }

        //########################################################
        //########################################################

        // Add a message from mother to worker
        // You should check `HasServer` before call
        public static void PushToWorker(IActionWorker action)
        {
            if (HasServer) Instance.ToWorker.Add(action);
            else Log.Error("Denied PushToWorker at remote client");
        }

        // Add a message from client to worker
        // Call this when the server might be remote
        public static void SendToWorker(IMessageWorker query)
        {
            // Process locally (SP)
            if (HasServer)
            {
                PushToWorker(query);
            }
            // Send to remote server (MP)
            else
            {
                NetPackage pkg = query.CreateNetPackage();
                ConnectionManager.Instance.SendToServer(pkg);
            }
        }

        //########################################################
        //########################################################

    }
}
