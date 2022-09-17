using System.IO;

namespace PipeManager
{
    public class PipePump : PipeReservoir, IPoweredNode
    {
        public override uint StorageID => 2;

        public PipePump(Vector3i position, byte connectMask, BlockValue bv)
            : base(position, connectMask, bv) { }

        public PipePump(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Pump");
        }

        public override string GetCustomDescription()
        {
            return "PipePump -> " + base.GetCustomDescription();
        }

    }
}
