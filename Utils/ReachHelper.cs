using KdTree3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NodeManager
{

    public static class ReachHelper
    {

        // The most simple case where we have a static radius
        // We simply add everything that is within the radius
        public static void AddLinks<T, M>(IWorldLink<T> self,
            KdTree<M>.Vector3i<T> others, int reach)
                where T : IWorldPos where M : IMetric
        {
            Tuple<Vector3i, T>[] items = others.RadialSearch(self.WorldPos, reach, 255);
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        // Get links from part that has the reach
        // This is straight forward as we know reach exactly
        // We can directly pass rotated reach to KD-Tree search
        public static void QueryLinks<S, T, M>(S self, KdTree<M>.Vector3i<T> others)
                where S : IReacher, IWorldLink<T> where T : IWorldPos where M : IMetric
        {

            Vector3i dim = self.Dimensions;
            Vector3i reach = self.RBLK.BlockReach;
            Vector3i offset = self.RBLK.ReachOffset;

            offset -= new Vector3i(dim.x / 2, dim.y / 2, dim.z / 2);

            // Calculate lower and upper boundary
            Vector3i max = reach;
            Vector3i min = max * -1;
            min -= dim;

            // reach.x -= dim.x / 2;
            // reach.y -= dim.y / 2;
            // reach.z -= dim.z / 2;

            // min += offset;
            // max += offset;

            min += Vector3i.one;
            //max -= Vector3i.one;
            
            offset = FullRotation.Rotate(self.Rotation, offset);
            min = FullRotation.Rotate(self.Rotation, min);
            max = FullRotation.Rotate(self.Rotation, max);

            if (min.x > max.x)
            {
                var tmp = min.x;
                min.x = max.x;
                max.x = tmp;
            }
            if (min.y > max.y)
            {
                var tmp = min.y;
                min.y = max.y;
                max.y = tmp;
            }
            if (min.z > max.z)
            {
                var tmp = min.z;
                min.z = max.z;
                max.z = tmp;
            }



            Log.Out("Apply offset {0}", offset);

            Tuple<Vector3i, T>[] items = others.RadialSearch(
                self.WorldPos + offset, min, max, 255,
                // This should ideally not be needed anymore
                (Tuple<Vector3i, T> kv, int dist) => self.IsInReach(kv.Item1, true));
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        // Search links that have reach, which is not straight forward
        // Those items may have different reach, so our KD-tree can't
        // exactly answer which ones are actually reaching us. So we
        // need to query a maximum volume and then narrow it down.
        public static void SearchLinks<T,M>(IWorldLink<T> self,
            KdTree<M>.Vector3i<T> others, int reach)
                where T : IReacher where M : IMetric
        {
            // We must optimize reach here, but now way around
            Tuple<Vector3i, T>[] items = others.RadialSearch(self.WorldPos, reach, 255,
                (Tuple<Vector3i, T> kv, int dist) => kv.Item2.IsInReach(self.WorldPos, false));
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        public static bool IsInReach(IReacher reacher, Vector3i target, bool check)
        {

            Log.Out("Re-Check item at {0} for target {1}", reacher.WorldPos, target);
            IReacherBlock BLOCK = reacher.RBLK;

            // That doesn't work for even multidim, since rotation pivot changes

            Vector3i offset = reacher.WorldPos - target;
            Log.Out("  offset in world space {0} with rotation {1}", offset, reacher.Rotation);
            offset = FullRotation.InvRotate(reacher.Rotation, offset);
            Log.Out("  offset rotated into local space {0}", offset);
            offset += BLOCK.ReachOffset; // Adjust in our space
            Log.Out("  offset in relative world space {0}", offset);

            
            // Get settings for reachable area
            Vector3i dim = reacher.Dimensions;
            Vector3i reach = BLOCK.BlockReach;
            
            offset.x -= dim.x / 2;
            offset.y -= dim.y / 2;
            offset.z -= dim.z / 2;

            // Calculate lower and upper boundary
            Vector3i max = reach;
            Vector3i min = max * -1;
            min -= dim;
            min += Vector3i.one;
            //max -= Vector3i.one;

            // Adjust for odd block shift
            // if (dim.x % 2 == 0) max.x += 1;
            // if (dim.y % 2 == 0) max.y += 1;
            // if (dim.z % 2 == 0) max.z += 1;
            // Check if point is withing our boundaries
            Log.Out("  min {0} to max {1} at offset {2}", min, max, offset);
            Log.Out(" ==> {0} < {1} < {2}", min.z, offset.z, max.z);
            var rv = min.x <= offset.x && offset.x <= max.x
                && min.y <= offset.y && offset.y <= max.y
                && min.z <= offset.z && offset.z <= max.z;
            
            Log.Out("  Result {0}", rv);
            if (check && rv == false)
                Log.Error("This should not happen anymore, radial search returned failure");
            return rv;


            // Vector3i offset = FullRotation.Rotate(reacher.Rotation, reacher.ReachOffset);
            // Vector3i reach = FullRotation.Rotate(reacher.Rotation, reacher.BlockReach);
            // Vector3i dim = FullRotation.Rotate(reacher.Rotation, reacher.Dimensions);

            // position = FullRotation.Rotate(reacher.Rotation, position);
            // target = FullRotation.Rotate(reacher.Rotation, target);
            
            // byte rot = reacher.Rotation;
        }
    }
}
