using System.Collections.Concurrent;
using System.IO;

namespace NodeManager
{
    public class PipeIrrigation : PipePump, IPoweredNode
    {

        public override ulong NextTick => 30;

        public override uint StorageID => 3;

        public PipeIrrigation(Vector3i position, BlockValue bv)
            : base(position, bv) { }


        public PipeIrrigation(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Irrigation");
        }

        public override string GetCustomDescription()
        {
            return string.Format("Irrigation {0}",
                base.GetCustomDescription());
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            base.OnManagerAttached(manager);
            Manager?.RemoveIrrigation(WorldPos);
            manager?.AddIrrigation(this);
        }

        protected override void UpdateGrid(PipeGrid grid)
        {
            base.UpdateGrid(grid);
            Log.Out("Update irrigation grid");
        }

        public override void Tick(ulong delta)
        {
            base.Tick(delta);
            Log.Warning("Tick irrigation");
        }

    }
}
