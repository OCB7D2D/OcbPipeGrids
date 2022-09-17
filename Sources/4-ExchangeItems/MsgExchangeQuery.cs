namespace NodeFacilitator
{

    public class NetPkgRemoteExchangeQuery : NetPkgWorkerQuery<MsgExchangeQuery> { }

    public class MsgExchangeQuery : ServerNodeQuery<NetPkgRemoteExchangeQuery>
    {

        public enum ExchangeItems
        {
            Query,
            Exchange,
        }

        private ExchangeItems Interaction;
        private int ItemType;
        private int ItemCount;

        public void Setup(Vector3i position,
            ExchangeItems interaction,
            int type, int count)
        {
            base.Setup(position);
            Interaction = interaction;
            ItemType = type;
            ItemCount = count;
        }

        public override void ProcessOnWorker(NodeManagerWorker worker)
        {
            // Client offers to exchange items and wants to know
            // how many are actually needed. Return number back
            // and lock the state for a few seconds until done!?
            if (Interaction == ExchangeItems.Query)
            {
                if (worker.TryGetNode(Position,
                    out IExchangeItems exchanger))
                {
                    int count = exchanger.AskExchangeCount(ItemType, ItemCount);
                    var response = new MsgExchangeResponse();
                    response.RecipientEntityId = SenderEntityId;
                    response.Setup(ItemType, count);
                    response.Setup(Position);
                    worker.AnswerToClient(response);
                }
                else
                {
                    Log.Warning("Wrong node found at {0} => {1}", Position,
                        worker.TryGetNode<NodeBase>(Position));
                }
            }
            // Execute the exchange once caller has learned
            // how many items can actually be consumed
            else if (Interaction == ExchangeItems.Exchange)
            {
                if (worker.TryGetNode(Position,
                    out IExchangeItems exchanger))
                {
                    exchanger.ExecuteExchange(ItemType, ItemCount);
                }
                else
                {
                    Log.Warning("Wrong node found at {0} => {1}", Position,
                        worker.TryGetNode<NodeBase>(Position));
                }
            }
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Interaction = (ExchangeItems)br.ReadByte();
            ItemType = br.ReadInt32();
            ItemCount = br.ReadInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write((byte)Interaction);
            bw.Write(ItemType);
            bw.Write(ItemCount);
        }

        protected override void SetupNetPkg(NetPkgRemoteExchangeQuery pkg) => pkg.FromMsg(this);

    }

}
