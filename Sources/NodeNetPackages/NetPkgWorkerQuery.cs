namespace NodeFacilitator
{
    public abstract class NetPkgWorkerQuery<T> : NetPackage where T : class, IMessageWorker, new()
    {

        T Msg;

        public override void ProcessPackage(World world, GameManager gm)
        {
            // We should only see these for remote clients
            Log.Out("Process network package to query worker for client {0}", Msg);
            Msg.SenderEntityId = this.Sender.entityId;
            NodeManagerInterface.PushToWorker(Msg);
            Msg = default(T); // Consumed once; release memory
            // var response = new MsgConnectorResponse();
            // worker.AnswerToClient(response, this);

        }

        public NetPkgWorkerQuery<T> FromMsg(T msg)
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