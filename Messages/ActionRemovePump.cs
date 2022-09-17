namespace PipeManager
{
    public class ActionRemovePump : RemoteQuery<NetPkgActionRemovePump>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            worker.Manager.UnregisterConnection(Position);
        }

        protected override void SetupNetPkg(NetPkgActionRemovePump pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
