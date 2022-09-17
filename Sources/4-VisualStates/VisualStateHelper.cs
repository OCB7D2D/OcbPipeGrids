namespace NodeFacilitator
{
    static class VisualStateHelper
    {

        // Call this on all `IStateBlock` when added or loaded
        public static void OnBlockVisible(IStateBlock block, Vector3i pos, BlockValue bv)
        {
            if (bv.isair || bv.ischild) return;
            if (GameManager.IsDedicatedServer) return;
            var action = new ActionAddVisualState();
            action.Setup(block.NodeType, pos, bv);
            NodeManagerInterface.SendToWorker(action);
        }

        // Call this on all `IStateBlock` when removed or unloaded
        public static void OnBlockInvisible(IStateBlock block, Vector3i pos, BlockValue bv)
        {
            if (bv.isair || bv.ischild) return;
            if (GameManager.IsDedicatedServer) return;
            var action = new ActionRemoveVisualState();
            action.Setup(block.NodeType, pos, bv);
            NodeManagerInterface.SendToWorker(action);
        }

    }
}
