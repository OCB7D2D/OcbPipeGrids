namespace PipeManager
{
    public class ActionRemoveSource : RemoteQuery<NetPkgActionRemoveSource>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            worker.Manager.UnregisterConnection(Position);
        }

        protected override void SetupNetPkg(NetPkgActionRemoveSource pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
