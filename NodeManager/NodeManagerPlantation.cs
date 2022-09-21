using KdTree3;
using System.Collections.Generic;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, PlantationGrowing> PlantGrowing
            = new Dictionary<Vector3i, PlantationGrowing>();

        public readonly KdTree<MetricChebyshev>.Vector3i<PlantationGrowing> PlantGrowings
            = new KdTree<MetricChebyshev>.Vector3i<PlantationGrowing>();

        public void AddPlantGrowing(PlantationGrowing plant)
        {
            Log.Warning("Add plant growing {0}", plant);
            PlantGrowing.Add(plant.WorldPos, plant);
            PlantGrowings.Add(plant.WorldPos, plant);
            System.Tuple<Vector3i, PipeWell>[] wells =
                Wells.RadialSearch(plant.WorldPos, 20);
            foreach (var kv in wells)
            {
                plant.Wells.Add(kv.Item2);
                kv.Item2.Plants.Add(plant);
            }
        }

        public bool RemovePlantGrowing(PlantationGrowing plant)
        {
            Log.Warning("Remove plant growing");
            foreach (var well in plant.Wells)
                well.Plants.Remove(plant);
            plant.Wells.Clear();
            PlantGrowings.RemoveAt(plant.WorldPos);
            return PlantGrowing.Remove(plant.WorldPos);
        }

        public void UpdatePlantStats(ActionUpdatePlantStats stats)
        {
            if (PlantGrowing.TryGetValue(stats.Position,
                out PlantationGrowing plant))
            {
                plant.CurrentSunLight = stats.SunLight;
                plant.CurrentFertility = stats.Fertility;
                plant.CurrentRain = stats.Rain;
            }
        }

    }
}
