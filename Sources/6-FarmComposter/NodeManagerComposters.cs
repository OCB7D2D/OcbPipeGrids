using KdTree3;

namespace NodeFacilitator
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Adjust for maximum parsed setting
        public static int ComposterToSoilReach = 0;

        // Keep all items in a queryable container for nearby nodes to find us
        public readonly KdTree<MetricChebyshev>.Vector3i<IComposter> Composters
            = new KdTree<MetricChebyshev>.Vector3i<IComposter>();

        // Invoked when new manager is attached
        public void AddComposter(IComposter composter)
        {
            Composters.Add(composter.WorldPos, composter);
            ReachHelper.QueryLinks(composter, FarmLands, Vector3i.zero);
            ReachHelper.QueryLinks(composter, FarmPlots, Vector3i.zero);
        }

        // Invoked when manager is set to `null`
        public bool RemoveComposter(IComposter composter)
        {
            // Make sure to unregister us from links
            foreach (var other in composter.FarmLands)
                other.Composters.Remove(composter);
            foreach (var other in composter.FarmPlots)
                other.Composters.Remove(composter);
            // Clear our reverse links
            composter.FarmLands.Clear();
            composter.FarmPlots.Clear();
            // Remove from tree and dictionary
            return Composters.RemoveAt(composter.WorldPos);
        }

    }
}
