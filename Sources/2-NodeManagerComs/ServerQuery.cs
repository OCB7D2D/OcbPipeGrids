namespace NodeFacilitator
{

    //########################################################
    // A message to the worker thread from remote clients
    // E.g. to register visual state listeners for shown blocks
    // As such the message might be from remote or local client
    // Additionally the message must be passable to worker thread
    //########################################################

    public abstract class ServerNodeQuery<T> : RemoteNodePackage<T>, IMessageWorker where T : NetPackage
    {

        // This method is executed on the worker thread
        public abstract void ProcessOnWorker(NodeManagerWorker worker);

        // Sender ID in order to respond to that client
        // Can also be used to do permission checks
        public int SenderEntityId { get; set; } = -1;

        // Must be implemented for NetPackage
        // Although isn't really used anywhere
        public override int GetLength() => 42;

    }

    //########################################################
    //########################################################

}