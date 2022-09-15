namespace PipeManager
{

    public class PipeGrid : IfaceGridNodeManaged
    {

        public int ID = -1;

        public bool HasSource { get; protected set; }

        public PipeGridManager Manager { get; protected set; }

        public PipeGrid(PipeGridManager manager)
        {
            Manager = manager;
        }

        int Count = 0;

        public bool IsCyclic = false;

        // Invoked from `PipeGrid.UpdateGrid`
        internal void AddConnection(PipeConnection connection)
        {
            if (++Count == 1) Manager.AddGrid(this);
        }

        // Invoked from `PipeGrid.UpdateGrid`
        internal void RemoveConnection(PipeConnection connection)
        {
            if (--Count == 0) Manager.RemoveGrid(this);
            // Need to update neighbours for all removals

            for (int side = 0; side < 6; side++)
            {
                if (connection[side] != null)
                {
                    byte mirror = FullRotation.Mirror(side);
                    connection[side][mirror] = null;
                }
            }

        }


        public override string ToString()
        {
            return string.Format(
                "Grid: {0} has {1} Pipe(s)",
                ID, Count);
        }

        // This may be done more efficient, but it
        // is an edge-case and will be optimized last
        //public void ResetGridState()
        //{
        //    foreach (var con in PipeGridServer.Instance.Connections)
        //    {
        //        // Only work on connection from our grid
        //        if (con.Value.Grid != this) continue;
        //    }
        //}

    }

}

