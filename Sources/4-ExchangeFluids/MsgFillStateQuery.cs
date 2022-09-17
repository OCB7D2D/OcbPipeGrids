namespace NodeFacilitator
{
    public class MsgFillStateQuery : ServerNodeQuery<NetPkgWaterLevelQuery>
    {

        public float ChangeFillState = 0f;

        public override void ProcessOnWorker(NodeManagerWorker worker)
        {
            var response = new MsgFillStateResponse();
            response.RecipientEntityId = SenderEntityId;
            response.Setup(Position); // Two steps
            if (worker.TryGetNode(Position, out IFilled node))
            {
                if (node is FarmWell well) well
                    .AddWater = ChangeFillState;
                response.WaterLevel = node.FillLevel;
            }
            worker.AnswerToClient(response);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            ChangeFillState = br.ReadSingle();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(ChangeFillState);
        }

        protected override void SetupNetPkg(NetPkgWaterLevelQuery pkg) => pkg.FromMsg(this);

    }
}
