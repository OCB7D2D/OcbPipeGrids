using System.Collections.Concurrent;
using System.IO;

namespace PipeManager
{

    public abstract class PipeNode : ITickable, IfaceGridNodeManaged
    {

        public static uint FooBar = 1;

        public virtual uint StorageID => 0;

        public virtual ulong NextTick => 200;

        public byte Rotation { get; } = 0;

        public Vector3i WorldPos { get; private set; }

        public int[] KdKey => new int[] { WorldPos.x, WorldPos.y, WorldPos.z };

        public PipeGridManager Manager { get; protected set; }

        protected PipeNode(Vector3i position, byte rotation)
        {
            WorldPos = position;
            Rotation = rotation;
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

        public PipeNode AttachToManager(PipeGridManager manager)
        {
            Manager = manager;
            OnManagerAttached(manager);
            return this;
        }

        public virtual void Tick(ulong delta, ConcurrentQueue<IActionClient> output)
        {
            // Log.Out("Tick Node");
        }

        protected virtual void OnManagerAttached(PipeGridManager manager)
        {
            ulong tick = NextTick;
            if (tick == 0) return;
            manager.Schedule(tick, this);
        }

        public abstract string GetCustomDescription();

    }

}