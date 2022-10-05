namespace NodeManager
{
    public class MsgWaterExchangeQuery : RemoteQuery<NetPkgWaterExchangeQuery>
    {

        public int Factor { get; set; }

        public int InventorySlot { get; set; }
        public int HoldingCount { get; set; }
        public int OldItemType { get; set; }
        public int NewItemType { get; set; }

        public override void ProcessOnWorker(PipeGridWorker worker)
        {
            var response = new MsgWaterExchangeResponse();
            NodeManager manager = worker.Manager;
            response.RecipientEntityId = SenderEntityId;
            Log.Out("++++ Ask for water {0} x {1} at {2}", HoldingCount, Factor, Position);
            response.Setup(Position); // Two steps
            if (manager.TryGetNode(Position, out PipeWell well))
            {
                response.Exchanged = well.ExchangeWater(HoldingCount, Factor);
                response.WaterLevel = well.FillState;
            }
            response.InventorySlot = InventorySlot;
            response.HoldingCount = HoldingCount;
            response.OldItemType = OldItemType;
            response.NewItemType = NewItemType;
            Log.Out("Gotcha water {0}", response.Exchanged);
            worker.AnswerToClient(response);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Factor = br.ReadInt32();
            InventorySlot = br.ReadInt32();
            HoldingCount = br.ReadInt32();
            OldItemType = br.ReadInt32();
            NewItemType = br.ReadInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Factor);
            bw.Write(InventorySlot);
            bw.Write(HoldingCount);
            bw.Write(OldItemType);
            bw.Write(NewItemType);
        }

        protected override void SetupNetPkg(NetPkgWaterExchangeQuery pkg) => pkg.Setup(this);

    }
}
