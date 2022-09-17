using KdTree3;
using System.Collections.Generic;

namespace NodeFacilitator
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        private const int PlantToPlantReach = 3;
        private const int SprinklerToPlantReach = 20;

        // Store all plants in a dictionary to update state
        public readonly Dictionary<Vector3i, IPlant> PlantsDict
            = new Dictionary<Vector3i, IPlant>();

        // Store all plants in a KD tree to find plants nearby
        public readonly KdTree<MetricChebyshev>.Vector3i<IPlant> PlantsTree
            = new KdTree<MetricChebyshev>.Vector3i<IPlant>();

        public void AddPlantGrowing(IPlant plant)
        {
            ReachHelper.AddLinks(plant, PlantsTree,
                PlantToPlantReach);
            ReachHelper.SearchLinks(plant, Sprinklers,
                SprinklerToPlantReach, Vector3i.zero);
            PlantsDict.Add(plant.WorldPos, plant);
            PlantsTree.Add(plant.WorldPos, plant);
        }

        public void RemovePlantGrowing(IPlant plant)
        {
            // Make sure to unregister us from links
            foreach (var other in plant.Sprinklers)
                other.Plants.Remove(plant);
            foreach (var other in plant.Plants)
            {
                if (other == plant) continue;
                other.Plants.Remove(plant);
            }
            // Clear our links
            plant.Plants.Clear();
            plant.Sprinklers.Clear();
            // Remove from tree and dictionary
            PlantsTree.RemoveAt(plant.WorldPos);
            PlantsDict.Remove(plant.WorldPos);
        }

        public void UpdatePlantStats(ActionUpdateLightLevels stats)
        {
           //  Log.Warning("Update Plant Stats for light {0}", stats.SunLight);
            // Use dictionary as it is hopefully faster than KD tree
            if (PlantsDict.TryGetValue(stats.Position, out IPlant plant))
            {
                //Log.Warning("Update Plant Stats for light {0}", stats.SunLight);
                plant.CurrentSunLight = stats.SunLight;
                //plant.CurrentFertility = stats.Fertility;
                //plant.CurrentRain = stats.Rain;
            }
        }

    }
}
