using KdTree3;

namespace NodeFacilitator
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Adjust for maximum parsed setting
        public static int WellToSoilReach = 0;
        public static int SprinklerToSoilReach = 0;

        public readonly KdTree<MetricChebyshev>.Vector3i<IWell> Wells =
            new KdTree<MetricChebyshev>.Vector3i<IWell>(AddDuplicateBehavior.Update);

        public void AddWell(FarmWell well)
        {
            Wells.Add(well.WorldPos, well);
            ReachHelper.QueryLinks(well, FarmLands, Vector3i.zero);
            ReachHelper.SearchLinks(well, Irrigators,
                IrrigatorToWellReach, Vector3i.zero);
        }

        public void RemoveWell(FarmWell well)
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
