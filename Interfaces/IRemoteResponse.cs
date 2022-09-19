namespace NodeManager
{
    public interface IRemoteResponse : IActionClient
    {
        void Read(PooledBinaryReader br);
        void Write(PooledBinaryWriter bw);
        NetPackage CreateNetPackage();
        int GetLength();
    }

}