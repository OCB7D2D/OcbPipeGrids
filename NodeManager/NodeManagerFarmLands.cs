using KdTree3;
using System;
using System.Collections.Generic;

namespace NodeManager
{

    public interface IFarmLand : IEqualityComparer<NodeBase>
    {
        HashSet<IWell> Wells { get; }
        HashSet<IComposter> Composters { get; }
        public float WaterFactor { get; }
        public float SoilFactor { get; }

    }

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IFarmLand> FarmLands
            = new KdTree<MetricChebyshev>.Vector3i<IFarmLand>();

        public void AddFarmLand(PlantationFarmLand land)
        {
            Log.Warning("Add Farm Land {0}", land);
            FarmLands.Add(land.WorldPos, land);
            System.Tuple<Vector3i, PipeWell>[] wells =
                Wells.RadialSearch(land.WorldPos, 20);
            foreach (var kv in wells)
            {
                if (!BlockHelper.IsInReach(
                    land.WorldPos, kv.Item1,
                    kv.Item2.BLOCK.BlockReach)) continue;
                land.Wells.Add(kv.Item2);
                kv.Item2.FarmLands.Add(land);
            }
        }

        public bool RemoveFarmLand(PlantationFarmLand land)
        {
            return FarmLands.RemoveAt(land.WorldPos);
        }

    }
}
