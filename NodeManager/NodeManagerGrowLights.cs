using KdTree3;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IGrowLight> GrowLights
            = new KdTree<MetricChebyshev>.Vector3i<IGrowLight>();

        // Invoked when new manager is attached
        public void AddGrowLight(IGrowLight light)
        {
            GrowLights.Add(light.WorldPos, light);
            ReachHelper.QueryLinks(light, FarmPlots);
        }

        // Invoked when manager is set to `null`
        public bool RemoveGrowLight(IGrowLight light)
        {
            // Make sure to unregister us from links
            foreach (var other in light.FarmPlots)
                other.GrowLights.Remove(light);
            // Clear our links
            light.FarmPlots.Clear();
            // Remove from tree and dictionary
            return GrowLights.RemoveAt(light.WorldPos);
        }

    }
}
