namespace NodeManager
{
    public class MsgConnectorQuery : RemoteQuery<NetPkgConnectorQuery>
    {

        // Just pass the block type instead?
        private byte PipeDiameter = 0;

        public void SetPipeDiameter(byte diameter)
            => PipeDiameter = diameter;

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            PipeDiameter = br.ReadByte();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(PipeDiameter);
        }


        public override void ProcessOnWorker(PipeGridWorker worker)
        {
            var response = new MsgConnectorResponse();
            NodeManager manager = worker.Manager;
            response.RecipientEntityId = SenderEntityId;
            response.Setup(Position); // Two steps
            manager.GetNeighbours(Position,
                ref response.NB, PipeDiameter);
            worker.AnswerToClient(response);
        }
        
        protected override void SetupNetPkg(NetPkgConnectorQuery pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
