using System.IO;

namespace NodeFacilitator
{
    public class PipeSource : PipePump, IPoweredNode
    {
        public static new TYPES NodeType = TYPES.PipeSource;
        public override uint StorageID => (uint)NodeType;

        public override ulong NextTick => 10;

        public PipeSource(Vector3i position, BlockValue bv) : base(position, bv)
        {
            Log.Out("Created Pump");
            SetFluidType(1);
        }

        public PipeSource(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Pump");
            SetFluidType(1);
        }

        public override string GetCustomDescription()
        {
            return "PipeSource -> " + base.GetCustomDescription();
        }

        public override bool Tick(ulong delta)
        {
            // Log.Out("Tick source {0}", IsPowered);
            if (IsPowered)
            {
                // Add some murky water
                AddFillLevel(0.1f * delta, 1);
                // Call base pump tick
                base.Tick(delta);
                //base.Tick(world, delta);
                // Only reduce to max amount after pumping
                // FillLevel = System.Math.Min(MaxFillState, FillLevel);
                // ToDo: check if we still have water around us!?
            }
            return true;
        }

    }
}
