
using NodeManager;
using System.Collections.Concurrent;
using System.Collections.Generic;

public interface ITickable : IWorldPos, IEqualityComparer<NodeBase>
{
    // NodeManager.NodeManager Manager { get; }
    ScheduledTick Scheduled { get; set; }
    bool Tick(ulong delta);
    ulong NextTick { get; }
}
