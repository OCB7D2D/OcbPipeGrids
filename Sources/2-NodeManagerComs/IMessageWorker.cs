namespace NodeFacilitator
{

    //########################################################
    // Wrapping worker query for remote clients
    //########################################################

    public interface IMessageWorker : IActionWorker
    {
        void Read(PooledBinaryReader br);
        void Write(PooledBinaryWriter bw);
        NetPackage CreateNetPackage();
        int GetLength();
    }

    //########################################################
    //########################################################

}