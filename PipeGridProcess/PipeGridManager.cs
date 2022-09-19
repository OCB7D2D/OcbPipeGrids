// Process running in a separate thread
// Coded so it could also run on main thread
using KdTree3;
using System;
using System.Collections.Generic;
using System.IO;

namespace PipeManager
{

    public class PipeGridManager : GlobalTicker, IPersistable
    {

        public string GetPersistName() => "PipeGridManager";
        public string GetLoadInfo() => Nodes.Count.ToString();

        //#####################################################################
        // Class Factory for Pipe Node Specializations
        //#####################################################################

        public static readonly Dictionary<uint, Func<BinaryReader, PipeNode>>
            Factory = new Dictionary<uint, Func<BinaryReader, PipeNode>>();

        public static void RegisterFactory(uint id, Func<BinaryReader, PipeNode> creator)
        {
            Factory.Add(id, creator);
        }

        public PipeNode InstantiateItem(uint id, BinaryReader br)
        {
            if (Factory.TryGetValue(id, out var ctor))
            {
                var node = ctor(br);
                node.AttachToManager(this);
                return node;
            }
            Log.Error("Could not instantiate unknown PipeGrid Type {0}", id);
            return null;
        }

        //#####################################################################
        //#####################################################################

        public static int[] KdKey(Vector3i pos)
            => new int[] { pos.x, pos.y, pos.z };

        public List<PipeGrid> Grids = new List<PipeGrid>();

        public readonly Dictionary<Vector3i, PipeNode> Nodes
            = new Dictionary<Vector3i, PipeNode>();

        // Use a KD-Tree to optimize finding nearest items
        // ToDo: benchmark exact lookup vs using a hash table
        // Note: pretty sure spatial queries will be faster though
        // Note: might not be true for a small radial searches
        //public readonly KdTree<int, PipeConnection> Connections = new KdTree<int,
        //    PipeConnection>(3, new KdTree.Math.IntCubeMath(), AddDuplicateBehavior.Update);

        public readonly KdTree<MetricChebyshev>.Vector3i<PipeWell> Wells =
            new KdTree<MetricChebyshev>.Vector3i<PipeWell>(AddDuplicateBehavior.Update);

        public readonly KdTree<MetricChebyshev>.Vector3i<PipeIrrigation> Irrigators =
            new KdTree<MetricChebyshev>.Vector3i<PipeIrrigation>(AddDuplicateBehavior.Update);

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, IPoweredNode> IsPowered
            = new Dictionary<Vector3i, IPoweredNode>();

        //internal bool TryGetConnection(Vector3i position, out PipeConnection connection)
        //    => Connections.TryFindValueAt(KdKey(position), out connection);

        internal bool TryGetNode(Vector3i position, out PipeNode node)
            => Nodes.TryGetValue(position, out node);

        internal bool TryGetNode<T> (Vector3i position, out T node) where T : class
        {
            if (Nodes.TryGetValue(position,
                out PipeNode instance))
            {
                node = instance as T;
                return true;
            }
            node = null;
            return false;
        }

        //#####################################################################
        //#####################################################################

        public void Cleanup()
        {
            // Persister.SaveSynchronous();
            foreach (var kv in Nodes)
                kv.Value.Cleanup();
            //Connections.Clear();
            Nodes.Clear();
            Wells.Clear();
            Grids.Clear();
        }

        //#####################################################################
        // Persistence interface and implementations
        //#####################################################################

        private static readonly byte FileVersion = 1;

        private readonly PersistedData Persister;

        public PipeGridManager()
        {
            Persister = new PersistedData(this);
        }

        // Path to persist the data to (filename)
        // ToDo: check if this is thread-safe enough!?
        public string GetStoragePath() => string.Format(
            "{0}/pipe-grids.dat", GameIO.GetSaveGameDir());
        public string GetBackupPath() => string.Format(
            "{0}/pipe-grids.dat.bak", GameIO.GetSaveGameDir());
        public string GetThreadKey() => "silent_PipeGridManagerDataSave";

