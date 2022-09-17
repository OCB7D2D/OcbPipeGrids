namespace NodeFacilitator
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        // Invoked by PipeConnection.OnManagerAttached
        internal bool RemoveConnection(PipeConnection node)
        {
            // Log.Out("Removing Connection from {0}", node.Grid);
            if (TryGetNode(node.WorldPos, out PipeConnection connection))
            {
                if (connection.Grid != null)
                {
                    bool needGridSplit = false;
                    // First remove from the existing grid
                    // Simply counts and disposes empty grids
                    connection.Grid = null;
                    // Update all neighbour connections/grids
                    for (int side = 0; side < 6; side++)
                    {
                        var neighbour = connection[side];
                        if (neighbour == null) continue;
                        if (needGridSplit == false)
                        {
                            // Switch branch flag
                            needGridSplit = true;
                            //neighbour.Grid?.CheckFluidType();
                        }
                        else
                        {
                            // Assign new grid to current side
                            PipeGrid grid = new PipeGrid(this);
                            // Propagate that change into neighbour tree
                            neighbour.PropagateGridChange(connection, grid);
                            //neighbour.Grid?.CheckFluidType();
                            //grid.CheckFluidType();
                        }
                        // Reset neighbour on the other side of the link
                        neighbour[FullRotation.Mirror(side)] = null;
                    }
                }
                else
                {
                    Log.Warning("Known connection doesn't have grid!?");
                }
                return true;
            }
            else
            {
                Log.Warning("Removing connection that isn't known!?");
            }
            return false;
        }

        // Invoked by PipeConnection.OnManagerAttached
        internal bool AddConnection(PipeConnection connection)
        {
            if (connection.Grid != null) Log.Warning("Loaded conn with grid");
            UpdateNeighbours(connection, connection.Neighbours);
            int grids = 0; int source = -1; int first = -1;
            for (int side = 0; side < 6; side++)
            {
                if (connection.Neighbours[side] == null) continue;
                if (connection.Neighbours[side].Grid == null)
                {
                    Log.Error("Neighbour without grid found");
                }
                else if (first == -1)
                {
                    first = side;
                }
                grids++;
            }

            if (source == -1)
            {
                source = first;
            }

            if (grids == 0)
            {
                connection.Grid = new PipeGrid(this);
            }
            else
            {
                //Log.Out("Joining grid");
                connection.PropagateGridChange(
                    connection.Neighbours[source]);
                //Log.Out("Joined grid");
            }

            return true;
        }

        // Invoked by `MsgConnectorQuery` for response
        public void GetAllNeighbours(Vector3i position,
            ref BlockConnector[] Neighbours)
        {
            // Collect existing neighbour nodes
            for (byte side = 0; side < 6; side++)
            {
                Neighbours[side].Reset();
                // Try to fetch the node at the given side
                var offset = FullRotation.Vector[side];
                if (TryGetNode(position + offset,
                    out PipeConnection neighbour))
                {
                    Neighbours[side].Set(neighbour);
                }
            }
        }

        // Used by `AddConnection` to gather up to date info
        private byte UpdateNeighbours(PipeConnection pipe, PipeConnection[] Neighbours)
        {
            byte count = 0;
            // Collect neighbours from existing blocks/connections
            for (byte side = 0; side < 6; side++)
            {
                Neighbours[side] = null;
                // Check if we can connect to side
                if (!pipe.CanConnect(side)) continue;
                // Try to fetch the node at the given side
                var offset = FullRotation.Vector[side];
                if (TryGetNode(pipe.WorldPos + offset,
                    out PipeConnection neighbour))
                {
                    // Check if other one can connect to use
                    byte mirrored = FullRotation.Mirror(side);
                    if (!neighbour.CanConnect(mirrored)) continue;
                    // Update the node connectors
                    Neighbours[side] = neighbour;
                    neighbour[mirrored] = pipe;
                    count += 1;
                }
            }

            return count;
        }

    }
}
