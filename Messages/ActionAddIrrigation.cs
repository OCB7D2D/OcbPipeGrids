﻿namespace PipeManager
{
    public class ActionAddIrrigation : RemoteQuery<NetPkgActionAddIrrigation>
    {

        private BlockValue BV;
        private byte ConnectMask;

        public void Setup(Vector3i position, BlockValue bv, byte mask)
        {
            base.Setup(position);
            ConnectMask = mask;
            BV = bv;
        }

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            new PipeIrrigation(Position, ConnectMask, BV)
                .AttachToManager(worker.Manager);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            BV.rawData = br.ReadUInt32();
            ConnectMask = br.ReadByte();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(BV.rawData);
            bw.Write(ConnectMask);
        }

        protected override void SetupNetPkg(NetPkgActionAddIrrigation pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
