namespace NodeManager
{
    public class ExecuteBlockChange : IActionClient
    {

        public int RecipientEntityId
        {
            get => -1; set => throw new
                System.Exception("Can send stop signal from clients!");
        }

        public Vector3i Position;
        public BlockValue BV;

        public void Setup(Vector3i pos, BlockValue bv)
        {
            Position = pos;
            BV = bv;

        }

        public void ProcessOnClient(NodeManagerClient client)
        {
            LateBlockChanges.AddPendingBlockChange(
                new BlockChangeInfo(Position, BV, sbyte.MaxValue));
        }

    }
}
