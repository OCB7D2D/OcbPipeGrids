namespace PipeManager
{
    public interface IActionClient
    {
        int EntityId { get; }
        void ProcessOnClient(PipeGridClient client);
    }

}