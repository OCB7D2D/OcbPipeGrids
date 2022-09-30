﻿namespace NodeManager
{
    public abstract class NetPkgWorkerQuery<T> : NetPackage where T : class, IRemoteQuery, new()
    {

        T Msg;

        public override void ProcessPackage(World world, GameManager gm)
        {
            // We should only see these for remote clients
            Log.Out("Process network package to query worker for client {0}", Msg);
            Msg.SenderEntityId = this.Sender.entityId;
            NodeManagerInterface.Instance.ToWorker.Enqueue(Msg);
            Msg = default(T); // Consumed and pushed away
            // var response = new MsgConnectorResponse();
            // worker.AnswerToClient(response, this);

        }

        public NetPkgWorkerQuery<T> Setup(T msg)
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