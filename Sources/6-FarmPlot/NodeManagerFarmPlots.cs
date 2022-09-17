using KdTree3;

namespace NodeFacilitator
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Keep all items in a queryable container for nearby nodes to find us
        // ToDo: we basically only need a dictionary here, as we have no reach
        public readonly KdTree<MetricChebyshev>.Vector3i<IFarmPlot> FarmPlots
            = new KdTree<MetricChebyshev>.Vector3i<IFarmPlot>();

        // Invoked when new manager is attached
        public void AddFarmPlot(IFarmPlot plot)
        {
            FarmPlots.Add(plot.WorldPos, plot);
            ReachHelper.SearchLinks(plot, Composters,
                ComposterToSoilReach, Vector3i.zero);
            ReachHelper.SearchLinks(plot, GrowLights,
                GrowLightToPlantReach, new Vector3i(0, 1, 0));
        }

        // Invoked when manager is set to `null`
        public void RemoveFarmPlot(IFarmPlot plot)
        {
            // Make sure to unregister us from links
            foreach (var node in plot.GrowLights)
                node.FarmPlots.Remove(plot);
            foreach (var node in plot.Composters)
                node.FarmPlots.Remove(plot);
            // Clear our links
            plot.GrowLights.Clear();
            plot.Composters.Clear();
            // Remove from KD tree
            FarmPlots.RemoveAt(plot.WorldPos);
        }

    }
}
