namespace PipeManager
{
    public interface IfacePipeGridAPI
    {
        bool HasPendingCanConnect(Vector3i position);
        bool CanConnect(Vector3i position, BlockConnector connector);
    }
}