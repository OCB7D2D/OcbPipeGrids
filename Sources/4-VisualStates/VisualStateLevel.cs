namespace NodeFacilitator
{

    //#####################################################
    // Basic interface for fill states from manager
    // Sent from worker thread to (interested) clients
    // Clients need to register interest for positions
    //#####################################################

    public struct FilledState : IVisualState
    {

        public int BID { get; set; }

        public Vector3i Position;
        public float FillState { get; set; }

        public void WriteState(PooledBinaryWriter bw)
        {
            bw.Write(BID);
            bw.Write(Position.x);
            bw.Write(Position.y);
            bw.Write(Position.z);
            bw.Write(FillState);
        }

        public void ReadState(PooledBinaryReader br)
        {
            BID = br.ReadInt32();
            Position.x = br.ReadInt32();
            Position.y = br.ReadInt32();
            Position.z = br.ReadInt32();
            FillState = br.ReadSingle();
        }

        public bool EqualsState(IVisualState other)
        {
            if (other is FilledState state)
            {
                return BID == state.BID
                    && Position == state.Position
                    && FillState == state.FillState;
            }
            return false;
        }

        public void UpdateVisualState(NodeManagerClient mother)
        {
            if (!(Block.list[BID] is IFilledBlock BLOCK)) return;
            BLOCK.UpdateVisualState(Position, FillState);
        }

    }
}
