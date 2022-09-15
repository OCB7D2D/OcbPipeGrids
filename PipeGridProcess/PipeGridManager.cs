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
        public string GetLoadInfo() => Connections.Count.ToString();
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
            return null;
        }

        //#####################################################################
        //#####################################################################

        public static int[] KdKey(Vector3i pos)
            => new int[] { pos.x, pos.y, pos.z };

        public List<PipeGrid> Grids = new List<PipeGrid>();

        // Use a KD-Tree to optimize finding nearest items
        // ToDo: benchmark exact lookup vs using a hash table
        // Note: pretty sure spatial queries will be faster though
        public readonly KdTree<int, PipeConnection> Connections = new KdTree<int,
            PipeConnection>(3, new KdTree.Math.IntCubeMath(), AddDuplicateBehavior.Update);

        internal bool TryGetNode(Vector3i position, out PipeConnection neighbour)
            => Connections.TryFindValueAt(KdKey(position), out neighbour);

        //#####################################################################
        //#####################################################################

        public void Cleanup()
        {
            // Persister.SaveSynchronous();
            foreach (var node in Connections)
                node.Value.Cleanup();
            Connections.Clear();
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
            Log.Out("{0} saving {1}",
                GetPersistName(),
                GetLoadInfo());
            bw.Write(Connections.Count);
            foreach (var kv in Connections)
            {
                // Make sure to write out the header first
                bw.Write(kv.Value.StorageID);
                // Write can be overridden
                kv.Value.Write(bw);
            }
            // bw.Write(Wells.Count);
            // foreach (var kv in Wells)
            // {
            //     // Write can be overridden
            //     kv.Value.Write(bw);
            // }
        }

        public virtual void Read(BinaryReader br)
        {
            // Connections.Clear();
            var version = br.ReadByte();
            int nodes = br.ReadInt32();
            for (int index = 0; index < nodes; ++index)
            {
                uint type = br.ReadUInt32();
                InstantiateItem(type, br);
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

        internal void RegisterConnection(PipeConnection connection)
            => Connections.Add(KdKey(connection.WorldPos), connection);

        public void UnregisterConnection(Vector3i position)
        {
            if (TryGetNode(position, out PipeConnection node))
            {
                node.Grid = null; // Invoke `UpdateGrid`
                Connections.RemoveAt(KdKey(position));
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
