using KdTree3;
using System;
using System.Collections.Generic;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<ISoil> FarmSoils
            = new KdTree<MetricChebyshev>.Vector3i<ISoil>();

        public void AddFarmLand(PlantationFarmLand land)
        {
            FarmSoils.Add(land.WorldPos, land);
            ReachHelper.SearchLinks(land, Wells, WellToSoilReach);
            ReachHelper.SearchLinks(land, Composters, ComposterToSoilReach);
        }

        public bool RemoveFarmLand(PlantationFarmLand land)
        {
            foreach (var node in land.Wells)
                node.Soils.Remove(land);
            foreach (var node in land.Composters)
                node.Soils.Remove(land);
            land.Wells.Clear();
            land.Composters.Clear();
            return FarmSoils.RemoveAt(land.WorldPos);
        }

    }
}
