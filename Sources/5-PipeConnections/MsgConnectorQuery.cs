namespace NodeFacilitator
{

    // Message to cross from worker thread to main game thread
    public class MsgConnectorQuery : ServerNodeQuery<NetPkgConnectorQuery>
    {

        public override void ProcessOnWorker(NodeManagerWorker worker)
        {
            var response = new MsgConnectorResponse();
            response.Setup(Position, SenderEntityId);
            worker.GetAllNeighbours(Position, ref response.NB);
            worker.AnswerToClient(response);
        }
        
        protected override void SetupNetPkg(NetPkgConnectorQuery pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;

    }

    // Wrapper for network package when transfering over the wire
    public class NetPkgConnectorQuery : NetPkgWorkerQuery<MsgConnectorQuery> { }

}
