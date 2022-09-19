namespace NodeManager
{
    public class ActionAddConnection : BaseActionAddBlock<NetPkgActionAddConnection>
    {
        public override PipeNode CreatePipeNode() => new PipeConnection(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddConnection pkg) => pkg.Setup(this);
    }

    public class ActionAddSource : BaseActionAddBlock<NetPkgActionAddSource>
    {
        public override PipeNode CreatePipeNode() => new PipeSource(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddSource pkg) => pkg.Setup(this);
    }

    public class ActionAddPump : BaseActionAddBlock<NetPkgActionAddPump>
    {
        public override PipeNode CreatePipeNode() => new PipePump(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPump pkg) => pkg.Setup(this);
    }

    public class ActionAddIrrigation : BaseActionAddBlock<NetPkgActionAddIrrigation>
    {
        public override PipeNode CreatePipeNode() => new PipeIrrigation(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddIrrigation pkg) => pkg.Setup(this);
    }

    public class ActionAddWell : BaseActionAddBlock<NetPkgActionAddWell>
    {
        public override PipeNode CreatePipeNode() => new PipeWell(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWell pkg) => pkg.Setup(this);
    }

    public abstract class BaseActionAddBlock<N> : RemoteQuery<N> where N : NetPackage
    {

        protected BlockValue BV;

        public abstract PipeNode CreatePipeNode();

        public override void ProcessOnServer(NodeManagerWorker worker)
            => CreatePipeNode().AttachToManager(worker.Manager);

        public void Setup(Vector3i position, BlockValue bv)
        {
            base.Setup(position);
            BV = bv;
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            BV.rawData = br.ReadUInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(BV.rawData);
        }

        public override int GetLength() => 42;

    }
}
