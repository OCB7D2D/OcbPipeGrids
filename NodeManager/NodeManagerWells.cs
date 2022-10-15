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

        public PipeWell AddWell(PipeWell well)
        {
            Wells.Add(well.WorldPos, well);
            ReachHelper.QueryLinks(well, FarmSoils);
            ReachHelper.SearchLinks(well, Irrigators,
                IrrigatorToWellReach);
            // Search for irrigation to fill up the well
            var wells = Irrigators.RadialSearch(
                well.WorldPos, IrrigatorToWellReach);
            for (int i = 0; i < wells.Length; i += 1)
            {
                well.Irrigators.Add(wells[i].Item2);
                wells[i].Item2.Wells.Add(well);
            }

            return well;
        }

        public bool RemoveWell(PipeWell well)
        {
            // Make sure to unregister us from links
            foreach (var node in well.Irrigators)
                node.Wells.Remove(well);
            foreach (var node in well.Soils)
                node.Wells.Remove(well);
            // Clear our links
            well.Irrigators.Clear();
            well.Soils.Clear();
            // Remove from KD tree
            return Wells.RemoveAt(well.WorldPos);
        }

    }
}
