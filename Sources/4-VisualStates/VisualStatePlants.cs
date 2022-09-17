namespace NodeFacilitator
{

    //#####################################################
    // Basic interface for plant states from manager
    // Sent from worker thread to (interested) clients
    // Clients need to register interest for positions
    //#####################################################

    public interface IPlantProgress
    {
        int BID { get; }
        float Progress { get; }
    }

    //#####################################################
    // Implementation without position, used to store
    // progress information at each client with minimal
    // memory overhead (omitting the position vector).
    //#####################################################

    public struct PlantProgress : IPlantProgress
    {
        public int BID { get; set; }
        public float Progress { get; set; }
        public PlantProgress(int bid, float progress)
            => (BID, Progress) = (bid, progress);
        public PlantProgress(IPlantProgress state)
            : this(state.BID, state.Progress) { }
    }

    //#####################################################
    // Message that is sent from worker to clients to
    // inform them about plant growth and other events.
    //#####################################################

    public struct MsgPlantProgress : IPlantProgress, IVisualState
    {

        public int BID { get; private set; }
        public float Progress { get; private set; }

        public Vector3i Position;

        public MsgPlantProgress(int bid, Vector3i pos, float progress) : this()
            => (BID, Position, Progress) = (bid, pos, progress);


        public void WriteState(PooledBinaryWriter bw)
        {
            bw.Write(BID);
            bw.Write(Position.x);
            bw.Write(Position.y);
            bw.Write(Position.z);
            bw.Write(Progress);
        }

        public void ReadState(PooledBinaryReader br)
        {
            BID = br.ReadInt32();
            Position.x = br.ReadInt32();
            Position.y = br.ReadInt32();
            Position.z = br.ReadInt32();
            Progress = br.ReadSingle();
        }

        public bool EqualsState(IVisualState other)
        {
            if (other is MsgPlantProgress state)
            {
                return BID == state.BID
                    && Position == state.Position
                    && Progress == state.Progress;
            }
            return false;
        }

        public void UpdateVisualState(NodeManagerClient mother)
            => NodeManagerInterface.Instance.Client.SetPlantState(Position, this);

    }
}
