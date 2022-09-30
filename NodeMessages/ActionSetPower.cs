namespace NodeManager
{
    public class ActionSetPower : IActionWorker
    {

        public int SenderEntityId { get => -1; set => throw new System.Exception("Can send stop signal from clients!"); }

        private Vector3i Position;
        private bool IsPowered;

        public void Setup(Vector3i position, bool powered)
        {
            Position = position;
            IsPowered = powered;
        }

        public void ProcessOnWorker(PipeGridWorker worker)
        {
            worker.Manager.UpdatePower(Position, IsPowered);
            Log.Out("Server got new powered {0} => {1}",
                Position, IsPowered);
        }

    }
}
