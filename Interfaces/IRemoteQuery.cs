namespace NodeManager
{
    public interface IRemoteQuery : IActionWorker
    {
        void Read(PooledBinaryReader br);
        void Write(PooledBinaryWriter bw);
        NetPackage CreateNetPackage();
        int GetLength();
    }

}