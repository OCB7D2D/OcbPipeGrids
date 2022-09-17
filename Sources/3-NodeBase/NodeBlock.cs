using System.IO;

namespace NodeFacilitator
{

    public abstract class NodeBlockBase : NodeBase
    {

        static ulong IDs = 0;

        public ulong ID = IDs++;

        public BlockValue BV = BlockValue.Air; // Air

        public int BlockID => BV.type;

        public byte Rotation => BV.rotation;

        protected NodeBlockBase(BinaryReader br)
            : base(br)
        {
            BV.rawData = br.ReadUInt32();
        }

        protected NodeBlockBase(Vector3i position, BlockValue bv)
            : base(position)
        {
            BV.rawData = bv.rawData;
        }
        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(BV.rawData);
        }

    }

    public abstract class NodeBlock<B> : NodeBlockBase, IBlockValue where B : class, IBlockNode
    {

        public readonly B BLOCK;

        public IBlockNode IBLK => BLOCK;

        BlockValue IBlockValue.BV => BV;

        public virtual void ParseBlockConfig() { }

        protected NodeBlock(BinaryReader br)
            : base(br)
        {
            GetBlock(out BLOCK);
            ParseBlockConfig();
        }

        protected NodeBlock(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            GetBlock(out BLOCK);
            ParseBlockConfig();
        }

        // Return block of given type (may return null)
        // We assume it is safe to access Blocks concurrently
        // Since these should never change once loaded on startup
        public bool GetBlock<T>(out T var) where T : class
            => (var = Block.list[BlockID] as T) != null;

    }

}