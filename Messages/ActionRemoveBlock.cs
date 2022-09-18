namespace PipeManager
{

    public class ActionRemoveConnection : BaseActionRemoveBlock<NetPkgActionRemoveConnection>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveConnection pkg) => pkg.Setup(this);
    }

    public class ActionRemoveSource : BaseActionRemoveBlock<NetPkgActionRemoveSource>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveSource pkg) => pkg.Setup(this);
    }

    public class ActionRemovePump : BaseActionRemoveBlock<NetPkgActionRemovePump>
    {
        protected override void SetupNetPkg(NetPkgActionRemovePump pkg) => pkg.Setup(this);
    }

    public class ActionRemoveIrrigation : BaseActionRemoveBlock<NetPkgActionRemoveIrrigation>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveIrrigation pkg) => pkg.Setup(this);
    }

    public class ActionRemoveWell : BaseActionRemoveBlock<NetPkgActionRemoveWell>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveWell pkg) => pkg.Setup(this);
    }

    public abstract class BaseActionRemoveBlock<N> : RemoteQuery<N>
        where N : NetPackage
    {
        public override void ProcessOnServer(PipeGridWorker worker)
            => worker.Manager.RemovePipeGridNode(Position);
        public override int GetLength() => 42;
    }
}
