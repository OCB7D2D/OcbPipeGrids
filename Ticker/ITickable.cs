
using NodeManager;
using System.Collections.Concurrent;

public interface ITickable
{
    Vector3i WorldPos { get; }
    bool Tick(ulong delta);
    ulong NextTick { get; }
}
