namespace NodeManager
{
    public interface IActionClient
    {
        int RecipientEntityId { get; }
        void ProcessOnMainThread(NodeManagerMother client);
    }

}