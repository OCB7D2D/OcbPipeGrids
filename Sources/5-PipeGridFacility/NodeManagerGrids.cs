using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeFacilitator
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public List<PipeGrid> Grids
            = new List<PipeGrid>();

        public bool AddGrid(PipeGrid grid)
        {
            var idx = Grids.IndexOf(grid);
            if (idx != -1) return false;
            // Log.Warning("Create a new grid");
            grid.ID = Grids.Count;
            Grids.Add(grid);
            return true;
        }

        public bool RemoveGrid(PipeGrid grid)
        {
            var idx = Grids.IndexOf(grid);
            //Console.WriteLine("Removing at {0}", idx);
            if (idx == -1) return false;
            Grids.RemoveAt(idx);
            while (idx < Grids.Count)
                Grids[idx++].ID--;
            //foreach (var i in Grids)
            //    Console.WriteLine("Grid {0}", i);
            return true;
        }

    }
}
