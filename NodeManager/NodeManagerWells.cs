using KdTree3;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        int WellToSoilReach = 4;
        int ComposterToSoilReach = 3;
        int IrrigatorToWellReach = 20;

        public readonly KdTree<MetricChebyshev>.Vector3i<IWell> Wells =
            new KdTree<MetricChebyshev>.Vector3i<IWell>(AddDuplicateBehavior.Update);

        public void AddWell(PipeWell well)
        {
            Wells.Add(well.WorldPos, well);
            ReachHelper.QueryLinks(well, FarmLands);
            ReachHelper.SearchLinks(well, Irrigators,
                IrrigatorToWellReach);
        }

        public void RemoveWell(PipeWell well)
        {
            // Make sure to unregister us from links
            foreach (var node in well.Irrigators)
                node.Wells.Remove(well);
            foreach (var node in well.FarmLands)
                node.Wells.Remove(well);
            // Clear our links
            well.Irrigators.Clear();
            well.FarmLands.Clear();
            // Remove from KD tree
            Wells.RemoveAt(well.WorldPos);
        }

    }
}
