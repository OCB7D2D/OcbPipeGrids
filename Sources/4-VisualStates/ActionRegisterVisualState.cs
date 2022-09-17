namespace NodeFacilitator
{
    public class ActionAddVisualState : RemoteBlockValue<NetPkgActionAddVisualState>, IMessageWorker
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
            => worker.AddVisualStateListener(SenderEntityId, Position);
        protected override void SetupNetPkg(NetPkgActionAddVisualState pkg) => pkg.FromMsg(this);
    }

    public class ActionRemoveVisualState : RemoteBlockValue<NetPkgActionRemoveVisualState>, IMessageWorker
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
            => worker.RemoveVisualStateListener(SenderEntityId, Position);
        protected override void SetupNetPkg(NetPkgActionRemoveVisualState pkg) => pkg.FromMsg(this);
    }

}
