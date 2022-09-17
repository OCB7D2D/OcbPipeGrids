namespace NodeFacilitator
{
    public class ActionSetPower : IActionWorker
    {

        public int SenderEntityId { get => -1; set => throw new System.
            Exception("Can't send Power State from remote clients!"); }

        private Vector3i Position;
        private bool IsPowered;

        public void Setup(Vector3i position, bool powered)
        {
            Position = new Vector3i(position);
            IsPowered = powered;
        }

        public void ProcessOnWorker(NodeManagerWorker worker)
        {
            worker.UpdatePower(Position, IsPowered);
        }

    }
}
