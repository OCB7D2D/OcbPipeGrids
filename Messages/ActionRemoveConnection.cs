namespace PipeManager
{
    public class ActionRemoveConnection : RemoteQuery<NetPkgActionRemoveConnection>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            worker.Manager.UnregisterConnection(Position);
        }

        protected override void SetupNetPkg(NetPkgActionRemoveConnection pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
