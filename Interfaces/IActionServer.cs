namespace PipeManager
{
    public interface IActionClient
    {
        int RecipientEntityId { get; }
        void ProcessOnClient(PipeGridClient client);
    }

}