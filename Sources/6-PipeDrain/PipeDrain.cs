using System.IO;

namespace NodeFacilitator
{
    public class PipeDrain : PipeReservoir, IPoweredNode
    {
        public static new TYPES NodeType = TYPES.PipeDrain;
        public override uint StorageID => (uint)NodeType;

        public PipeDrain(Vector3i position, BlockValue bv) : base(position, bv) { }

        public PipeDrain(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Drain");
        }

        public override string GetCustomDescription()
        {
            return "PipeDrain -> " + base.GetCustomDescription();
        }

        public override bool Tick(ulong delta)
        {
            bool rv = base.Tick(delta);
            FillLevel = 0;
            return rv;
        }

    }
}
