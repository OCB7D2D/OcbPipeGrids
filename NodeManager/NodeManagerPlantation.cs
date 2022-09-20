using System.Collections.Generic;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, PlantationGrowing> PlantGrowing
            = new Dictionary<Vector3i, PlantationGrowing>();

        public bool RemovePlantGrowing(Vector3i position)
        {
            Log.Warning("Remove plant growing");
            return PlantGrowing.Remove(position);
        }

        public void AddPlantGrowing(PlantationGrowing plant)
        {
            Log.Warning("Add plant growing {0}", plant);
            PlantGrowing.Add(plant.WorldPos, plant);
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
