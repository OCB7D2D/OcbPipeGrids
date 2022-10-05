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

        public bool RemoveConnection(PipeConnection node)
        {

            Vector3i position = node.WorldPos;

            Log.Out("Removing COnnection from {0}", node.Grid);
            // node.Grid = null; // Invoke `UpdateGrid`
            // connection.AddConnection(this);
            // Connections.RemoveAt(KdKey(position));
            if (TryGetNode(position, out PipeConnection connection))
            // Log.Warning("Found the node from nodes array");
            //if (Connections.TryFindValueAt(KdKey(position),
            //    out PipeConnection connection))
            {
                Log.Out("-- FOUND PIPING {0}", connection.Grid);
                if (connection.Grid != null)
                {
                    bool hasNothingYet = true;
                    // First remove from  the existing grid
                    // Simply counts and disposes empty grids
                    connection.Grid = null;
                    // Update all neighbour connections/grids
                    for (int side = 0; side < 6; side++)
                    {
                        var neighbour = connection[side];
                        if (neighbour == null) continue;
                        if (hasNothingYet == true)
                        {
                            // Re-check grid if it was cyclic before
                            if (neighbour.Grid != null && neighbour.Grid.IsCyclic)
                            {
                                // Reset cyclic flag and recheck
                                neighbour.Grid.IsCyclic = false;
                                // Propagate that change into neighbour tree
                                neighbour.PropagateGridChange(connection);
                            }
                            // Switch branch flag
                            hasNothingYet = false;
                        }
                        else
                        {
                            //Log.Out("Create new grid -----");
                            // Assign new grid to current connection
                            // connection.Grid = new PipeGrid();
                            // Propagate that change into neighbour tree
                            neighbour.PropagateGridChange(connection, new PipeGrid(this));
                        }
                        // Reset neighbour on the other side of the link
                        neighbour[FullRotation.Mirror(side)] = null;
                    }
                }
                else
                {
                    Log.Warning("Known connection doesn't have grid!?");
                }
                // Remove from connections
                //Connections.RemoveAt(KdKey(position));
                return true;
            }
            else
            {
                Log.Warning("Removing connection that isn't known!?");
            }
            return false;
        }

        internal bool AddConnection(PipeConnection connection)
        {
            // connection.AddConnection(this);

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

            // manager.AddPipeGridNode(this);

            if (grids == 0)
            {
                //Log.Out("Creating Grid");
                connection.Grid = new PipeGrid(this);
                //Log.Out("Created Grid");
            }
            else
            {
                for (byte side = 0; side < 6; side++)
                {
                    if (connection.Neighbours[side] == null) continue;
                }
                //Log.Out("Joining grid");
                // Grid = Neighbours[source].Grid;
                connection.PropagateGridChange(
                    connection.Neighbours[source]);
                //Log.Out("Joined grid");
                for (byte side = 0; side < 6; side++)
                {
                    if (connection.Neighbours[side] == null) continue;
                    // Log.Out("Neigh {0}", Neighbours[side].CountLongestDistance());
                }
            }

            return true; // Connections.Add(KdKey(connection.WorldPos), connection);
        }


        public byte GetNeighbours(Vector3i position,
            ref BlockConnector[] Neighbours,
            byte pipeDiameter = 0)
        {
            byte count = 0;
            //NbCache.Clear();
            // Collect neighbours from existing blocks/connections
            for (byte side = 0; side < 6; side++)
            {
                Neighbours[side].Reset();
                // Try to fetch the node at the given side
                var offset = FullRotation.Vector[side];
                if (TryGetNode(position + offset,
                    out PipeConnection neighbour))
                {
                    // Check if diameters are OK to connect to
                    //byte diameter = neighbour.BLOCK.PipeDiameter;
                    //Log.Out("Check {0} vs {1}", diameter, pipeDiameter);
                    //if (diameter != 0 && pipeDiameter != 0 &&
                    //    diameter != pipeDiameter) continue;
                    Log.Out("Found {0}", neighbour);
                    Neighbours[side].Set(neighbour);
                    count += 1;
                }
            }
            //for (int i = 0; i < 6; i += 1) NB[i].Reset();
            //if (NB.Length != 6) throw new ArgumentException("NB size mismatch");
            //// ToDo: Can we optimize the math to have true radial and cubic searches?
            //foreach (var node in Connections.RadialSearch(KdKey(position), 1, NbCache))
            //{
            //    Vector3i delta = node.Value.WorldPos - position;
            //    if (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z != 1) continue;
            //    NB[FullRotation.VectorToSide(delta)].Set(node.Value);
            //    count += 1;
            //}
            return count;
        }

        public byte UpdateNeighbours(PipeConnection pipe, PipeConnection[] Neighbours)
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

            // NbCache.Clear();
            // for (int i = 0; i < 6; i += 1) NB[i] = null;
            // if (NB.Length != 6) throw new ArgumentException("NB size mismatch");
            // // ToDo: Can we optimize the math to have true radial and cubic searches?
            // foreach (var node in Connections.RadialSearch(KdKey(position), 1, NbCache))
            // {
            //     Vector3i delta = node.Value.WorldPos - position;
            //     if (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z != 1) continue;
            //     NB[FullRotation.VectorToSide(delta)] = node.Value;
            //     count += 1;
            // }
            return count;
        }

    }
}
