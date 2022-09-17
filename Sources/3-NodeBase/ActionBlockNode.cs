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

}
