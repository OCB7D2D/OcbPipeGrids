namespace PipeManager
{
    public interface IActionServer
    {
        int SenderEntityId { get; set; }
        void ProcessOnServer(PipeGridWorker worker);
    }

}