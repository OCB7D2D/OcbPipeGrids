using KdTree3;
using System.Collections.Generic;

namespace NodeManager
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<IComposter> Composters
            = new KdTree<MetricChebyshev>.Vector3i<IComposter>();

        // Invoked when new manager is attached
        public void AddComposter(IComposter composter)
        {
            Composters.Add(composter.WorldPos, composter);
            ReachHelper.QueryLinks(composter, FarmSoils);
        }

        // Invoked when manager is set to `null`
        public bool RemoveComposter(IComposter composter)
        {
            // Make sure to unregister us from links
            foreach (var other in composter.Soils)
                other.Composters.Remove(composter);
            // Clear our links
            composter.Soils.Clear();
            // Remove from tree and dictionary
            return Composters.RemoveAt(composter.WorldPos);
        }

    }
}
