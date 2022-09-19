namespace NodeManager
{
    public class MsgWaterLevelResponse : RemoteResponse<NetPkgWaterLevelResponse>
    {

        public float WaterLevel { get; set; }

        public override void ProcessOnClient(NodeManagerClient client)
        {
            World world = GameManager.Instance.World;
            BlockValue bv = world.GetBlock(Position);
            if (bv.Block is BlockPipeWell well)
                well.OnWaterLevelChanged(Position, WaterLevel);
            if (world.GetChunkFromWorldPos(Position) is Chunk chunk)
            {
                var action = new ActionUpdateLightLevel();
                action.Setup(Position, chunk.GetLight(
                    Position.x, Position.y, Position.z,
                    Chunk.LIGHT_TYPE.SUN));
                NodeManagerInterface.Instance.Input.Enqueue(action);
            }
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            WaterLevel = br.ReadSingle();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(WaterLevel);
        }

        protected override void SetupNetPkg(NetPkgWaterLevelResponse pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }
}
