using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{

    public abstract class PipeNode : ITickable, IfaceGridNodeManaged
    {

        public abstract uint StorageID { get; }

        public virtual ulong NextTick => 200;

        public Vector3i WorldPos { get; private set; }

        public int[] KdKey => new int[] { WorldPos.x, WorldPos.y, WorldPos.z };

        public NodeManager Manager { get; protected set; }

        protected PipeNode(Vector3i position, BlockValue bv)
        {
            WorldPos = position;
        }

        public PipeNode(BinaryReader br)
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

        public PipeNode AttachToManager(NodeManager manager)
        {
            Manager = manager;
            OnManagerAttached(manager);
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
            Log.Out("On Manager Attached");
            manager.AddPipeGridNode(this);
            ulong tick = NextTick;
            if (tick == 0) return;
            manager.Schedule(tick, this);
        }

        public abstract string GetCustomDescription();

    }

}