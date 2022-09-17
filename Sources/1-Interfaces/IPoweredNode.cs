namespace NodeFacilitator
{
    public interface IPoweredNode : IWorldPos
    {
        bool IsPowered { get; set; }
    }

}