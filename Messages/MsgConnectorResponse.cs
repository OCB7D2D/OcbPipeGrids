namespace PipeManager
{
    public class MsgConnectorResponse : RemoteResponse<NetPkgConnectorResponse>
    {

        public BlockConnector[] NB = new BlockConnector[6];

        public override void ProcessOnClient(PipeGridClient client)
        {
            client.OnConnectResponse(this);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            foreach (var nb in NB)
                nb.read(br);
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            foreach (var nb in NB)
                nb.write(bw);
        }

        protected override void SetupNetPkg(NetPkgConnectorResponse pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }
}
