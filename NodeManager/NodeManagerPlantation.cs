using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
