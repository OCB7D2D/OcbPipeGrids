namespace NodeFacilitator
{

    //########################################################
    // A message from the worker to clients
    // This must be able to cross networks
    //########################################################

    public interface IMessageClient
    {
        int RecipientEntityId { get; }
        void ProcessAtClient(NodeManagerClient client);
        void Read(PooledBinaryReader br);
        void Write(PooledBinaryWriter bw);
        NetPackage CreateNetPackage();
        int GetLength();
    }

    //########################################################
    //########################################################

}