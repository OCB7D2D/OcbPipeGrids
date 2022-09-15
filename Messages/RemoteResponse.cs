namespace PipeManager
{

    // A message from the worker thread
    public abstract class RemoteResponse<T> : RemotePackage<T>,
        IRemoteResponse, IActionClient where T : NetPackage
    {

        public int EntityId { get; private set; } = -1;

        // Main function called when message is received on client
        public abstract void ProcessOnClient(PipeGridClient client);

    }

}