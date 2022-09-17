namespace NodeFacilitator
{
    public abstract class NetPkgWorkerAnswer<T> : NetPackage where T : IMessageClient, new()
    {

        T Msg;

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            // We should only see these on remote clients (no local manager)
            Log.Out("999 Process network package for worker answer from server to client");
            // IMHO I should directly consume the package at client?
            Msg.ProcessAtClient(NodeManagerInterface.Instance.Client);
            // Msg.RecipientEntityId = this.Sender.entityId;
            // NodeManagerInterface.Instance.ForClients.Add(Msg);
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