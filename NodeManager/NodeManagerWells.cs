using KdTree3;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        int WellToPlantReach = 8;
        int IrrigatorToWellReach = 20;

        public readonly KdTree<MetricChebyshev>.Vector3i<PipeWell> Wells =
            new KdTree<MetricChebyshev>.Vector3i<PipeWell>(AddDuplicateBehavior.Update);

        public PipeWell AddWell(PipeWell well)
        {
            Log.Warning("========= ADDWELL");
            Wells.Add(well.WorldPos, well);
            // Search for output to fill up the well
            var wells = Irrigators.RadialSearch(
                well.WorldPos, IrrigatorToWellReach);
            for (int i = 0; i < wells.Length; i += 1)
            {
                well.Irrigators.Add(wells[i].Item2);
                wells[i].Item2.Wells.Add(well);

            }
            // Search for plants this well will serve
            var plants = PlantGrowings.RadialSearch(
                well.WorldPos, WellToPlantReach);
            for (int i = 0; i < plants.Length; i += 1)
            {
                well.Plants.Add(plants[i].Item2);
                plants[i].Item2.Wells.Add(well);
            }
            return well;
        }

        public void RemoveWell(PipeWell well)
        {
            foreach (var node in well.Plants)
                node.Wells.Remove(well);
            foreach (var node in well.Irrigators)
                node.Wells.Remove(well);
            well.Plants.Clear();
            well.Irrigators.Clear();
            Wells.RemoveAt(well.WorldPos);
        }

    }
}
