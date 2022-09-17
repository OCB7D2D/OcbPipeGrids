using KdTree3;
using System;

namespace NodeFacilitator
{

    public static class ReachHelper
    {

        // The most simple case where we have a static radius
        // We simply add everything that is within the radius
        public static void AddLinks<T, M>(IWorldLink<T> self,
            KdTree<M>.Vector3i<T> others, int reach)
                where T : IWorldPos where M : KdTree3.IMetric
        {
            Tuple<Vector3i, T>[] items = others.RadialSearch(self.WorldPos, reach, 255);
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        // Get links from part that has the reach
        // This is straight forward as we know reach exactly
        // We can directly pass rotated reach to KD-Tree search
        public static void QueryLinks<S, T, M>(S self, KdTree<M>.Vector3i<T> others, Vector3i shift)
                where S : IReacher, IWorldLink<T> where T : IWorldRotated where M : KdTree3.IMetric
        {

            Log.Out("Query {0}", self);
            if (self == null || self.RBLK == null || self.RBLK.BLK == null)
            {
                Log.Error("EMPTY {0} {1} {2}", self, self?.RBLK, self?.RBLK?.BLK);
            }
            Vector3i dim = self.RBLK.Dimensions;
            Log.Out("===> {0} {1} {2} {3}", self, others, shift, dim);
            Vector3i reach = FullRotation.GetReach(self.Rotation, self.RBLK.Reach);
            Vector3i offset = FullRotation.GetOffset(self.Rotation, self.RBLK.Reach);

            offset -= new Vector3i(dim.x / 2, dim.y / 2, dim.z / 2);

            // Calculate lower and upper boundary
            Vector3i max = reach;
            Vector3i min = max * -1;
            min += Vector3i.one;
            max += shift;
            min -= dim;
            
            offset = FullRotation.Rotate(self.Rotation, offset);
            min = FullRotation.Rotate(self.Rotation, min);
            max = FullRotation.Rotate(self.Rotation, max);

            if (min.x > max.x) (max.x, min.x) = (min.x, max.x);
            if (min.y > max.y) (max.y, min.y) = (min.y, max.y);
            if (min.z > max.z) (max.z, min.z) = (min.z, max.z);

            Tuple<Vector3i, T>[] items = others.RadialSearch(
                self.WorldPos + offset, min, max, 255,
                // This should ideally not be needed anymore
                (Tuple<Vector3i, T> kv, int dist) => IsInReach(self, kv.Item1
                    + FullRotation.Rotate(kv.Item2.Rotation, shift), true));
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        // Search links that have reach, which is not straight forward
        // Those items may have different reach, so our KD-tree can't
        // exactly answer which ones are actually reaching us. So we
        // need to query a maximum volume and then narrow it down.

        public static void SearchLinks<T,M>(IWorldLink<T> self,
            KdTree<M>.Vector3i<T> others, int reach, Vector3i shift)
                where T : IReacher where M : KdTree3.IMetric
        {
            Tuple<Vector3i, T>[] items = others.RadialSearch(self.WorldPos, reach, 255,
                (Tuple<Vector3i, T> kv, int dist) => IsInReach(kv.Item2, self.WorldPos
                    + FullRotation.Rotate(self.Rotation, shift), false));
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        public static bool IsInReach(IReacher reacher, Vector3i target, bool check)
        {

            if (reacher.RBLK == null) Log.Error("RBLK is nnull");
            if (reacher.RBLK?.BLK == null) Log.Error("BLK is nnull");
            IReacherBlock BLOCK = reacher.RBLK;

            // That doesn't work for even multidim, since rotation pivot changes

            Vector3i offset = reacher.WorldPos - target;
            offset = FullRotation.InvRotate(reacher.Rotation, offset);
            offset += FullRotation.GetOffset(reacher.Rotation, BLOCK.Reach); // Adjust in our space
            
            // Get settings for reachable area
            Vector3i dim = reacher.RBLK.Dimensions;
            Vector3i reach = FullRotation.GetReach(reacher.Rotation, BLOCK.Reach);

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
            // Log.Out("  min {0} to max {1} at offset {2}", min, max, offset);
            // Log.Out(" ==> {0} < {1} < {2}", min.z, offset.z, max.z);
            var rv = min.x <= offset.x && offset.x <= max.x
                && min.y <= offset.y && offset.y <= max.y
                && min.z <= offset.z && offset.z <= max.z;
            
            // Log.Out("  Result {0}", rv);
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
