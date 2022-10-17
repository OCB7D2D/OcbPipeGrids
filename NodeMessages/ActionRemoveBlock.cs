namespace NodeManager
{

    public class ActionRemoveBlock : BaseActionRemoveBlock<NetPkgActionRemoveBlock>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveBlock pkg) => pkg.Setup(this);
    }

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

    public class ActionRemoveReservoir : BaseActionRemoveBlock<NetPkgActionRemoveReservoir>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveReservoir pkg) => pkg.Setup(this);
    }

    public class ActionRemoveIrrigation : BaseActionRemoveBlock<NetPkgActionRemoveIrrigation>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveIrrigation pkg) => pkg.Setup(this);
    }

    public class ActionRemoveWell : BaseActionRemoveBlock<NetPkgActionRemoveWell>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveWell pkg) => pkg.Setup(this);
    }

    public class ActionRemoveFluidConverter : BaseActionRemoveBlock<NetPkgActionRemoveFluidConverter>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveFluidConverter pkg) => pkg.Setup(this);
    }

    public class ActionRemoveWaterBoiler : BaseActionRemoveBlock<NetPkgActionRemoveWaterBoiler>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveWaterBoiler pkg) => pkg.Setup(this);
    }

    public class ActionRemoveFluidInjector : BaseActionRemoveBlock<NetPkgActionRemoveFluidInjector>
    {
        protected override void SetupNetPkg(NetPkgActionRemoveFluidInjector pkg) => pkg.Setup(this);
    }

    public abstract class BaseActionRemoveChest<N> : BaseActionRemoveBlock<N>
        where N : NetPackage
    {
    }

    public abstract class BaseActionRemoveBlock<N> : RemoteQuery<N>
        where N : NetPackage
    {
        public override void ProcessOnWorker(PipeGridWorker worker)
        {
            Log.Out("Process disconnect on server");
            worker.Manager.RemoveManagedNode(Position);
        }
        public override int GetLength() => 42;
    }
}
