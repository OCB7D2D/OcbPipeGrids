namespace PipeManager
{
    public abstract class NetPkgWorkerAnswer<T> : NetPackage where T : IRemoteResponse, new()
    {

        T Msg;

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            // We should only see these on remote clients (no local manager)
            Log.Out("Process network package for worker answer from server to client");
        }

        public NetPkgWorkerAnswer<T> FromMsg(T msg)
        {
            Msg = msg;
            return this;
        }

        public override void read(PooledBinaryReader br)
        {
            Msg = new T();
            Msg.Read(br);
        }

        public override void write(PooledBinaryWriter bw)
        {
            Msg.Write(bw);
        }

        public override int GetLength()
        {
            return Msg.GetLength();
        }

    }
}