namespace NodeManager
{
    public class ActionStopManager : IActionServer
    {
        public int SenderEntityId { get => -1; set => throw new System.Exception("Can send stop signal from clients!"); }
        public void ProcessOnServer(NodeManagerWorker worker)
        {
            worker.SendStopSignal();
        }
    }
}
