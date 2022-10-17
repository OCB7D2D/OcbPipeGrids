using System;

namespace NodeManager
{
    public class ActionAddBlock : ActionAddBlock<NetPkgActionAddBlock>
    {
        // public override NodeBase CreateNode() => new PipeConnection(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddBlock pkg) => pkg.Setup(this);
    }

    public class ActionAddConnection : ActionAddMyBlock<NetPkgActionAddConnection>
    {
        public override NodeBase CreateNode() => new PipeConnection(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddConnection pkg) => pkg.Setup(this);
    }

    public class ActionAddSource : ActionAddMyBlock<NetPkgActionAddSource>
    {
        public override NodeBase CreateNode() => new PipeSource(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddSource pkg) => pkg.Setup(this);
    }

    public class ActionAddPump : ActionAddMyBlock<NetPkgActionAddPump>
    {
        public override NodeBase CreateNode() => new PipePump(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPump pkg) => pkg.Setup(this);
    }

    public class ActionAddReservoir : ActionAddMyBlock<NetPkgActionAddReservoir>
    {
        public override NodeBase CreateNode() => new PipeReservoir(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddReservoir pkg) => pkg.Setup(this);
    }

    public class ActionAddIrrigation : ActionAddMyBlock<NetPkgActionAddIrrigation>
    {
        public override NodeBase CreateNode() => new PipeIrrigation(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddIrrigation pkg) => pkg.Setup(this);
    }

    public class ActionAddWell : ActionAddMyBlock<NetPkgActionAddWell>
    {
        public override NodeBase CreateNode() => new PipeWell(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWell pkg) => pkg.Setup(this);
    }

    public class ActionAddFluidConverter : ActionAddMyBlock<NetPkgActionAddFluidConverter>
    {
        public override NodeBase CreateNode() => new PipeFluidConverter(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFluidConverter pkg) => pkg.Setup(this);
    }

    public class ActionAddWaterBoiler : ActionAddMyBlock<NetPkgActionAddWaterBoiler>
    {
        public override NodeBase CreateNode() => new PipeWaterBoiler(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWaterBoiler pkg) => pkg.Setup(this);
    }

    public class ActionAddFluidInjector : ActionAddMyBlock<NetPkgActionAddFluidInjector>
    {
        public override NodeBase CreateNode() => new PipeFluidInjector(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFluidInjector pkg) => pkg.Setup(this);
    }

    public abstract class BaseActionAddChest<N> : ActionAddMyBlock<N> where N : NetPackage
    {

        protected ItemStack[] Items
            = new ItemStack[0];

        // Specialized setup instructions
        public override void Setup(Vector3i position, BlockValue bv)
        {
            base.Setup(position, bv);
            World world = GameManager.Instance.World;
            if (world.GetTileEntity(0, position) is
                TileEntityLootContainer container)
                    Items = container.GetItems();
            Log.Out("Setup Items for loot {0}", Items.Length);
            Items = ItemStack.Clone(Items);
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Items = GameUtils.ReadItemStack(br);
            if (Items == null) return;
            Items = ItemStack.Clone(Items);
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            GameUtils.WriteItemStack(bw, Items);
        }

    }

    public abstract class BaseActionAddBlock<N> : RemoteQuery<N> where N : NetPackage
    {

        protected BlockValue BV;

        public virtual void Setup(Vector3i position, BlockValue bv)
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

    public abstract class ActionAddBlock<N> : BaseActionAddBlock<N> where N : NetPackage
    {
        protected ushort Type;

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

    }

    public abstract class ActionAddMyBlock<N> : BaseActionAddBlock<N> where N : NetPackage
    {

        public override void ProcessOnWorker(PipeGridWorker worker)
            => CreateNode().AttachToManager(worker.Manager);

        public abstract NodeBase CreateNode();

    }

}
