namespace NodeManager
{
    public abstract class NetPkgWorkerAnswer<T> : NetPackage where T : IRemoteResponse, new()
    {

        T Msg;

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            // We should only see these on remote clients (no local manager)
            Log.Out("Process network package for worker answer from server to client");
            // Msg.RecipientEntityId = this.Sender.entityId;
            NodeManagerInterface.Instance.ToMother.Enqueue(Msg);
            Msg = default(T); // Consumed and pushed away
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
            base.write(bw);
            Msg.Write(bw);
        }

        public override int GetLength()
        {
            return Msg.GetLength();
        }

    }
}