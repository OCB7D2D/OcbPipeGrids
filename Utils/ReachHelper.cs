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
        public static void InitBlock(IReacherBlock block)
        {
            var properties = block.IBLK.BLK.Properties;
            if (properties.Contains("BlockReach")) block.BlockReach =
                Vector3i.Parse(properties.GetString("BlockReach"));
            if (properties.Contains("ReachOffset")) block.ReachOffset =
                Vector3i.Parse(properties.GetString("ReachOffset"));
            if (properties.Contains("BoundHelperColor")) block.BoundHelperColor =
                StringParsers.ParseColor32(properties.GetString("BoundHelperColor"));
            if (properties.Contains("ReachHelperColor")) block.ReachHelperColor =
                StringParsers.ParseColor32(properties.GetString("ReachHelperColor"));
        }


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
                (Tuple<Vector3i, T> kv, int dist) => BlockHelper.IsInReach(self.WorldPos, kv));
            foreach (Tuple<Vector3i, T> kv in items) self.AddLink(kv.Item2);
        }

        public static bool IsInReach(IReacher reacher, Vector3i target)
        {
            Log.Out("Check in reach for {0} {1}", reacher, reacher?.IBLK);
            Vector3i offset = target - reacher.WorldPos;
            Log.Out("Check if in reach {0}", offset);
            offset = FullRotation.InvRotate(reacher.Rotation, offset);
            offset -= reacher.ReachOffset; // Adjust in our space
            Log.Out("   local reach {0} with {1}", offset, reacher.BlockReach);
            // Get settings for reachable area
            Vector3i dim = reacher.Dimensions;
            Vector3i reach = reacher.BlockReach;
            Log.Out("Has Dim {0} plus {1}", dim, reach);
            // Calculate lower and upper boundary
            Vector3i max = reach + new Vector3i(
                dim.x / 2, dim.y / 2, dim.z / 2);
            Vector3i min = max * -1;
            // Adjust for odd block shift
            if (dim.x % 2 == 0) max.x += 1;
            if (dim.y % 2 == 0) max.y += 1;
            if (dim.z % 2 == 0) max.z += 1;
            // Check if point is withing our boundaries
            Log.Out("From {0} to {1}", min, max);
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
