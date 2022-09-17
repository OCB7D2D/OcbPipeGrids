namespace NodeFacilitator
{

    //########################################################
    // Action to update block values from mother to worker
    //########################################################

    public class ActionSetBlockValue : ActionBlockValue
    {
        public override void ProcessOnWorker(NodeManagerWorker worker)
        {
            if (worker.TryGetNode(Position, out NodeBlockBase node))
            {
                node.BV.rawData = BV.rawData;
            }
            else
            {
                Log.Warning("No Node found to Update Block Value at {0}", Position);
            }
        }
    }

    //########################################################
    //########################################################

}
