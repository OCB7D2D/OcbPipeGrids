namespace PipeManager
{
    public class ActionAddConnection : RemoteQuery<NetPkgActionAddConnection>
    {

        private byte Rotation;
        private byte ConnectMask;

        public void Setup(Vector3i position, byte rotation, byte mask)
        {
            base.Setup(position);
            Rotation = rotation;
            ConnectMask = mask;
        }

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            new PipeConnection(Position, ConnectMask, Rotation)
                .AttachToManager(worker.Manager);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Rotation = br.ReadByte();
            ConnectMask = br.ReadByte();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Rotation);
            bw.Write(ConnectMask);
        }

        protected override void SetupNetPkg(NetPkgActionAddConnection pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
