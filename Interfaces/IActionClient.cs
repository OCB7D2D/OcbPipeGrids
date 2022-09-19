namespace NodeManager
{
    public interface IActionServer
    {
        int SenderEntityId { get; set; }
        void ProcessOnServer(NodeManagerWorker worker);
    }

}