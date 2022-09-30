namespace NodeManager
{
    public class MsgDescriptionResponse : RemoteResponse<NetPkgDescriptionResponse>
    {

        public string Description { get; set; }

        public override void ProcessOnMainThread(NodeManagerMother client)
        {
            client.OnDescriptionResponse(this);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Description = br.ReadString();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Description);
        }

        protected override void SetupNetPkg(NetPkgDescriptionResponse pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }
}
