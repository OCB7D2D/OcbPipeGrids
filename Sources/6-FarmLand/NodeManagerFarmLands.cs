using KdTree3;

namespace NodeFacilitator
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Keep all items in a queryable container for nearby nodes to find us
        // ToDo: we basically only need a dictionary here, as we have no reach
        public readonly KdTree<MetricChebyshev>.Vector3i<IFarmLand> FarmLands
            = new KdTree<MetricChebyshev>.Vector3i<IFarmLand>();

        // Invoked when new manager is attached
        public void AddFarmLand(IFarmLand land)
        {
            FarmLands.Add(land.WorldPos, land);
            ReachHelper.SearchLinks(land, Wells,
                WellToSoilReach, Vector3i.zero);
            ReachHelper.SearchLinks(land, Composters,
                ComposterToSoilReach, Vector3i.zero);
        }

        // Invoked when manager is set to `null`
        public void RemoveFarmLand(IFarmLand land)
        {
            // Make sure to unregister us from links
            foreach (var node in land.Wells)
                node.FarmLands.Remove(land);
            foreach (var node in land.Composters)
                node.FarmLands.Remove(land);
            // Clear our links
            land.Wells.Clear();
            land.Composters.Clear();
            // Remove from KD tree
            FarmLands.RemoveAt(land.WorldPos);
        }

    }
}
