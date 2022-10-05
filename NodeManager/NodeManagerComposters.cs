using KdTree3;
using System.Collections.Generic;

namespace NodeManager
{

    public interface IComposter
    {
        HashSet<IPlant> Plants { get; }

        float GrowProgress { get; set; }

    }

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IComposter> Composters
            = new KdTree<MetricChebyshev>.Vector3i<IComposter>();

        

        public void AddComposter(PlantationComposter composter)
        {
            Log.Warning("Add composter {0}", composter);
            Composters.Add(composter.WorldPos, composter);

            var plants = Plants.RadialSearch(composter.WorldPos,
                            3); // BlockReach

            foreach (var kv in plants)
            {
                composter.Plants.Add(kv.Item2);
                kv.Item2.Composters.Add(composter);
            }
        }

        public bool RemoveComposter(PlantationComposter plant)
        {
            return Composters.RemoveAt(plant.WorldPos);
        }

    }
}
