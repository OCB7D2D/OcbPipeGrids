using System.IO;

namespace NodeManager
{
    public class PipePump : PipeReservoir, IPoweredNode
    {
        public override uint StorageID => 2;

        public PipePump(Vector3i position, BlockValue bv)
            : base(position, bv) { }

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
