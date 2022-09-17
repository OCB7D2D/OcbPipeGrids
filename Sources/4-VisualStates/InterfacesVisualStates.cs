namespace NodeFacilitator
{

    // Interface for block supporting visual states
    public interface IStateBlock : IBlockNode
    {

        // Required to specialize data-stream read
        // Called to create specialized empty instance
        // Then calls interface `ReadState` to consume data
        IVisualState CreateEmptyVisualState();

    }

    // Interface for nodes providing the visual states to `IStateBlock`
    public interface IStateNode : IManagerNode
    {

        // Returns the actual state that can than be
        // passed (or sent over network) to clients.
        IVisualState GetState();

    }

    // State to cross thread barrier
    public interface IVisualState
    {

        // Block to determine specialization
        // This must be a `IStateBlock` to work
        int BID { get; }

        // Read state from network stream
        void ReadState(PooledBinaryReader br);

        // Write state to network stream
        void WriteState(PooledBinaryWriter bw);

        // Check if state has changed
        bool EqualsState(IVisualState other);
        
        // Update the visual state once recieved at the client
        void UpdateVisualState(NodeManagerClient client);
    }

}
