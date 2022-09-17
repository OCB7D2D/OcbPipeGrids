namespace NodeFacilitator
{

    //########################################################
    // Action to be pushed from mother to worker
    // Use to e.g. sync main server thread stuff
    // Cheap enough to not need any batch update!?
    //########################################################

    public abstract class ActionPosition : IActionWorker
    {
        public int SenderEntityId { get; set; } = -1;
        public abstract void ProcessOnWorker(NodeManagerWorker worker);
        public Vector3i Position { get; private set; } = Vector3i.zero;
        public virtual void Setup(Vector3i position)
        {
            Position = new Vector3i(position);
        }
    }

    //########################################################
    // An action associated with a specific block type 
    // ToDo: indirection with `ActionPosition` is not needed
    //########################################################

    public abstract class ActionBlockNode : ActionPosition
    {
        protected ushort Type;
        public virtual void Setup(TYPES type, Vector3i position)
        {
            base.Setup(position);
            Type = (ushort)type;
        }
    }

    //########################################################
    // Even more specialzed base action with known block value
    // Used mainly when a block is first added and/or loaded
    //########################################################

    public abstract class ActionBlockValue : ActionBlockNode
    {
        protected BlockValue BV;
        public virtual void Setup(TYPES type, Vector3i pos, BlockValue bv)
        {
            base.Setup(type, pos);
            BV = new BlockValue {
                rawData = bv.rawData,
                // Not really used yet!?
                damage = bv.damage
            };
        }
    }

    //########################################################
    //########################################################

}
