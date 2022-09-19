namespace NodeManager
{
    public class MsgConnectorQuery : RemoteQuery<NetPkgConnectorQuery>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            var response = new MsgConnectorResponse();
            NodeManager manager = worker.Manager;
            response.RecipientEntityId = SenderEntityId;
            response.Setup(Position); // Two steps
            manager.GetNeighbours(Position, ref response.NB);
            worker.AnswerToClient(response);
        }
        
        protected override void SetupNetPkg(NetPkgConnectorQuery pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
