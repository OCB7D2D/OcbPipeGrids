using System.IO;

namespace NodeFacilitator
{
    public class PipePump : PipeReservoir, IPoweredNode
    {

        //########################################################
        //########################################################

        public static new TYPES NodeType = TYPES.PipePump;
        public override uint StorageID => (uint)NodeType;

        //########################################################
        //########################################################

        public PipePump(
            Vector3i position,
            BlockValue bv)
        : base(position, bv)
        { }

        public PipePump(
            BinaryReader br)
        : base(br)
        { }

        //########################################################
        //########################################################

        public override string GetCustomDescription()
            => "Pump " + base.GetCustomDescription();

        //########################################################
        //########################################################

    }
}
