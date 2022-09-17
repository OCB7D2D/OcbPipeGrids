using System.Collections.Generic;
using System;

namespace NodeFacilitator
{

    //########################################################
    //########################################################

    public interface IMultiNodeBlock : ICustomBlockParser
    {
        List<Tuple<Vector3i, string, byte>> Nodes { get; }
    }

    //########################################################
    //########################################################

}
