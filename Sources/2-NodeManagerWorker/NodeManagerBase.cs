using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NodeFacilitator
{

    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        static internal float TimeScale(float delta) => delta / 60000f;

        public string GetPersistName() => "PipeGridManager";
        public string GetLoadInfo() => Nodes.Count.ToString();

        //########################################################
        // Class Factory for Pipe Node Specializations
        //########################################################

        public static readonly Dictionary<uint, Func<BinaryReader, NodeBase>>
            StorageFactory = new Dictionary<uint, Func<BinaryReader, NodeBase>>();
        public static readonly Dictionary<uint, Func<Vector3i, BlockValue, NodeBase>>
            CreatorFactory = new Dictionary<uint, Func<Vector3i, BlockValue, NodeBase>>();

        public static void RegisterFactory(uint id,
            Func<BinaryReader, NodeBase> stored,
            Func<Vector3i, BlockValue, NodeBase> created)
        {
            StorageFactory.Add(id, stored);
            CreatorFactory.Add(id, created);
        }

        public NodeBase InstantiateItem(ushort id, Vector3i pos, BlockValue bv)
        {
            if (CreatorFactory.TryGetValue(id, out var ctor))
            {
                var node = ctor(pos, bv);
                node.AttachToManager(this);
                return node;
            }
            Log.Error("Could not instantiate unknown PipeGrid Type {0}", id);
            return null;
        }

        public NodeBase InstantiateItem(uint id, BinaryReader br)
        {
            if (StorageFactory.TryGetValue(id, out var ctor))
            {
                var node = ctor(br);
                node.AttachToManager(this);
                return node;
            }
            Log.Error("Could not instantiate unknown PipeGrid Type {0}", id);
            return null;
        }

        //########################################################
        //########################################################

        public readonly Dictionary<Vector3i, NodeBase> Nodes
            = new Dictionary<Vector3i, NodeBase>();

        public T TryGetNode<T>(Vector3i position) where T : class
            => TryGetNode(position, out T node) ? node : null;

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

        //########################################################
        //########################################################

        public void Cleanup()
        {
            // Persister.SaveSynchronous();
            foreach (var kv in Nodes)
                kv.Value.Cleanup();
            // Clear all containers ...
            VisualStateListeners.Clear();
            Composters.Clear();
            FarmLands.Clear();
            FarmPlots.Clear();
            Irrigators.Clear();
            GrowLights.Clear();
            Sprinklers.Clear();
            Wells.Clear();
            Nodes.Clear();
            Grids.Clear();
        }

        //########################################################
        // Persistence interface and implementations
        //########################################################

        private static readonly byte FileVersion = 1;

        private readonly PersistedData Persister;

        protected BlockingCollection<IActionMother> ToMother;
        protected BlockingCollection<IMessageClient> ToClients;

        public void PushToMother(IActionMother response)
        {
            ToMother.Add(response);
        }

        public void SendToClient(IMessageClient response)
        {
            ToClients.Add(response);
        }

        public NodeManager(
            BlockingCollection<IActionMother> output,
            BlockingCollection<IMessageClient> clients)
        {
            Persister = new PersistedData(this);
            ToMother = output;
            ToClients = clients;
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
            WriteChests(bw);
            WritePendingChunkChanges(bw);
        }

        public virtual void Read(BinaryReader br)
        {
            var version = br.ReadByte();
            int nodes = br.ReadInt32();
            for (int index = 0; index < nodes; ++index)
            {
                uint type = br.ReadUInt32();
                var node = InstantiateItem(type, br);
                // Restore power state automatically
                if (node is IPoweredNode powered)
                {
                    PowerState[node.WorldPos] = powered;
                    powered.IsPowered = br.ReadBoolean();
                }
            }
            ReadChests(br);
            ReadPendingChunkChanges(br);
            // Call late init once all are attached
            // Useful since other items are loaded now
            foreach (var node in Nodes)
                node.Value.OnAfterLoad();

        }

        //########################################################
        // Hooks for adding and removing pipe nodes
        //########################################################

        internal void AddManagedNode(NodeBase node)
        {
            if (Nodes.TryGetValue(node.WorldPos, out var old))
            {
                if (old.StorageID == node.StorageID)
                    Log.Warning("Overwrite existing node at {0}", node.WorldPos);
                else Log.Warning("Overwrite node type at {0} ({1} with {2})",
                    node.WorldPos, old.StorageID, node.StorageID);
                Nodes[node.WorldPos] = node;
            }
            else
            {
                Nodes.Add(node.WorldPos, node);
            }
            // Log.Out("Add powered {0}", node.WorldPos);
            if (node is IPoweredNode powered)
                AddPowered(powered);
        }

        public void RemoveManagedNode(Vector3i position)
        {
            if (TryGetNode(position, out var node))
                node.AttachToManager(null);
            if (PowerState.ContainsKey(position))
                RemovePowered(position);
            if (Nodes.ContainsKey(position))
                Nodes.Remove(position);
        }

        //########################################################
        // Hook to update the block value at manager
        // E.g. called when meta flags change via client
        //########################################################

        public void UpdateBlockValue(Vector3i position, BlockValue bv)
        {
            if (TryGetNode<NodeBlockBase>(position, out var node)) node.BV = bv;
        }

        //########################################################
        //########################################################

    }

}
