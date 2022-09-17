using System;
using System.Collections.Generic;
using System.IO;

namespace NodeFacilitator
{

    public class PipeTank : PipeReservoir, IPoweredNode
    {

        //########################################################
        //########################################################

        public static new TYPES NodeType = TYPES.PipeTank;
        public override uint StorageID => (uint)NodeType;

        //########################################################
        //########################################################

        public IMultiNodeBlock MBLK;

        //########################################################
        //########################################################

        readonly List<PipeConnection> ConnectorNodes
            = new List<PipeConnection>();

        //########################################################
        //########################################################

        public override void ParseBlockConfig()
        {
            base.ParseBlockConfig();
            GetBlock(out MBLK);
        }

        public PipeTank(
            BinaryReader br)
        : base(br)
        { }

        public PipeTank(
            Vector3i position,
            BlockValue bv)
        : base(position, bv)
        {
            MultiNodeHelper.OnCreate(this, MBLK, ref ConnectorNodes);
        }

        //########################################################
        //########################################################

        public override void OnAfterLoad()
        {
            ConnectorNodes.Clear();
            base.OnAfterLoad();
            MultiNodeHelper.OnAfterLoad(
                this, MBLK, ConnectorNodes);
        }

        //########################################################
        //########################################################

        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            MultiNodeHelper.OnManagerAttached(
                this, MBLK, ConnectorNodes, manager);
            base.OnManagerAttached(manager);
        }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
        {
            return "Tank -> " + base.GetCustomDescription();
        }


        //########################################################
        //########################################################

        protected override void TickSegment(float capacity, List<PipeReservoir> reservoirs)
        {
            if (reservoirs.Count == 0) return;
            if (reservoirs[0] != this) Log.Warning("Not what I expected");
            for (int i = 1; i < reservoirs.Count; i++)
            {
                PipeReservoir reservoir = reservoirs[i];
                // Start to take from reservoirs if they are filled 50%
                if (reservoir.FillLevel / reservoir.MaxFillState < 0.5f) continue;
                float taking = Math.Min(capacity * 2, MaxFillState - FillLevel);
                taking = Math.Min(taking, reservoir.FillLevel);
                reservoir.FillLevel -= taking;
                FillLevel += taking;
            }

        }

        //########################################################
        //########################################################

    }
}
