using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{

    public abstract class NodeBase : ITickable, IfaceGridNodeManaged
    {

        public abstract uint StorageID { get; }

        public virtual ulong NextTick => 0;

        public Vector3i WorldPos { get; private set; }

        public int[] KdKey => new int[] { WorldPos.x, WorldPos.y, WorldPos.z };

        public NodeManager Manager { get; protected set; }

        protected NodeBase(Vector3i position, BlockValue bv)
        {
            WorldPos = position;
        }

        public NodeBase(BinaryReader br)
        {
            WorldPos = new Vector3i(
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32());
        }

        public virtual void Write(BinaryWriter bw)
        {
            bw.Write(WorldPos.x);
            bw.Write(WorldPos.y);
            bw.Write(WorldPos.z);
        }

        public NodeBase AttachToManager(NodeManager manager)
        {
            OnManagerAttached(manager);
            Manager = manager;
            return this;
        }

        public virtual void Tick(ulong delta)
        {
            // Log.Out("Tick Node");
        }

        public virtual void Cleanup()
        {

        }

        protected virtual void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            Log.Out("On Manager Attached {0}", manager);
            // Manager?.RemoveManagedNode(WorldPos);
            if (manager == null) return;
            manager.AddManagedNode(this);
            ulong tick = NextTick;
            if (tick == 0) return;
            manager.Schedule(tick, this);
        }

        public abstract string GetCustomDescription();

    }

}