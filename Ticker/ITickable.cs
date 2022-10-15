
using NodeManager;
using System.Collections.Concurrent;
using System.Collections.Generic;

public interface ITickable : IWorldPos, IEqualityComparer<NodeBase>
{
    bool Tick(ulong delta);
    ulong NextTick { get; }
}
