using KdTree3;
using System.Collections.Generic;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IFarmPlot> FarmPlots
            = new KdTree<MetricChebyshev>.Vector3i<IFarmPlot>();

        

        public void AddFarmPlot(PlantationFarmPlot plot)
        {
            Log.Warning("Add FarmPlot {0}", plot);
            FarmPlots.Add(plot.WorldPos, plot);
            ReachHelper.SearchLinks(plot, Composters,
                ComposterToSoilReach);
        }

        public void RemoveFarmPlot(PlantationFarmPlot plot)
        {
            Log.Out("Remove Farm plot called");
            // Make sure to unregister us from links
            foreach (var node in plot.Composters)
                node.FarmPlots.Remove(plot);
            // Clear our links
            plot.Composters.Clear();
            // Remove from KD tree
            FarmPlots.RemoveAt(plot.WorldPos);
        }

    }
}
