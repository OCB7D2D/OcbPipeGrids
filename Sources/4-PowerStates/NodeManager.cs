using System.Collections.Generic;

namespace NodeFacilitator
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, IPoweredNode> PowerState
            = new Dictionary<Vector3i, IPoweredNode>();

        private bool RemovePowered(Vector3i position)
        {
            return PowerState.Remove(position);
        }

        private void AddPowered(IPoweredNode powered)
        {
            PowerState.Add(powered.WorldPos, powered);
        }

        public void UpdatePower(Vector3i position, bool powered)
        {
            if (PowerState.TryGetValue(position,
                out IPoweredNode node))
            {
                node.IsPowered = powered;
            }
        }

    }
}
