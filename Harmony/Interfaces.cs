using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{
    interface IBoundHelper
    {
        Vector3i BlockReach { get; }
        Vector3i ReachOffset { get; }
    }

}
