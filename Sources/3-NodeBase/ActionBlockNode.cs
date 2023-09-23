namespace NodeFacilitator
{

    public class ActionAddBlock : ActionBlockValue
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
            => worker.InstantiateItem(Type, Position, BV);
    }

    public class ActionRemoveBlock : ActionBlockNode
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
            => worker.RemoveManagedNode(Position);
    }

    public class ActionBlockValueChanged : ActionBlockValue
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
            => worker.UpdateBlockValue(Position, BV);
    }

}
