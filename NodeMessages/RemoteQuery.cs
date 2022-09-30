namespace NodeManager
{

    // A message to the worker thread
    public abstract class RemoteQuery<T> : RemotePackage<T>,
        IRemoteQuery, IActionWorker where T : NetPackage
    {
        public int SenderEntityId { get; set; } = -1;
        // Main function called when message is received on server
        public abstract void ProcessOnWorker(PipeGridWorker worker);
        public override int GetLength() => 42;

    }

}