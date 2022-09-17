namespace PipeManager
{
    public interface IPoweredNode
    {
        Vector3i WorldPos { get; }
        bool IsPowered { get; set; }
    }

}