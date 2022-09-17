using System.Collections.Generic;

namespace NodeFacilitator
{

    public class PipeGrid : IfaceGridNodeManaged
    {

        public int ID = -1;

        public ushort FluidType { get; set; } = 0;

        public bool HasSource { get; protected set; }

        public NodeManager Manager { get; protected set; }

        public PipeGrid(NodeManager manager)
        {
            Manager = manager;
        }

        public bool Debug = false;

        public int Count { get; protected set; } = 0;

        private HashSet<PipeConnection> Connections 
            = new HashSet<PipeConnection>();

        // public bool IsCyclic = false;

        // Invoked from `PipeConnection.UpdateGrid`
        internal void AddConnection(PipeConnection connection)
        {
            if (Debug) Log.Out("Grid connection added");
            if (++Count == 1) Manager.AddGrid(this);
            Connections.Add(connection);
        }

        // Invoked from `PipeConnection.UpdateGrid`
        internal void RemoveConnection(PipeConnection connection)
        {
            if (Debug) Log.Out("Grid connection removed");
            if (--Count == 0) Manager.RemoveGrid(this);
            Connections.Remove(connection);
            // Need to update neighbours for all removals

            //for (int side = 0; side < 6; side++)
            //{
            //    if (connection[side] != null)
            //    {
            //        byte mirror = FullRotation.Mirror(side);
            //        connection[side][mirror] = null;
            //    }
            //}

        }


        public override string ToString()
        {
            return string.Format(
                "Grid {0} has {1} Pipe(s) (fluid {2})",
                ID, Count, FluidType);
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

        public void CheckFluidType()
        {
            FluidType = 0;
            Log.Out("Checking the fluid type");
            foreach (var node in Connections)
            {
                if (!(node is IFilledState filled)) continue;
                if (filled.FillLevel < 1e-3) continue;
                if (FluidType == 0) FluidType = filled.FluidType;
                else if (FluidType != filled.FluidType)
                    Log.Error("FluidType out of sync");
            }
        }
    }

}

