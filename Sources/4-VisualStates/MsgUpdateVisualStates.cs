using System.Collections.Generic;

namespace NodeFacilitator
{

    // Send visual states from worker thread to clients (either local or remote)
    // We do this in a batched fashion, so one package contains multiple/all updates
    public class MsgUpdateVisualStates : ServerResponse<NetPkgUpdateVisualStates>
    {

        // Main function that is invoked once the message reaches the client
        // It is guaranteed that this will only be called on the real client
        public override void ProcessAtClient(NodeManagerClient client)
        {
            foreach (IVisualState state in States)
                state.UpdateVisualState(client);
        }

        // All states attached to the message
        // This will actually cross thread border
        private readonly HashSet<IVisualState> States
            = new HashSet<IVisualState>();

        // Return if sending of message makes any sense
        // Might be true if Setup has only stale states
        public bool HasPayLoad() => States.Count != 0;

        // Setup the message from existing state nodes
        // Invoked on the worker thread before sending
        public void Setup(ICollection<VisualStateListener> nodes)
        {
            foreach (VisualStateListener node in nodes)
            {
                var state = node.GetState();
                if (state == null) continue;
                // Check if state has changed, skip otherwise
                if (!state.EqualsState(node.LastVisualState))
                {
                    node.LastVisualState = state;
                    States.Add(node.LastVisualState);
                }
            }
        }

        // Read function in order to pass via network
        public override void Read(PooledBinaryReader br)
        {
            States.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var id = br.ReadInt32();
                var blk = Block.list[id];
                if (blk is IStateBlock block)
                {
                    IVisualState state = block
                        .CreateEmptyVisualState();
                    state.ReadState(br);
                    States.Add(state);
                }
            }
        }

        // Write function in order to pass via network
        public override void Write(PooledBinaryWriter bw)
        {
            bw.Write(States.Count);
            foreach (IVisualState item in States)
            {
                bw.Write(item.BID);
                var id = item.BID;
                var blk = Block.list[id];
                if (blk is IStateBlock)
                    item.WriteState(bw);
            }
        }

        // Implementation for the message over network interface
        protected override void SetupNetPkg(NetPkgUpdateVisualStates pkg) => pkg.FromMsg(this);

        // Note: this doesn't really seem to do much
        public override int GetLength() => 42;

    }
}
