using KdTree3;
using System.Collections.Generic;

namespace NodeManager
{

    public interface IFarmPlot
    {
        HashSet<IComposter> Composters { get; }
        public float WaterFactor { get; }
        public float SoilFactor { get; }

    }

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IFarmPlot> FarmPlots
            = new KdTree<MetricChebyshev>.Vector3i<IFarmPlot>();

        

        public void AddFarmPlot(PlantationFarmPlot plot)
        {
            Log.Warning("Add composter {0}", plot);
            FarmPlots.Add(plot.WorldPos, plot);
        }

        public bool RemoveFarmPlot(PlantationFarmPlot plant)
        {
            return FarmPlots.RemoveAt(plant.WorldPos);
        }

    }
}
