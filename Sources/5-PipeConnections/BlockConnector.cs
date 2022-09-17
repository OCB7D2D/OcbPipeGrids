namespace NodeFacilitator
{

    public enum ConnectorFlag
    {
        None = 0,
        Cyclic = 1,
        Breaker = 2,
    }


    // Stuct used to pass connector info between worker and client

    public struct BlockConnector
    {

        // public bool IsCyclic => (Flags & (byte)ConnectorFlag.Cyclic) != 0;
        public bool IsBreaker => (Flags & (byte)ConnectorFlag.Breaker) != 0;

        public byte ConnectMask { get; private set; }

        public uint SideMask { get; private set; }
        public byte Rotation { get; private set; }
        public byte Flags { get; private set; }
        public int Grid { get; private set; }
        public int Pipes { get; private set; }
        public int BlockID { get; private set; }
        public ushort FluidType { get; private set; }

        public IBlockConnection BLOCK;

        public void Reset()
        {
            BLOCK = null;
            BlockID = 0;
            Rotation = 0;
            ConnectMask = 255;
            SideMask = 0;
            Flags = 0;
            Pipes = 0;
            Grid = -1;
            FluidType = 0;
        }

        public BlockConnector Set(BlockValue bv, IBlockConnection block)
        {
            BLOCK = block;
            BlockID = bv.type;
            Rotation = bv.rotation;
            ConnectMask = block.ConnectMask;
            SideMask = block.SideMask;
            Flags = block.ConnectFlags;
            Pipes = 0;
            Grid = -1;
            FluidType = 0;
            return this;
        }

        public BlockConnector Set(PipeConnection connection)
        {
            // Should be safe to use block config?
            BLOCK = connection.BLOCK;
            BlockID = connection.BlockID;
            Rotation = connection.Rotation;
            ConnectMask = connection.ConnectMask;
            SideMask = connection.SideMask;
            Flags = (byte)connection.GetFlags();
            Pipes = connection.CountSegmentPipes();
            Grid = connection.Grid != null ?
                connection.Grid.ID : -1;
            FluidType = connection.Grid != null ?
                connection.Grid.FluidType : (ushort)0;
            return this;
        }

        public void Read(PooledBinaryReader br)
        {
            // ToDo: pack more tightly
            BlockID = br.ReadInt32();
            Rotation = br.ReadByte();
            ConnectMask = br.ReadByte();
            SideMask = br.ReadUInt32();
            Flags = br.ReadByte();
            Pipes = br.ReadInt32();
            Grid = br.ReadInt32();
            FluidType = br.ReadUInt16();
            GetBlock(out BLOCK);
        }

        public void Write(PooledBinaryWriter bw)
        {
            bw.Write(BlockID);
            bw.Write(Rotation);
            bw.Write(ConnectMask);
            bw.Write(SideMask);
            bw.Write(Flags);
            bw.Write(Pipes);
            bw.Write(Grid);
            bw.Write(FluidType);
        }

        // Return block of given type (may return null)
        // We assume it is safe to access Blocks concurrently
        // Since these should never change once loaded on startup
        public bool GetBlock<T>(out T var) where T : class
            => (var = Block.list[BlockID] as T) != null;

    }

}
