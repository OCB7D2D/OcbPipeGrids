using System;

namespace NodeManager
{

    public abstract class BaseActionBlock<N> : RemoteQuery<N> where N : NetPackage
    {

        protected BlockValue BV;

        protected virtual void Setup(Vector3i position, BlockValue bv)
        {
            base.Setup(position);
            BV = bv;
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            BV.rawData = br.ReadUInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(BV.rawData);
        }

        public override int GetLength() => 42;

    }

    public class ActionRemoveBlock : BaseActionBlock<NetPkgActionRemoveBlock>, IRemoteQuery
    {
        protected ushort Type;

        internal void Setup(TYPES type, Vector3i pos)
        {
            Type = (ushort)type;
            Setup(pos);
        }

        public virtual void SetStorageID(TYPES type)
        {
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

        public override void ProcessOnWorker(PipeGridWorker worker)
            => worker.Manager.RemoveManagedNode(Position);

        protected override void SetupNetPkg(NetPkgActionRemoveBlock pkg) => pkg.Setup(this);

    }

    public class ActionAddBlock : BaseActionBlock<NetPkgActionAddBlock>, IRemoteQuery
    {
        protected ushort Type;

        public virtual void Setup(TYPES type, Vector3i pos, BlockValue bv)
        {
            Type = (ushort)type;
            Setup(pos, bv);
        }

        public virtual void SetStorageID(TYPES type)
        {
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

        public override void ProcessOnWorker(PipeGridWorker worker)
            => worker.Manager.InstantiateItem(Type, Position, BV);

        protected override void SetupNetPkg(NetPkgActionAddBlock pkg) => pkg.Setup(this);

    }

}
