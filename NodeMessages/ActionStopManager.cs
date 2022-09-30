namespace NodeManager
{
    public class ActionStopManager : IActionWorker
    {
        public int SenderEntityId { get => -1; set => throw new System.Exception("Can send stop signal from clients!"); }
        public void ProcessOnWorker(PipeGridWorker worker)
        {
            worker.SendStopSignal();
        }
    }
}
