﻿namespace NodeFacilitator
{
    public class ActionUpdateLightLevel : IActionWorker
    {
        private Vector3i Position;
        public byte LightLevel = 0;
        public int SenderEntityId { get => -1; set => throw new System.Exception("Can send stop signal from clients!"); }

        public void Setup(Vector3i position, byte light)
        {
            Position = position;
            LightLevel = light;
        }

        public void ProcessOnWorker(NodeManagerWorker worker)
        {
            if (worker.TryGetNode(Position, out ISunLight node))
                node.CurrentSunLight = LightLevel;
        }
    }
}