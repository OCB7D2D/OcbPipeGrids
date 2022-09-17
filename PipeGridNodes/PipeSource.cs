﻿using System.Collections.Concurrent;
using System.IO;

namespace PipeManager
{
    public class PipeSource : PipePump, IPoweredNode
    {
        public override uint StorageID => 4;

        public override ulong NextTick => 10;

        public PipeSource(Vector3i position, byte connectMask, byte rotation)
            : base(position, connectMask, rotation) { }

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

        public override void Tick(ulong delta, ConcurrentQueue<IActionClient> output)
        {
            Log.Out("Tick source {0}", IsPowered);
            if (IsPowered)
            {
                // Add some water
                FillState += 0.1f * delta;
                // Call base pump tick
                base.Tick(delta, output);
                //base.Tick(world, delta);
                // Only reduce to max amount after pumping
                FillState = System.Math.Min(MaxFillState, FillState);
                // ToDo: check if we still have water around us!?
            }
        }

    }
}
