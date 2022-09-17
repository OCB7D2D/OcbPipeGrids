using System.Collections.Generic;

// Sometimes we want to make some states within the NodeManager thread
// visible on the client, and there are multiple way to achieve this.
// E.g. each client could request the visual state for each visible
// block from time to time, which would be rather sub-optimal.
// I use a more sophisticated register/unregister schema, where each
// client registers the visible blocks and they should recieve updates
// regularly and automatically in a batched fashion.

namespace NodeFacilitator
{

    public class VisualStateListener : IStateNode
    {
        public IStateNode Node;

        public IVisualState LastVisualState;

        public VisualStateListener(IStateNode node)
        {
            Node = node;
        }

        public Vector3i WorldPos => Node.WorldPos;
        public IVisualState GetState() => Node.GetState();
        public bool Equals(IWorldPos x, IWorldPos y) => Node.Equals(x, y);
        public int GetHashCode(IWorldPos obj) => Node.GetHashCode(obj);
    }

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Local listeners can be updated pretty fast
        // There is no network traffic involved with it
        private ulong UpdateIntervalLocal = 20; // 1s

        // Remote clients are more expensive to update
        // Therefore use a much bigger interval for them
        private ulong UpdateIntervalRemote = 90 * 20; // 90s

        // Internal state holding last update tick stamps
        private ulong VisualStateLastTickLocal = 0;
        private ulong VisualStateLastTickRemote = 0;

        // Dictionary holding a list of registered items per client
        public readonly Dictionary<int, HashSet<VisualStateListener>>
            VisualStateListeners = new Dictionary<int, HashSet<VisualStateListener>>();

        // Add block to visual state listeners
        // invoked by `ActionAddVisualState` message
        public void AddVisualStateListener(int sender, Vector3i position)
        {
            Log.Out("Add Visual State Listener");
            // Try to get node at the given position
            if (!TryGetNode<IStateNode>(position,
                    out IStateNode node))
            {
                Log.Error("No node found to add Visual" +
                    " State listener {0}", position);
                return;
            }
            // Make sure container for client is available
            if (!VisualStateListeners.TryGetValue(sender,
                out HashSet<VisualStateListener> listeners))
            {
                listeners = VisualStateListeners[sender]
                    = new HashSet<VisualStateListener>();
            }
            // Add block to listeners
            listeners.Add(new VisualStateListener(node));
        }

        public void RemoveVisualStateListener(int sender, Vector3i position)
        {
            // Do nothing if container does not exist
            if (!VisualStateListeners.TryGetValue(sender,
                out HashSet<VisualStateListener> listeners)) return;
            // Remove all helpers where the position matches
            listeners.RemoveWhere(node => node.WorldPos == position);
            // Delete the container once it is no longer needed
            if (listeners.Count == 0) VisualStateListeners.Remove(sender);
        }

        // Invoked by main worker thread ticker
        public void TickVisualStateListeners()
        {
            // We tick all visual state listeners for a specific client at once.
            // This allows us to send all changes for one client in a burst at the
            // same time to avoid some network overhead. To reduce CPU strain this
            // will by done in certain time intervals only. Remote clients will
            // get a much slower update tick than local client in order to not
            // waste too much network traffic. We also ensure that we only send
            // actually changed states over the wire and the thread boundary.
            var tick = GameTimer.Instance.ticks;
            // Local player can be synced faster since
            // it doesn't involve any network overhead
            // Just passes it through thread barrier
            var dl = tick - VisualStateLastTickLocal;
            if (dl > UpdateIntervalLocal)
            {
                if (!VisualStateListeners.TryGetValue(-1,
                    out HashSet<VisualStateListener> listeners)) return;
                var response = new MsgUpdateVisualStates();
                response.RecipientEntityId = -1;
                response.Setup(listeners);
                if (response.HasPayLoad())
                    SendToClient(response);
                VisualStateLastTickLocal = tick;
            }
            // Remote clients get updates more slowly
            // Maybe we should spread update to clients
            // To avoid spikes in outgoing data?
            var dr = tick - VisualStateLastTickRemote;
            if (dr > UpdateIntervalRemote)
            {
                foreach (var client in VisualStateListeners)
                {
                    if (client.Key == -1) continue; // ignore locals
                    var response = new MsgUpdateVisualStates();
                    response.RecipientEntityId = client.Key;
                    response.Setup(client.Value);
                    if (response.HasPayLoad())
                        SendToClient(response);
                }
                VisualStateLastTickRemote = tick;
            }
        }

    }
}
