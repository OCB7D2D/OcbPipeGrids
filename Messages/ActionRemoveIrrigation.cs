namespace PipeManager
{
    public class ActionRemoveIrrigation : RemoteQuery<NetPkgActionRemoveIrrigation>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            worker.Manager.UnregisterConnection(Position);
        }

        protected override void SetupNetPkg(NetPkgActionRemoveIrrigation pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
