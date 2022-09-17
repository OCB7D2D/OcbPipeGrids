public interface IBlockConnection : IBlockNode
{
    bool BreakSegment { get; set; }

    bool NeedsPower { get; }

    byte PipeDiameter { get; set; }

    int MaxConnections { get; set; }

    bool CanConnect(byte side, byte rotation);

    byte ConnectMask { get; set; }

    uint SideMask { get; set; }

    byte ConnectFlags { get; }


}
