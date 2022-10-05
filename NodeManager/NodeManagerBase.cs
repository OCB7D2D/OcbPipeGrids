// Process running in a separate thread
// Coded so it could also run on main thread
using KdTree3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NodeManager
{

    public partial class NodeManager : GlobalTicker, IPersistable
    {

        static internal float TimeScale(float delta) => delta / 60000f;

        public string GetPersistName() => "PipeGridManager";
        public string GetLoadInfo() => Nodes.Count.ToString();

        //#####################################################################
        // Class Factory for Pipe Node Specializations
        //#####################################################################

        public static readonly Dictionary<uint, Func<BinaryReader, NodeBase>>
            Factory = new Dictionary<uint, Func<BinaryReader, NodeBase>>();

        public static void RegisterFactory(uint id, Func<BinaryReader, NodeBase> creator)
        {
            Factory.Add(id, creator);
        }

        public NodeBase InstantiateItem(uint id, BinaryReader br)
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

        public readonly Dictionary<Vector3i, NodeBase> Nodes
            = new Dictionary<Vector3i, NodeBase>();

        // Use a KD-Tree to optimize finding nearest items
        // ToDo: benchmark exact lookup vs using a hash table
        // Note: pretty sure spatial queries will be faster though
        // Note: might not be true for a small radial searches
        //public readonly KdTree<int, PipeConnection> Connections = new KdTree<int,
        //    PipeConnection>(3, new KdTree.Math.IntCubeMath(), AddDuplicateBehavior.Update);

        //internal bool TryGetConnection(Vector3i position, out PipeConnection connection)
        //    => Connections.TryFindValueAt(KdKey(position), out connection);

        public bool TryGetNode(Vector3i position, out NodeBase node)
            => Nodes.TryGetValue(position, out node);

        public bool TryGetNode<T> (Vector3i position, out T node) where T : class
        {
            if (Nodes.TryGetValue(position,
                out NodeBase instance))
            {
                node = instance as T;
                return node != null;
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

        public ConcurrentQueue<IActionClient> ToMainThread;

        public NodeManager(ConcurrentQueue<IActionClient> output)
        {
            Persister = new PersistedData(this);
            ToMainThread = output;
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
                //Log.Out(" Reading type {0}", type);
                var node = InstantiateItem(type, br);
                // Restore power state automatically
                if (node is IPoweredNode powered)
                {
                    IsPowered[node.WorldPos] = powered;
                    powered.IsPowered = br.ReadBoolean();
                }
            }
            foreach (var node in Nodes)
                node.Value.OnAfterLoad();

        }

        //#####################################################################
        // Hooks for adding and removing pipe nodes
        //#####################################################################

        internal void AddManagedNode(NodeBase node)
        {
            Nodes.Add(node.WorldPos, node);
            // if (node is PipeConnection connection) AddConnection(connection);
            // if (node is PipeIrrigation irrigation) AddIrrigation(irrigation);
            if (node is IPoweredNode powered)
                AddPowered(powered);
            // if (node is PipeWell well) AddWell(well);
        }

        public void RemoveManagedNode(Vector3i position)
        {
            // Log.Out("1 Remove from pipe connection");
            if (TryGetNode(position, out var node))
            {
                node.AttachToManager(null);
                // Log.Out("2 Remove from pipe connection");
                // node.AddConnection
                // RemoveConnection(position, node);
            }
            //if (Irrigators.ContainsKey(position))
            //    RemoveIrrigation(position);
            if (IsPowered.ContainsKey(position))
                RemovePowered(position);
            //if (Wells.ContainsKey(position))
            //    RemoveWell(position);
            if (Nodes.ContainsKey(position))
                Nodes.Remove(position);
        }



        //#####################################################################
        // Hooks for pipe grids
        //#####################################################################



        // private static readonly NearestNeighbourList<KdTreeNode<int, PipeConnection>, int> NbCache
        //     = new NearestNeighbourList<KdTreeNode<int, PipeConnection>, int>(new KdTree.Math.IntegerMath());


    }

}
