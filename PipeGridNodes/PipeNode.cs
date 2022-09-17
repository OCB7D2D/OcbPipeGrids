using System.Collections.Concurrent;
using System.IO;

namespace PipeManager
{

    public abstract class PipeNode : ITickable, IfaceGridNodeManaged
    {

        public BlockValue BV = BlockValue.Air; // Air

        public virtual uint StorageID => 0;

        public virtual ulong NextTick => 200;

        public Vector3i WorldPos { get; private set; }

        public int[] KdKey => new int[] { WorldPos.x, WorldPos.y, WorldPos.z };

        public int BlockID => BV.type;

        public byte Rotation => BV.rotation;

        public PipeGridManager Manager { get; protected set; }

        protected PipeNode(Vector3i position, BlockValue bv)
        {
            BV = bv;
            WorldPos = position;
        }

        public PipeNode(BinaryReader br)
        {
            WorldPos = new Vector3i(
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32());
            BV.rawData = br.ReadUInt32();
        }

        public virtual void Write(BinaryWriter bw)
        {
            bw.Write(WorldPos.x);
            bw.Write(WorldPos.y);
            bw.Write(WorldPos.z);
            bw.Write(BV.rawData);
        }

        // Base method to get base block instance
        // We assume it is safe to access Blocks concurrently
        // Since these should never change once loaded on startup
        // public Block GetBlock() => Block.list[BlockID];

        // Return block of given type (may return null)
        public bool GetBlock<T>(out T var) where T : class
            => (var = Block.list[BlockID] as T) != null;

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