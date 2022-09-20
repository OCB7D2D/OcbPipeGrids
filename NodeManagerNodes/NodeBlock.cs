using System.IO;

namespace NodeManager
{
    public abstract class NodeBlock<B> : NodeBase where B : class
    {

        public BlockValue BV = BlockValue.Air; // Air

        public int BlockID => BV.type;

        public byte Rotation => BV.rotation;

        public readonly B BLOCK;

        protected NodeBlock(BinaryReader br)
            : base(br)
        {
            BV.rawData = br.ReadUInt32();
            GetBlock(out BLOCK);
        }

        protected NodeBlock(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            BV.rawData = bv.rawData;
            GetBlock(out BLOCK);
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(BV.rawData);
        }

        // Return block of given type (may return null)
        // We assume it is safe to access Blocks concurrently
        // Since these should never change once loaded on startup
        public bool GetBlock<T>(out T var) where T : class
            => (var = Block.list[BlockID] as T) != null;

    }

}