namespace NodeManager
{
    public interface IActionClient
    {
        int RecipientEntityId { get; }
        void ProcessOnClient(NodeManagerClient client);
    }

}