        public void LoadData()
        {
            try
            {
                Persister.LoadPersistedData();
            }
            catch (Exception)
            {
                Log.Error("Error loading PipeGridManager data");
            }
        }

        public void SaveData()
        {
            try
            {
                Persister.SaveSynchronous();
            }
            catch (Exception)
            {
                Log.Error("Error storing PipeGridManager data");
            }
        }

        public virtual void Write(BinaryWriter bw)
        {
            bw.Write(FileVersion);
            Log.Out("{0} saving {1} {2}",
                GetPersistName(),
                GetLoadInfo(),
                Nodes.Count);
            bw.Write(Nodes.Count);
            foreach (var kv in Nodes)
            {
                // Make sure to write out the header first
                bw.Write(kv.Value.StorageID);
                // Write can be overridden
                kv.Value.Write(bw);
                // Store power state automatically
                if (kv.Value is IPoweredNode powered)
                    bw.Write(powered.IsPowered);
            }
        }

        public virtual void Read(BinaryReader br)
        {
            var version = br.ReadByte();
            int nodes = br.ReadInt32();
            Log.Out("Reading X nodes {0}", nodes);
            for (int index = 0; index < nodes; ++index)
            {
                uint type = br.ReadUInt32();
                var node = InstantiateItem(type, br);
                // Restore power state automatically
                if (node is IPoweredNode powered)
                {
                    IsPowered[node.WorldPos] = powered;
                    powered.IsPowered = br.ReadBoolean();
                }
            }
        }

        //#####################################################################
        // Hooks for pipe connections
        //#####################################################################

        internal void AddPipeGridNode(PipeNode node)
        {
            Nodes.Add(node.WorldPos, node);
            if (node is PipeConnection connection) AddConnection(connection);
            if (node is PipeIrrigation irrigation) AddIrrigation(irrigation);
            if (node is IPoweredNode powered) AddPowered(powered);
            if (node is PipeWell well) AddWell(well);
        }

        public void RemovePipeGridNode(Vector3i position)
        {
            // Log.Out("1 Remove from pipe connection");
            if (TryGetNode(position, out PipeConnection node))
            {
                // Log.Out("2 Remove from pipe connection");
                // node.AddConnection
                RemoveConnection(position, node);
            }
            if (Irrigators.ContainsKey(position))
                RemoveIrrigation(position);
            if (IsPowered.ContainsKey(position))
                RemovePowered(position);
            if (Wells.ContainsKey(position))
                RemoveWell(position);
            if (Nodes.ContainsKey(position))
                Nodes.Remove(position);
        }

        private bool RemovePowered(Vector3i position)
        {
            return IsPowered.Remove(position);
        }

