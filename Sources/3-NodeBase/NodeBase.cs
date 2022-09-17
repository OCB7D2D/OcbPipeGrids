using System.Collections.Generic;
using System.IO;

namespace NodeFacilitator
{

    public abstract class NodeBase : ITickable, IfaceGridNodeManaged, IEqualityComparer<IWorldPos>
    {

        public abstract uint StorageID { get; }

        public virtual bool RequiresBiome => false;
        public virtual ulong NextTick => 0;

        public Vector3i WorldPos { get; private set; }

        public int[] KdKey => new int[] { WorldPos.x, WorldPos.y, WorldPos.z };

        public NodeManager Manager { get; protected set; }

        public ScheduledTick Scheduled { get; set; }

        public byte BiomeID { get; protected set; } = 255;

        protected NodeBase(Vector3i position)
        {
            WorldPos = position;
            if (RequiresBiome == false) return;
            BiomeID = NodeManagerInterface.Instance
                .BiomeMap.GetBiomeID(WorldPos);
            Log.Out("Created with Biome {0}", BiomeID);
        }

        public NodeBase(BinaryReader br)
        {
            WorldPos = new Vector3i(
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32());
            Log.Out("=> Loaded {0} from disc", WorldPos);
            if (RequiresBiome == false) return;
            BiomeID = NodeManagerInterface.Instance
                .BiomeMap.GetBiomeID(WorldPos);
            Log.Out("=> Loaded {1} from disc with Biome {0}", BiomeID, WorldPos);
        }

        public virtual void Write(BinaryWriter bw)
        {
            bw.Write(WorldPos.x);
            bw.Write(WorldPos.y);
            bw.Write(WorldPos.z);
        }

        public virtual void OnAfterLoad()
        {

        }

        public NodeBase AttachToManager(NodeManager manager)
        {
            OnManagerAttached(manager);
            Manager = manager;
            return this;
        }

        public virtual bool Tick(ulong delta)
        {
            return Manager != null;
        }

        public virtual void Cleanup()
        {

        }

        protected virtual void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            // Log.Out("On Manager Attached {0}", manager);
            // Manager?.RemoveManagedNode(WorldPos);
            if (manager != null)
            {
                manager.AddManagedNode(this);
                ulong tick = NextTick;
                if (tick != 0) Scheduled =
                    manager.Schedule(tick, this);
            }
            else if (Scheduled != null)
            {
                Manager?.Unschedule(Scheduled);
            }
        }

        public abstract string GetCustomDescription();

        public bool Equals(IWorldPos x, IWorldPos y)
        {
            return x.WorldPos.Equals(y.WorldPos);
        }

        public int GetHashCode(IWorldPos obj)
        {
            return obj.WorldPos.GetHashCode();
        }
    }

}