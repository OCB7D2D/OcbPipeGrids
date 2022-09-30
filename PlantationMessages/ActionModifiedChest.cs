namespace NodeManager
{

    public class ActionModifyChest : IActionWorker
    {

        public int SenderEntityId { get => -1; set => throw new
            System.Exception("Can send stop signal from clients!"); }

        private ItemStack[] Stack;

        public void Setup(ItemStack[] stack)
        {
            Stack = stack;
        }

        public void ProcessOnWorker(PipeGridWorker worker)
        {
            Log.Warning("Got the change of the chest");
        }

    }

}
