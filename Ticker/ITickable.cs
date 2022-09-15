
using PipeManager;
using System.Collections.Concurrent;

public interface ITickable
{
    Vector3i WorldPos { get; }
    void Tick(ulong delta, ConcurrentQueue<IActionClient> output);
    ulong NextTick { get; }
}
