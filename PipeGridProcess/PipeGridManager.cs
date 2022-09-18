// Process running in a separate thread
// Coded so it could also run on main thread
using KdTree;
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
        public readonly KdTree<int, PipeConnection> Connections = new KdTree<int,
            PipeConnection>(3, new KdTree.Math.IntCubeMath(), AddDuplicateBehavior.Update);

        public readonly KdTree<int, PipeWell> Wells = new KdTree<int,
            PipeWell>(3, new KdTree.Math.IntCubeMath(), AddDuplicateBehavior.Update);

        public readonly KdTree<int, PipeIrrigation> Irrigators = new KdTree<int,
            PipeIrrigation>(3, new KdTree.Math.IntCubeMath(), AddDuplicateBehavior.Update);

        // Store all powered items in a dictionary to update state
        public readonly Dictionary<Vector3i, IPoweredNode> IsPowered
            = new Dictionary<Vector3i, IPoweredNode>();

        internal bool TryGetConnection(Vector3i position, out PipeConnection connection)
            => Connections.TryFindValueAt(KdKey(position), out connection);

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
            Connections.Clear();
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
            // Connections.Clear();
            var version = br.ReadByte();
            int nodes = br.ReadInt32();
            Log.Out("Reading X noides {0}", nodes);
            for (int index = 0; index < nodes; ++index)
            {
                uint type = br.ReadUInt32();
                var node = InstantiateItem(type, br);
                if (node is IPoweredNode powered)
                {
                    IsPowered[node.WorldPos] = powered;
                    powered.IsPowered = br.ReadBoolean();
                }
            }

            // int wells = br.ReadInt32();
            // for (int index = 0; index < wells; ++index)
            // {
            //     AddWell(new PipeGridWell(br));
            // }
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
            if (Nodes.ContainsKey(position))
                Nodes.Remove(position);
            if (TryGetNode(position, out PipeConnection node))
                RemoveConnection(position, node);
            if (Irrigators.ContainsKey(KdKey(position)))
                RemoveIrrigation(position);
            if (IsPowered.ContainsKey(position))
                RemovePowered(position);
            if (Wells.ContainsKey(KdKey(position)))
                RemoveWell(position);
        }

        private bool RemovePowered(Vector3i position)
        {
            return IsPowered.Remove(position);
        }

        private void RemoveConnection(Vector3i position, PipeConnection node)
        {
            node.Grid = null; // Invoke `UpdateGrid`
            Connections.RemoveAt(KdKey(position));
        }

        private bool AddConnection(PipeConnection connection)
        {
            return Connections.Add(KdKey(connection.WorldPos), connection);
        }

        private void AddPowered(IPoweredNode powered)
        {
            IsPowered.Add(powered.WorldPos, powered);
        }



        private void AddIrrigation(PipeIrrigation irrigation)
        {
            Irrigators.Add(KdKey(irrigation.WorldPos), irrigation);
            // Search for existing wells in reach
            var results = Wells.RadialSearch(
                KdKey(irrigation.WorldPos), 20, NbWellCache);
            for (int i = 0; i < results.Length; i += 1)
                results[i].Value.AddIrrigation(irrigation);
        }

        private void RemoveIrrigation(Vector3i position)
        {
            Irrigators.RemoveAt(KdKey(position));
        }

        private static readonly NearestNeighbourList<KdTreeNode<int, PipeWell>, int> NbWellCache
            = new NearestNeighbourList<KdTreeNode<int, PipeWell>, int>(new KdTree.Math.IntegerMath());
        private static readonly NearestNeighbourList<KdTreeNode<int, PipeIrrigation>, int> NbIrrigatorCache
            = new NearestNeighbourList<KdTreeNode<int, PipeIrrigation>, int>(new KdTree.Math.IntegerMath());

        public PipeWell AddWell(PipeWell well)
        {
            Log.Warning("========= ADDWELL");
            Wells.Add(KdKey(well.WorldPos), well);
            // Search for output to fill up the well
            var results = Irrigators.RadialSearch(
                KdKey(well.WorldPos), 20, NbIrrigatorCache);
            for (int i = 0; i < results.Length; i += 1)
            {
                well.AddIrrigation(results[i].Value);
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
            if (Wells.TryFindValueAt(KdKey(position),
                out PipeWell well))
            {
                // Search for output to fill up the well
                var results = Irrigators.RadialSearch(
                KdKey(position), 20, NbIrrigatorCache);
                //for (int i = 0; i < results.Length; i += 1)
                //{
                //    well.RemoveIrrigation(results[i].Value);
                //}
                Wells.RemoveAt(KdKey(position));
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


        private static readonly NearestNeighbourList<KdTreeNode<int, PipeConnection>, int> NbCache
            = new NearestNeighbourList<KdTreeNode<int, PipeConnection>, int>(new KdTree.Math.IntegerMath());

        public byte GetNeighbours(Vector3i position, ref BlockConnector[] NB)
        {
            byte count = 0;
            NbCache.Clear();
            for (int i = 0; i < 6; i += 1) NB[i].Reset();
            if (NB.Length != 6) throw new ArgumentException("NB size mismatch");
            // ToDo: Can we optimize the math to have true radial and cubic searches?
            foreach (var node in Connections.RadialSearch(KdKey(position), 1, NbCache))
            {
                Vector3i delta = node.Value.WorldPos - position;
                if (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z != 1) continue;
                NB[FullRotation.VectorToSide(delta)].Set(node.Value);
                count += 1;
            }
            return count;
        }

    }

}
