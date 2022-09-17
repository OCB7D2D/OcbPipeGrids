namespace NodeFacilitator
{
    public interface INodeReset
    {
        void Reset();
    }

    public class MsgNodeReset : ServerNodeQuery<NetPkgNodeReset>
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
        {
            // There is no safety check if sender is allowed to do this
            // ToDo: add some checks to only allow owner/ally to do that
            if (worker.TryGetNode(Position, out INodeReset node)) node.Reset();
            else Log.Warning("Node to reset not found at {0}", Position);
        }

        protected override void SetupNetPkg(NetPkgNodeReset pkg) => pkg.FromMsg(this);

        public override int GetLength() => 42;

    }

    // Wrapper for network package when transfering over the wire
    public class NetPkgNodeReset : NetPkgWorkerQuery<MsgNodeReset> { }

}
