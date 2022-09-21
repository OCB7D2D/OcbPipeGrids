﻿using KdTree3;

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

        internal void AddIrrigation(PipeIrrigation irrigation)
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

        internal void RemoveIrrigation(PipeIrrigation irrigation)
        {
            foreach (var well in irrigation.Wells)
                well.Irrigators.Remove(irrigation);
            irrigation.Wells.Clear();
            Irrigators.RemoveAt(irrigation.WorldPos);
        }

    }
}
