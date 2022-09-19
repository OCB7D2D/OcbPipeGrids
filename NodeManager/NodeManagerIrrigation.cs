using KdTree3;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<PipeIrrigation> Irrigators =
            new KdTree<MetricChebyshev>.Vector3i<PipeIrrigation>(AddDuplicateBehavior.Update);

        // private static readonly NearestNeighbourList<PipeWell> NbWellCache
        //     = new NearestNeighbourList<PipeWell>();
        // private static readonly NearestNeighbourList<PipeIrrigation> NbIrrigatorCache
        //     = new NearestNeighbourList<PipeIrrigation>();

        private void AddIrrigation(PipeIrrigation irrigation)
        {
            Irrigators.Add(irrigation.WorldPos, irrigation);
            // Search for existing wells in reach
            var results = Wells.RadialSearch(
                irrigation.WorldPos, 20 /*, NbWellCache */);
            for (int i = 0; i < results.Length; i += 1)
                results[i].Item2.AddIrrigation(irrigation);
        }

        private void RemoveIrrigation(Vector3i position)
        {
            Irrigators.RemoveAt(position);
        }

    }
}
