using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{
    class WellHelper
    {
        public static bool IsMurkyWater(int type)
            => type == 1 || type == 2 || type == 3;

        public static float MixFluids(
            float current, float added,
            float taken, ulong span = 12000)
        {
            // float full = NodeManager.TimeScale(span);
            // float scale = NodeManager.TimeScale(delta);
            current = added * taken +
                current * (span - taken);
            Log.Out("Adding for slice {0}", taken);
            current /= span;
            return current;
        }


        public static float ConsumeFluids(
            HashSet<IWell> wells, float want)
        {
            float sum = 0f;
            foreach (IWell well in wells)
                sum += well.FillState;
            // Can take partially
            if (sum > want)
            {
                float taken = 0f;
                float factor = want / sum;
                foreach (IWell well in wells)
                {
                    float take = well.FillState * factor;
                    taken += take; well.FillState -= take;
                }
                return taken;
            }
            // Otherwise take everything
            foreach (IWell well in wells)
                well.FillState = 0;
            return sum;
        }
    }
}
