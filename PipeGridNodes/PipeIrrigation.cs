using System.Collections.Concurrent;
using System.IO;

namespace PipeManager
{
    class PipeIrrigation : PipePump, IPoweredNode
    {

        public override ulong NextTick => 30;

        public override uint StorageID => 3;

        public PipeIrrigation(Vector3i position, byte connectMask, BlockValue bv)
            : base(position, connectMask, bv) { }


        public PipeIrrigation(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Irrigation");
        }

        public override string GetCustomDescription()
        {
            return "PipeIrrigation";
        }

        protected override void OnManagerAttached(PipeGridManager manager)
        {
            base.OnManagerAttached(manager);
            Log.Out("Manager attached to irrigation");
        }

        protected override void UpdateGrid(PipeGrid grid)
        {
            base.UpdateGrid(grid);
            Log.Out("Update irrigation grid");
        }

        public override void Tick(ulong delta, ConcurrentQueue<IActionClient> output)
        {
            base.Tick(delta, output);
            Log.Warning("Tick irrigation");
        }

    }
}
