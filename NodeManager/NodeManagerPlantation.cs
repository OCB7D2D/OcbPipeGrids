using KdTree3;
using System.Collections.Generic;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        private const int PlantToPlantReach = 3;

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, IPlant> PlantsDict
            = new Dictionary<Vector3i, IPlant>();

        public readonly KdTree<MetricChebyshev>.Vector3i<IPlant> PlantsTree
            = new KdTree<MetricChebyshev>.Vector3i<IPlant>();

        public void AddPlantGrowing(IPlant plant)
        {
            PlantsDict.Add(plant.WorldPos, plant);
            PlantsTree.Add(plant.WorldPos, plant);
            ReachHelper.AddLinks(plant, PlantsTree,
                PlantToPlantReach);
        }

        public bool RemovePlantGrowing(IPlant plant)
        {
            // Make sure to unregister us from links
            foreach (var other in plant.Plants)
                other.Plants.Remove(plant);
            // Clear our links
            plant.Plants.Clear();
            // Remove from tree and dictionary
            return PlantsTree.RemoveAt(plant.WorldPos)
                || PlantsDict.Remove(plant.WorldPos);
        }

        public void UpdatePlantStats(ActionUpdatePlantStats stats)
        {
            // Use dictionary as it is hopefully faster than KD tree
            if (PlantsDict.TryGetValue(stats.Position, out IPlant plant))
            {
                plant.CurrentSunLight = stats.SunLight;
                //plant.CurrentFertility = stats.Fertility;
                //plant.CurrentRain = stats.Rain;
            }
        }

    }
}
