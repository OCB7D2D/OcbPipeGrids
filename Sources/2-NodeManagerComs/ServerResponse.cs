namespace NodeFacilitator
{

    //########################################################
    //########################################################

    // Message from worker to clients without position, e.g. for batch updates
    public abstract class ServerResponse<T> : RemoteBasePackage<T>, IMessageClient where T : NetPackage
    {

        // Recipient is only set for messages to remote clients
        public int RecipientEntityId { get; set; } = -1;

        // Main function called when message is received on client
        public abstract void ProcessAtClient(NodeManagerClient mother);

    }

    //########################################################
    //########################################################

    // Message from worker to clients with one specific block/node position
    public abstract class ServerNodeResponse<T> : RemoteNodePackage<T>, IMessageClient where T : NetPackage
    {

        // Recipient is only set for messages to remote clients
        public int RecipientEntityId { get; set; } = -1;

        // Main function called when message is received on client
        public abstract void ProcessAtClient(NodeManagerClient mother);

        public void Setup(Vector3i position, int recipient)
        {
            RecipientEntityId = recipient;
            base.Setup(position);
        }

    }

    //########################################################
    //########################################################

}