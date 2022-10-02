public interface IBlockReservoir : IBlockConnection
{
    float MaxFillState { get; set; }
    ushort FluidType { get; set; }
}
