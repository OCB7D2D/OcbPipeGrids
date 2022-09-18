namespace PipeManager
{
    public class MsgWaterExchangeResponse : RemoteResponse<NetPkgWaterExchangeResponse>
    {

        public int Exchanged { get; set; }
        public float WaterLevel { get; set; }
        public int InventorySlot { get; set; }
        public int HoldingCount { get; set; }
        public int OldItemType { get; set; }
        public int NewItemType { get; set; }

        public ItemClass NewItem => ItemClass.GetForId(NewItemType);
        public ItemValue NewItemValue => new ItemValue(NewItemType);
        public ItemValue OldItemValue => new ItemValue(OldItemType);

        public override void ProcessOnClient(PipeGridClient client)
        {
            World world = GameManager.Instance.World;
            BlockValue bv = world.GetBlock(Position);
            if (bv.Block is BlockPipeWell well)
                well.OnWaterLevelChanged(Position, WaterLevel);
            var entity = world.GetEntity(RecipientEntityId);
            Log.Out("Got response from exchange {0} {1} {2}",
                Exchanged, RecipientEntityId, entity);

            EntityPlayerLocal pp = world.GetPrimaryPlayer();

            // Check if still holding the items in the same slot
            ItemStack holding = pp.inventory.GetItem(InventorySlot);
            Log.Out("===> Check {0} {1}", OldItemType, NewItemType);
            if (holding.itemValue.type == OldItemType)
            {
                Log.Out("Slot has still the same holding item");
                if (Exchanged >= HoldingCount)
                {
                    Log.Warning("Water exchange count too big?");
                }
                if (holding.count == HoldingCount && Exchanged >= HoldingCount)
                {
                    Log.Out("Still holding the same amount");
                    pp.inventory.SetItem(InventorySlot,
                        new ItemStack(NewItemValue, HoldingCount));
                }
                else
                {
                    holding.count -= Exchanged;
                    pp.inventory.SetItem(InventorySlot, holding);
                    var stack = new ItemStack(NewItemValue, Exchanged);
                    if (stack.count > 0) pp.bag.TryStackItem(0, stack);
                    if (stack.count > 0) pp.inventory.TryStackItem(0, stack);
                    if (stack.count > 0 && pp.bag.AddItem(stack)) stack.count = 0;
                    if (stack.count > 0 && pp.inventory.AddItem(stack)) stack.count = 0;
                    if (stack.count > 0) GameManager.Instance.ItemDropServer(
                        stack, Position + Vector3i.up, Vector3i.zero);
                }
            }
            else
            {
                // ToDo: implement alternative for late replies
                // We can still check existing inventory to do
                // the exchange. Otherwise, water is lost, or
                // we could send it back to the server again.
                Log.Warning("Water exchange failed, water wasted");
            }
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Exchanged = br.ReadInt32();
            WaterLevel = br.ReadSingle();
            InventorySlot = br.ReadInt32();
            HoldingCount = br.ReadInt32();
            OldItemType = br.ReadInt32();
            NewItemType = br.ReadInt32();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(Exchanged);
            bw.Write(WaterLevel);
            bw.Write(InventorySlot);
            bw.Write(HoldingCount);
            bw.Write(OldItemType);
            bw.Write(NewItemType);
        }

        protected override void SetupNetPkg(NetPkgWaterExchangeResponse pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;
    }
}
