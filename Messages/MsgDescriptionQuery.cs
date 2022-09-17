namespace PipeManager
{
    public class MsgDescriptionQuery : RemoteQuery<NetPkgDescriptionQuery>
    {

        public override void ProcessOnServer(PipeGridWorker worker)
        {
            var response = new MsgDescriptionResponse();
            Log.Out("Sending back {0}", Position);
            response.Setup(Position); // Two steps
            response.Description = worker.GetCustomDesc(Position);
            //worker.SendToClient(response, this);
            worker.AnswerToClient(response);
        }
        
        protected override void SetupNetPkg(NetPkgDescriptionQuery pkg) => pkg.Setup(this);

        public override int GetLength() => 42;
    }
}
