namespace NodeManager
{
    public class ActionUpdateLightLevel : IActionServer
    {
        private Vector3i Position;
        public byte LightLevel = 0;
        public int SenderEntityId { get => -1; set => throw new System.Exception("Can send stop signal from clients!"); }

        public void Setup(Vector3i position, byte light)
        {
            Position = position;
            LightLevel = light;
        }

        public void ProcessOnServer(NodeManagerWorker worker)
        {
            if (worker.Manager.TryGetNode(Position, out ISunLight node))
                node.CurrentSunLight = LightLevel;
        }
    }
}
