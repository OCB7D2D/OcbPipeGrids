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

        public readonly KdTree<MetricChebyshev>.Vector3i<IPlant> Plants
            = new KdTree<MetricChebyshev>.Vector3i<IPlant>();

        

        public void AddPlantGrowing(PlantationGrowing plant)
        {
            Log.Warning("Add plant growing {0}", plant);
            PlantGrowing.Add(plant.WorldPos, plant);
            PlantGrowings.Add(plant.WorldPos, plant);

            // We only need to check for reservoir below once
            // Because it is impossible to change once placed


            //Manager.Com

            // Search for existing plants
            System.Tuple<Vector3i, IPlant>[] plants =
                Plants.RadialSearch(plant.WorldPos, 3);
            foreach (var kv in plants)
            {
                plant.Plants.Add(kv.Item2);
                kv.Item2.Plants.Add(plant);
            }


            System.Tuple<Vector3i, IComposter>[] composters =
                Composters.RadialSearch(plant.WorldPos, 3);
            foreach (var kv in composters)
            {
                plant.Composters.Add(kv.Item2);
                kv.Item2.Plants.Add(plant);
            }

            // Add our plant to the index
            Plants.Add(plant.WorldPos, plant);

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
            foreach (var other in plant.Plants)
                other.Plants.Remove(plant);
            plant.Plants.Clear();
            foreach (var well in plant.Wells)
                well.Plants.Remove(plant);
            plant.Wells.Clear();
            Plants.RemoveAt(plant.WorldPos);
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
