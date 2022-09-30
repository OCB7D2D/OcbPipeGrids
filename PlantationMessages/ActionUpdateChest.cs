namespace NodeManager
{

    public class ActionUpdateChest : IActionWorker
    {

        public int SenderEntityId { get => -1; set => throw new
            System.Exception("Can send stop signal from clients!"); }

        private Vector3i Position;
        private ItemStack[] Stack;

        public void Setup(Vector3i pos, ItemStack[] stack)
        {
            Position = pos;
            if (stack == null) Stack = new ItemStack[0];
            else Stack = ItemStack.Clone(stack);
        }

        public void ProcessOnWorker(PipeGridWorker worker)
        {
            worker.Manager.UpdateChest(Position, Stack);
            Log.Warning("Got the change of the chest");
        }

    }

}
