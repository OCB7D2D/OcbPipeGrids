namespace NodeFacilitator
{

    public abstract class RemoteBlockNode<N> : ServerNodeQuery<N> where N : NetPackage
    {
        protected ushort Type;

        public virtual void Setup(TYPES type, Vector3i pos)
        {
            base.Setup(pos);
            Type = (ushort)type;
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Type = br.ReadUInt16();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Type);
        }
    }

    public abstract class RemoteBlockValue<N> : RemoteBlockNode<N> where N : NetPackage
    {

        protected BlockValue BV;

        public virtual void Setup(TYPES type, Vector3i pos, BlockValue bv)
        {
            base.Setup(type, pos);
            BV = bv;
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            BV.rawData = br.ReadUInt32();
            BV.damage = br.ReadInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(BV.rawData);
            bw.Write(BV.damage);
        }

        public override int GetLength() => 42;

    }

}