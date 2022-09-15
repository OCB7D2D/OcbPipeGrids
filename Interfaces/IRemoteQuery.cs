namespace PipeManager
{
    public interface IRemoteQuery : IActionServer
    {
        void Read(PooledBinaryReader br);
        void Write(PooledBinaryWriter bw);
        NetPackage CreateNetPackage();
        int GetLength();
    }

}