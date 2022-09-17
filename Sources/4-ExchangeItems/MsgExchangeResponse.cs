namespace NodeFacilitator
{

    public class NetPkgRemoteExchangeResponse : NetPkgWorkerAnswer<MsgExchangeResponse> { }

    public class MsgExchangeResponse : ServerNodeResponse<NetPkgRemoteExchangeResponse>
    {

        public int ItemType { get; private set; }
        public int ItemCount { get; private set; }

        public override void ProcessAtClient(NodeManagerClient client)
        {
            World world = GameManager.Instance.World;
            EntityPlayerLocal pp = world.GetPrimaryPlayer();
            var action = pp.inventory.holdingItemData.item.Actions[0];
            if (action is ItemActionExchangeInteraction interaction)
                interaction.ProcessServerResponse(this);
        }

        public void Setup(int type, int count)
        {
            ItemType = type;
            ItemCount = count;
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            ItemType = br.ReadInt32();
            ItemCount = br.ReadInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(ItemType);
            bw.Write(ItemCount);
        }

        protected override void SetupNetPkg(NetPkgRemoteExchangeResponse pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }

}
