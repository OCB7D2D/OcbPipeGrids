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
            Tuple<Vector3i, T>[] items = others.RadialSearch(
                self.WorldPos + self.RotatedOffset, self.RotatedReach, 255,
                (Tuple<Vector3i, T> kv, int dist) => self.IsInReach(kv.Item1));
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
            Tuple<Vector3i, T>[] items = others.RadialSearch(self.WorldPos, reach, 255,
                (Tuple<Vector3i, T> kv, int dist) => kv.Item2.IsInReach(self.WorldPos));
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        public static bool IsInReach(IReacher reacher, Vector3i target)
        {

            Log.Out("Check item reach at {0} for pos {1}", reacher.WorldPos, target);
            IReacherBlock BLOCK = reacher.RBLK;

            // That doesn't work for even multidim, since rotation pivot changes

            Vector3i offset = target - reacher.WorldPos;
            Log.Out("  offset in world space {0} with rotation {1}", offset, reacher.Rotation);
            offset = FullRotation.InvRotate(reacher.Rotation, offset);
            Log.Out("  offset rotated into local space {0}", offset);
            offset -= BLOCK.ReachOffset; // Adjust in our space
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

            // Adjust for odd block shift
            // if (dim.x % 2 == 0) max.x += 1;
            // if (dim.y % 2 == 0) max.y += 1;
            // if (dim.z % 2 == 0) max.z += 1;
            // Check if point is withing our boundaries
            Log.Out("  min {0} to max {1} at offset {2}", min, max, offset);
            var rv = min.x <= offset.x && offset.x <= max.x
                && min.y <= offset.y && offset.y <= max.y
                && min.z <= offset.z && offset.z <= max.z;
            
            Log.Out("  Result {0}", rv);
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
