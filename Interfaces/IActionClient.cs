namespace NodeManager
{
    public interface IActionWorker
    {
        int SenderEntityId { get; set; }
        void ProcessOnWorker(PipeGridWorker worker);
    }

}