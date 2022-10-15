using KdTree3;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IComposter> Composters
            = new KdTree<MetricChebyshev>.Vector3i<IComposter>();

        // Invoked when new manager is attached
        public void AddComposter(IComposter composter)
        {
            Composters.Add(composter.WorldPos, composter);
            ReachHelper.QueryLinks(composter, FarmLands);
            ReachHelper.QueryLinks(composter, FarmPlots);
        }

        // Invoked when manager is set to `null`
        public bool RemoveComposter(IComposter composter)
        {
            // Make sure to unregister us from links
            foreach (var other in composter.FarmLands)
                other.Composters.Remove(composter);
            foreach (var other in composter.FarmPlots)
                other.Composters.Remove(composter);
            // Clear our links
            composter.FarmLands.Clear();
            composter.FarmPlots.Clear();
            // Remove from tree and dictionary
            return Composters.RemoveAt(composter.WorldPos);
        }

    }
}
