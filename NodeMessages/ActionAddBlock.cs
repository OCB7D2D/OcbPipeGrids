namespace NodeManager
{
    public class ActionAddConnection : BaseActionAddBlock<NetPkgActionAddConnection>
    {
        public override NodeBase CreateNode() => new PipeConnection(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddConnection pkg) => pkg.Setup(this);
    }

    public class ActionAddSource : BaseActionAddBlock<NetPkgActionAddSource>
    {
        public override NodeBase CreateNode() => new PipeSource(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddSource pkg) => pkg.Setup(this);
    }

    public class ActionAddPump : BaseActionAddBlock<NetPkgActionAddPump>
    {
        public override NodeBase CreateNode() => new PipePump(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddPump pkg) => pkg.Setup(this);
    }

    public class ActionAddReservoir : BaseActionAddBlock<NetPkgActionAddReservoir>
    {
        public override NodeBase CreateNode() => new PipeReservoir(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddReservoir pkg) => pkg.Setup(this);
    }

    public class ActionAddIrrigation : BaseActionAddBlock<NetPkgActionAddIrrigation>
    {
        public override NodeBase CreateNode() => new PipeIrrigation(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddIrrigation pkg) => pkg.Setup(this);
    }

    public class ActionAddWell : BaseActionAddBlock<NetPkgActionAddWell>
    {
        public override NodeBase CreateNode() => new PipeWell(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWell pkg) => pkg.Setup(this);
    }

    public class ActionAddFluidConverter : BaseActionAddBlock<NetPkgActionAddFluidConverter>
    {
        public override NodeBase CreateNode() => new PipeFluidConverter(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddFluidConverter pkg) => pkg.Setup(this);
    }

    public class ActionAddWaterBoiler : BaseActionAddBlock<NetPkgActionAddWaterBoiler>
    {
        public override NodeBase CreateNode() => new PipeWaterBoiler(Position, BV);
        protected override void SetupNetPkg(NetPkgActionAddWaterBoiler pkg) => pkg.Setup(this);
    }

    public abstract class BaseActionAddChest<N> : BaseActionAddBlock<N> where N : NetPackage
    {

        protected ItemStack[] Items
            = new ItemStack[0];

        // Specialized setup instructions
        public override void Setup(Vector3i position, BlockValue bv)
        {
            base.Setup(position);
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

        public abstract NodeBase CreateNode();

        public override void ProcessOnWorker(PipeGridWorker worker)
            => CreateNode().AttachToManager(worker.Manager);

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
}
