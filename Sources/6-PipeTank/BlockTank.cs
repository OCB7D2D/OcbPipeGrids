using NodeFacilitator;
using System;
using System.Collections.Generic;
using System.Xml;

class BlockTank : ImpBlockPipeReservoirPowered, IMultiNodeBlock
{

    //########################################################
    //########################################################

    public override TYPES NodeType => PipeTank.NodeType;

    //########################################################
    //########################################################

    public override bool NeedsPower => true;

    //########################################################
    //########################################################

    // Custom XML config is stored into this container
    public List<Tuple<Vector3i, string, byte>> Nodes { get; }
        = new List<Tuple<Vector3i, string, byte>>();

    // Parse custom xml after block is initialized
    public void PostParse(XmlElement xml) =>
        MultiNodeHelper.ParseConfig(this, xml);

    //########################################################
    //########################################################

}