        private bool RemoveConnection(Vector3i position, PipeConnection node)
        {
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
                            Log.Out("Create new grid -----");
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


        private bool AddConnection(PipeConnection connection)
        {
            connection.AddConnection(this);
            return true; // Connections.Add(KdKey(connection.WorldPos), connection);
        }

        private void AddPowered(IPoweredNode powered)
        {
            IsPowered.Add(powered.WorldPos, powered);
        }



        private void AddIrrigation(PipeIrrigation irrigation)
        {
            Irrigators.Add(irrigation.WorldPos, irrigation);
            // Search for existing wells in reach
            var results = Wells.RadialSearch(
                irrigation.WorldPos, 20 /*, NbWellCache */);
            for (int i = 0; i < results.Length; i += 1)
                results[i].Item2.AddIrrigation(irrigation);
        }

        private void RemoveIrrigation(Vector3i position)
        {
            Irrigators.RemoveAt(position);
        }

        private static readonly NearestNeighbourList<PipeWell> NbWellCache
            = new NearestNeighbourList<PipeWell>();
        private static readonly NearestNeighbourList<PipeIrrigation> NbIrrigatorCache
            = new NearestNeighbourList<PipeIrrigation>();

        public PipeWell AddWell(PipeWell well)
        {
            Log.Warning("========= ADDWELL");
            Wells.Add(well.WorldPos, well);
            // Search for output to fill up the well
            var results = Irrigators.RadialSearch(
                well.WorldPos, 20 /*, NbIrrigatorCache*/);
            for (int i = 0; i < results.Length; i += 1)
            {
                well.AddIrrigation(results[i].Item2);
            }
            //foreach (var output in Find<PipeGridOutput>(
            //    well.WorldPos, OutputArea, OutputHeight))
            //{
            //    // Add cross-references
            //    output.AddWell(well);
            //    well.AddOutput(output);
            //}
            //
            //// PlantManager.OnWellAdded(well);
            //
            //// Search for plants that could use us
            //foreach (var plant in Find<PlantGrowing>(
            //    well.WorldPos, well.SearchArea, well.SearchHeight))
            //{
            //    // Add reference
            //    plant.AddWell(well);
            //}
            //
            //// Register positions in our dictionary
            //if (false && well.GetBlock() is Block block && block.isMultiBlock)
            //{
            //    int rotation = well.Rotation;
            //    int length = block.multiBlockPos.Length;
            //    for (int _idx = 0; _idx < length; ++_idx)
            //    {
            //        var pos = block.multiBlockPos.Get(
            //            _idx, block.blockID, rotation);
            //        Wells[pos + well.WorldPos] = well;
            //    }
            //}
            //else
            //{
            //    // Block has only one position
            //    Wells[well.WorldPos] = well;
            //}
            //
            return well;
        }

        public bool RemoveWell(Vector3i position)
        {
            if (Wells.TryFindValueAt(position,
                out PipeWell well))
            {
                // Search for output to fill up the well
                var results = Irrigators.RadialSearch(
                    position, 20 /*, NbIrrigatorCache */);
                //for (int i = 0; i < results.Length; i += 1)
                //{
                //    well.RemoveIrrigation(results[i].Value);
                //}
                Wells.RemoveAt(position);
                return true;
            }

            //if (Wells.TryGetValue(position,
            //    out PipeGridWell well))
            //{
            //    foreach (var output in Find<PipeGridOutput>(
            //        position, OutputArea, OutputHeight))
            //    {
            //        output.RemoveWell(well);
            //        well.RemoveOutput(output);
            //    }
            //    // Search for plants that could use us
            //    foreach (var plant in Find<PlantGrowing>(
            //        well.WorldPos, OutputArea, OutputHeight))
            //    {
            //        // Add reference
            //        plant.RemoveWell(well);
            //    }
            //    Wells.Remove(position);
            //    return true;
            //}
            return false;
        }

        public void UpdatePower(Vector3i position, bool powered)
        {
            if (IsPowered.TryGetValue(position,
                out IPoweredNode node))
            {
                node.IsPowered = powered;
            }
        }

        //#####################################################################
        // Hooks for pipe grids
        //#####################################################################

        public bool AddGrid(PipeGrid grid)
        {
            var idx = Grids.IndexOf(grid);
            if (idx != -1) return false;
            Log.Warning("Create a new grid");
            grid.ID = Grids.Count;
            Grids.Add(grid);
            return true;
        }

        public bool RemoveGrid(PipeGrid grid)
        {
            var idx = Grids.IndexOf(grid);
            Console.WriteLine("Removing at {0}", idx);
            if (idx == -1) return false;
            Grids.RemoveAt(idx);
            while (idx < Grids.Count)
                Grids[idx++].ID--;
            foreach (var i in Grids)
                Console.WriteLine("Grid {0}", i);
            return true;
        }


        // private static readonly NearestNeighbourList<KdTreeNode<int, PipeConnection>, int> NbCache
        //     = new NearestNeighbourList<KdTreeNode<int, PipeConnection>, int>(new KdTree.Math.IntegerMath());

        public byte GetNeighbours(Vector3i position, ref BlockConnector[] Neighbours)
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
