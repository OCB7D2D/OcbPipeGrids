using KdTree3;

namespace NodeFacilitator
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Adjust for maximum parsed setting
        public static int GrowLightToPlantReach = 0;

        public readonly KdTree<MetricChebyshev>.Vector3i<IGrowLight> GrowLights
            = new KdTree<MetricChebyshev>.Vector3i<IGrowLight>();

        // Invoked when new manager is attached
        public void AddGrowLight(IGrowLight light)
        {
            Log.Out("Adding grow light {0} search plots {1}", light.WorldPos, FarmPlots.Count);
            GrowLights.Add(light.WorldPos, light);
            ReachHelper.QueryLinks(light, FarmPlots,
                new Vector3i(0, 1, 0)); // Specialize
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
