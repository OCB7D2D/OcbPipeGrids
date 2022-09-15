namespace PipeManager
{
    public class MsgConnectorQuery : RemoteQuery<NetPkgConnectorQuery>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            var response = new MsgConnectorResponse();
            PipeGridManager manager = worker.Manager;
            response.Setup(Position); // Two steps
            manager.GetNeighbours(Position, ref response.NB);
            worker.AnswerToClient(response, this);
        }
        
        protected override void SetupNetPkg(NetPkgConnectorQuery pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
