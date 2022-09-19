namespace NodeManager
{
    public class MsgDescriptionQuery : RemoteQuery<NetPkgDescriptionQuery>
    {

        public override void ProcessOnServer(NodeManagerWorker worker)
        {
            var response = new MsgDescriptionResponse();
            NodeManager manager = worker.Manager;
            response.RecipientEntityId = SenderEntityId;
            response.Setup(Position); // Two steps
            response.Description = worker.GetCustomDesc(Position);
            worker.AnswerToClient(response);
        }
        
        protected override void SetupNetPkg(NetPkgDescriptionQuery pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
