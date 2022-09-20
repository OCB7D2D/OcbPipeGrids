using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{
    public class PipeSource : PipePump, IPoweredNode
    {
        public override uint StorageID => 4;

        public override ulong NextTick => 10;

        public PipeSource(Vector3i position, BlockValue bv)
            : base(position, bv) { }

        public PipeSource(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Pump");
        }

        public override string GetCustomDescription()
        {
            return "PipeSource -> " + base.GetCustomDescription();
        }

        public override bool Tick(ulong delta)
        {
            Log.Out("Tick source {0}", IsPowered);
            if (IsPowered)
            {
                // Add some water
                FillState += 0.1f * delta;
                // Call base pump tick
                base.Tick(delta);
                //base.Tick(world, delta);
                // Only reduce to max amount after pumping
                FillState = System.Math.Min(MaxFillState, FillState);
                // ToDo: check if we still have water around us!?
            }
            return true;
        }

    }
}
