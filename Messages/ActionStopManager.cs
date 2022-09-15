namespace PipeManager
{
    public class ActionStopManager : IActionServer
    {
        public void ProcessOnServer(PipeGridWorker worker)
        {
            worker.SendStopSignal();
        }
    }
}
