using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NodeManager
{
    public class PipeWaterBoiler : NodeBlock<BlockWaterBoiler>, IPoweredNode
    {

        public static TYPES NodeType = TYPES.PipeWaterBoiler;
        public override uint StorageID => (uint)TYPES.PipeWaterBoiler;

        public override ulong NextTick => 30;

        public bool IsPowered { get; set; }

        PipeReservoir Input;
        PipeReservoir Output;

        Vector3i InPos(byte rotation) => WorldPos + FullRotation.Rotate(rotation, BLOCK.InputPosition);
        Vector3i OutPos(byte rotation) => WorldPos + FullRotation.Rotate(rotation, BLOCK.OutputPosition);

        public PipeWaterBoiler(Vector3i position, BlockValue bv)
            : base(position, bv)
        {

            string name = BLOCK.GetBlockName();
            BlockValue BlkIn = Block.GetBlockValue(name + "Input");
            BlockValue BlkOut = Block.GetBlockValue(name + "Output");
            if (BlkIn.isair) throw new Exception(name + "Input not found");
            if (BlkOut.isair) throw new Exception(name + "Output not found");
            Log.Out("Created water boiler, has {0} {1}",
                InPos(Rotation), OutPos(Rotation));
            BlkIn.rotation = BlkOut.rotation = Rotation;
            Input = new PipeReservoir(InPos(Rotation), BlkIn);
            Output = new PipeReservoir(OutPos(Rotation), BlkOut);
            Input.IsPowered = Output.IsPowered = true;
        }

        public override void OnAfterLoad()
        {
            base.OnAfterLoad();
            Log.Warning("===== AfterLoad {0} {1}",
                InPos(Rotation), OutPos(Rotation));
            if (Manager == null) return;
            Manager.TryGetNode(InPos(Rotation), out Input);
            Manager.TryGetNode(OutPos(Rotation), out Output);
            Log.Warning("Read after load {0} {1}", Input, Output);
        }

        public PipeWaterBoiler(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Water Boiler");
            // Clean Water
            // SetFluidType(2);
        }

        // Keep a list of plants that get water from us.
        internal readonly HashSet<PipeWell> Wells
            = new HashSet<PipeWell>();

        public override string GetCustomDescription()
        {
            return string.Format("Boiler In: {0}, Out: {1}",
                Input?.GetCustomDescription(),
                Output?.GetCustomDescription());
        }

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            if (manager == null)
            {
                Manager?.RemoveManagedNode(InPos(Rotation));
                Manager?.RemoveManagedNode(OutPos(Rotation));
            }
            else
            {
                Input?.AttachToManager(manager);
                Output?.AttachToManager(manager);
            }
            base.OnManagerAttached(manager);

            // Manager?.RemoveFluidConverer(this);
            // manager?.AddFluidConverter(this);
        }

        //protected override void UpdateGrid(PipeGrid grid)
        //{
        //    base.UpdateGrid(grid);
        //    Log.Out("Update irrigation grid");
        //}

        public override bool Tick(ulong delta)
        {
            if (Input == null) return true;
            if (Output == null) return true;
            if (Input.FillState > 1)
            {
                Input.FillState -= 1;
                Output.FillState += 0.25f;
            }
            Log.Warning("Tick water boiler");
            return base.Tick(delta);
        }

    }
}
