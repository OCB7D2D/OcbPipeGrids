using KdTree3;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IFarmLand> FarmLands
            = new KdTree<MetricChebyshev>.Vector3i<IFarmLand>();

        // Invoked when new manager is attached
        public void AddFarmLand(IFarmLand land)
        {
            FarmLands.Add(land.WorldPos, land);
            ReachHelper.SearchLinks(land, Wells,
                WellToSoilReach);
            ReachHelper.SearchLinks(land, Composters,
                ComposterToSoilReach);
        }

        // Invoked when manager is set to `null`
        public bool RemoveFarmLand(IFarmLand land)
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
            return FarmLands.RemoveAt(land.WorldPos);
        }

    }
}
