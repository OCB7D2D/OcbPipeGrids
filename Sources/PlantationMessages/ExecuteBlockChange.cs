namespace NodeFacilitator
{

    //################################################################
    //################################################################

    public class ExecuteBlockChange : IActionMother
    {

        //********************************************************
        //********************************************************

        public Vector3i Position;
        public BlockValue BV;

        //********************************************************
        //********************************************************

        public int RecipientEntityId
        {
            // Only to be used to sync from mother to worker
            // Remote block changes are sent through vanilla!
            get => -1; set => throw new System.Exception(
                "Cannot send block change from clients!");
        }

        //********************************************************
        //********************************************************

        public void Setup(Vector3i pos, BlockValue bv)
        {
            Position = pos;
            BV = bv;
        }

        //********************************************************
        //********************************************************

        public void ProcessAtMother(NodeManagerInterface api)
        {
            NodeManager.ExecuteBlockChange(
                new BlockChangeInfo(Position,
                    BV, sbyte.MaxValue));
        }

        //********************************************************
        //********************************************************

    }

    //################################################################
    //################################################################

}
