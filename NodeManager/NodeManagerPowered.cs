using System.Collections.Generic;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, IPoweredNode> IsPowered
            = new Dictionary<Vector3i, IPoweredNode>();

        private bool RemovePowered(Vector3i position)
        {
            return IsPowered.Remove(position);
        }

        private void AddPowered(IPoweredNode powered)
        {
            IsPowered.Add(powered.WorldPos, powered);
        }

        public void UpdatePower(Vector3i position, bool powered)
        {
            if (IsPowered.TryGetValue(position,
                out IPoweredNode node))
            {
                node.IsPowered = powered;
            }
        }

    }
}
