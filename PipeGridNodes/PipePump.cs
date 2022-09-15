using System.IO;

namespace PipeManager
{
    public class PipePump : PipeConnection
    {
        public override uint StorageID => 2;

        public PipePump(Vector3i position, byte connectMask, byte rotation)
            : base(position, connectMask, rotation) { }

        public PipePump(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Pump");
        }

        public override string GetCustomDescription()
        {
            return "PipePump";
        }

    }
}
