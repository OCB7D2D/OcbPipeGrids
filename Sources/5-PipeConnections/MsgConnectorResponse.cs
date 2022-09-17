namespace NodeFacilitator
{
    public class NetPkgConnectorResponse : NetPkgWorkerAnswer<MsgConnectorResponse> { }

    public class MsgConnectorResponse : ServerNodeResponse<NetPkgConnectorResponse>
    {

        public BlockConnector[] NB = new BlockConnector[6];

        public override void ProcessAtClient(NodeManagerClient client)
        {
            client.OnConnectResponse(this);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            for (int i = 0; i < NB.Length; i++)
                NB[i].Read(br);
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            for (int i = 0; i < NB.Length; i++)
                NB[i].Write(bw);
        }

        protected override void SetupNetPkg(NetPkgConnectorResponse pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }
}
