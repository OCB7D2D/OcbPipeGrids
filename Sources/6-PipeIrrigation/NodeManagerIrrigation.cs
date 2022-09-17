using KdTree3;

namespace NodeFacilitator
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Adjust for maximum parsed setting
        public static int IrrigatorToWellReach = 0;

        public readonly KdTree<MetricChebyshev>.Vector3i<IIrrigator> Irrigators =
            new KdTree<MetricChebyshev>.Vector3i<IIrrigator>(AddDuplicateBehavior.Update);

        internal void AddIrrigation(IIrrigator irrigation)
        {
            Irrigators.Add(irrigation.WorldPos, irrigation);
            // Search for existing wells in reach
            var results = Wells.RadialSearch(
                irrigation.WorldPos, IrrigatorToWellReach);
            for (int i = 0; i < results.Length; i += 1)
            {
                results[i].Item2.Irrigators.Add(irrigation);
                irrigation.Wells.Add(results[i].Item2);
            }
        }

        internal void RemoveIrrigation(IIrrigator irrigation)
        {
            foreach (var well in irrigation.Wells)
                well.Irrigators.Remove(irrigation);
            irrigation.Wells.Clear();
            Irrigators.RemoveAt(irrigation.WorldPos);
        }

    }
}
