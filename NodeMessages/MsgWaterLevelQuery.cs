namespace NodeManager
{
    public class MsgWaterLevelQuery : RemoteQuery<NetPkgWaterLevelQuery>
    {

        public float AddWater = 0f;

        public override void ProcessOnWorker(PipeGridWorker worker)
        {
            var response = new MsgWaterLevelResponse();
            NodeManager manager = worker.Manager;
            response.RecipientEntityId = SenderEntityId;
            response.Setup(Position); // Two steps
            if (manager.TryGetNode(Position, out PipeWell well))
            {
                well.AddWater = AddWater;
                response.WaterLevel = well.WaterAvailable;
            }
            worker.AnswerToClient(response);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            AddWater = br.ReadSingle();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(AddWater);
        }

        protected override void SetupNetPkg(NetPkgWaterLevelQuery pkg) => pkg.Setup(this);

    }
}
