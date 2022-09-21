public interface IBlockConnection : IBlockNode
{
    bool BreakDistance { get; }

    bool MultiBlockPipe { get; set; }

    bool NeedsPower { get; }

    int MaxConnections { get; set; }

    bool CanConnect(byte side, byte rotation);

    byte ConnectMask { get; set; }

    byte ConnectFlags { get; }

}
