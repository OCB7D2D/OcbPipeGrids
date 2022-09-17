namespace NodeFacilitator
{

    public class NetPkgDescriptionQuery : NetPkgWorkerQuery<MsgDescriptionQuery> { }

    public class MsgDescriptionQuery : ServerNodeQuery<NetPkgDescriptionQuery>
    {

        public override void ProcessOnWorker(NodeManagerWorker worker)
        {
            var response = new MsgDescriptionResponse();
            response.RecipientEntityId = SenderEntityId;
            response.Setup(Position); // Two steps
            if (worker.TryGetNode(Position, out var node))
                response.Description = node.GetCustomDescription();
            else response.Description = "Error: PipeNode not found";
            worker.AnswerToClient(response);
        }
        
        protected override void SetupNetPkg(NetPkgDescriptionQuery pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }

}
