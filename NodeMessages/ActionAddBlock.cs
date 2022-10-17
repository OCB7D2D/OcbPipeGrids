using System;

namespace NodeManager
{
    /*
    public class ActionAddBlock : ActionAddBlock<NetPkgActionAddBlock>
    {
        // public override NodeBase CreateNode() => new PipeConnection(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddBlock pkg) => pkg.Setup(this);
    }

    public class ActionAddConnection : ActionAddBlock<NetPkgActionAddConnection>
    {
        // public override NodeBase CreateNode() => new PipeConnection(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddConnection pkg) => pkg.Setup(this);
    }

    public class ActionAddSource : ActionAddBlock<NetPkgActionAddSource>
    {
        // public override NodeBase CreateNode() => new PipeSource(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddSource pkg) => pkg.Setup(this);
    }

    public class ActionAddPump : ActionAddBlock<NetPkgActionAddPump>
    {
        // public override NodeBase CreateNode() => new PipePump(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPump pkg) => pkg.Setup(this);
    }

    public class ActionAddReservoir : ActionAddBlock<NetPkgActionAddReservoir>
    {
        // public override NodeBase CreateNode() => new PipeReservoir(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddReservoir pkg) => pkg.Setup(this);
    }

    public class ActionAddIrrigation : ActionAddBlock<NetPkgActionAddIrrigation>
    {
        // public override NodeBase CreateNode() => new PipeIrrigation(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddIrrigation pkg) => pkg.Setup(this);
    }

    public class ActionAddWell : ActionAddBlock<NetPkgActionAddWell>
    {
        // public override NodeBase CreateNode() => new PipeWell(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWell pkg) => pkg.Setup(this);
    }

    public class ActionAddFluidConverter : ActionAddBlock<NetPkgActionAddFluidConverter>
    {
        // public override NodeBase CreateNode() => new PipeFluidConverter(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFluidConverter pkg) => pkg.Setup(this);
    }

    public class ActionAddWaterBoiler : ActionAddBlock<NetPkgActionAddWaterBoiler>
    {
        // public override NodeBase CreateNode() => new PipeWaterBoiler(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWaterBoiler pkg) => pkg.Setup(this);
    }

    public class ActionAddFluidInjector : ActionAddBlock<NetPkgActionAddFluidInjector>
    {
        // public override NodeBase CreateNode() => new PipeFluidInjector(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFluidInjector pkg) => pkg.Setup(this);
    }
    */

    public abstract class BaseActionAddChest<N> : ActionAddMyBlock<N> where N : NetPackage
    {

        protected ItemStack[] Items
            = new ItemStack[0];

        // Specialized setup instructions
        protected override void Setup(Vector3i position, BlockValue bv)
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

    public class ActionRemoveBlock2 : BaseActionAddBlock<NetPkgWorkerAction<ActionRemoveBlock2>>, IRemoteQuery
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

        protected override void SetupNetPkg(NetPkgWorkerAction<ActionRemoveBlock2> pkg) => pkg.Setup(this);

    }

    public class ActionAddBlock2 : BaseActionAddBlock<NetPkgWorkerAction<ActionAddBlock2>>, IRemoteQuery
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

        protected override void SetupNetPkg(NetPkgWorkerAction<ActionAddBlock2> pkg) => pkg.Setup(this);

    }


    public abstract class ActionAddMyBlock<N> : BaseActionAddBlock<N> where N : NetPackage
    {

        public override void ProcessOnWorker(PipeGridWorker worker)
            => CreateNode().AttachToManager(worker.Manager);

        public abstract NodeBase CreateNode();

    }

}
