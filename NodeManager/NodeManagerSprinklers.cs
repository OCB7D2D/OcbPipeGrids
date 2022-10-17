using KdTree3;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<ISprinkler> Sprinklers
            = new KdTree<MetricChebyshev>.Vector3i<ISprinkler>();

        // Invoked when new manager is attached
        public void AddSprinkler(ISprinkler sprinkler)
        {
            Sprinklers.Add(sprinkler.WorldPos, sprinkler);
            // ReachHelper.QueryLinks(composter, FarmLands);
            // ReachHelper.QueryLinks(composter, FarmPlots);
        }

        // Invoked when manager is set to `null`
        public bool RemoveSprinkler(ISprinkler sprinkler)
        {
            // Make sure to unregister us from links
            foreach (var other in sprinkler.Plants)
                other.Sprinklers.Remove(sprinkler);
            // Clear our links
            sprinkler.Plants.Clear();
            // Remove from tree and dictionary
            return Sprinklers.RemoveAt(sprinkler.WorldPos);
        }

    }
}
