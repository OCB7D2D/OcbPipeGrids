namespace NodeManager
{

    public enum ConnectorFlag
    {
        None = 0,
        Cyclic = 1,
        Breaker = 2,
    }

    public struct BlockConnector
    {

        public bool IsCyclic => (Flags & (byte)ConnectorFlag.Cyclic) != 0;
        public bool IsBreaker => (Flags & (byte)ConnectorFlag.Breaker) != 0;

        public byte ConnectMask { get; private set; }

        public uint SideMask { get; private set; }
        public byte Distance { get; private set; }
        public byte Rotation { get; private set; }
        public byte Flags { get; private set; }
        public int Grid { get; private set; }
        public int BlockID { get; private set; }

        public IBlockConnection BLOCK;

        public void Reset()
        {
            ConnectMask = 255;
            SideMask = 0;
            Distance = 0;
            Rotation = 0;
            Flags = 0;
            Grid = -1;
        }

        public BlockConnector Set(BlockValue bv, IBlockConnection block)
        {
            BLOCK = block;
            BlockID = bv.type;
            ConnectMask = block.ConnectMask;
            SideMask = block.SideMask;
            Distance = 0;
            Rotation = bv.rotation;
            Flags = block.ConnectFlags;
            Grid = -1;
            return this;
        }

        public BlockConnector Set(PipeConnection connection)
        {
            // Should be safe to use block config?
            BLOCK = connection.BLOCK;
            BlockID = connection.BlockID;
            ConnectMask = connection.ConnectMask;
            SideMask = connection.SideMask;
            Distance = connection.CountLongestDistance();
            Rotation = connection.Rotation;
            Flags = (byte)connection.GetFlags();
            Grid = connection.Grid != null ?
                connection.Grid.ID : -1;
            return this;
        }

        public void Read(PooledBinaryReader br)
        {
            // ToDo: pack more tightly
            BlockID = br.ReadInt32();
            ConnectMask = br.ReadByte();
            SideMask = br.ReadUInt32();
            Distance = br.ReadByte();
            Rotation = br.ReadByte();
            Flags = br.ReadByte();
            Grid = br.ReadInt32();
            GetBlock(out BLOCK);
        }

        public void Write(PooledBinaryWriter bw)
        {
            bw.Write(BlockID);
            bw.Write(ConnectMask);
            bw.Write(SideMask);
            bw.Write(Distance);
            bw.Write(Rotation);
            bw.Write(Flags);
            bw.Write(Grid);
        }

        // Return block of given type (may return null)
        // We assume it is safe to access Blocks concurrently
        // Since these should never change once loaded on startup
        public bool GetBlock<T>(out T var) where T : class
            => (var = Block.list[BlockID] as T) != null;

    }

}
