namespace PipeManager
{

    // A message to the worker thread
    public abstract class RemoteQuery<T> : RemotePackage<T>,
        IRemoteQuery, IActionServer where T : NetPackage
    {

        // Main function called when message is received on server
        public abstract void ProcessOnServer(PipeGridWorker worker);

    }

}