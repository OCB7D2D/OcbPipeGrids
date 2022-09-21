using System;

namespace NodeManager
{
    public static class ConnectionHelper
    {

        static readonly byte MaxDistance = 5;

        public static bool CanConnect(byte mask, byte rotation, byte face)
        {
            face = FullRotation.InvFace(face, rotation);
            return (mask & (byte)(1 << face)) != 0;
        }

        public static bool CanConnect(BlockConnector neighbour, byte face)
            => CanConnect(neighbour.ConnectMask, neighbour.Rotation, face);

        public static bool CanConnect(BlockValue BV, byte face, BlockValue TO)
        {
            return false;
        }

        private static bool AreFacesAligned(BlockConnector ours,
            byte face, BlockConnector other, byte mirror)
        {
            face = FullRotation.InvFace(face, ours.Rotation);
            // byte mirror = FullRotation.Mirror(face);
            mirror = FullRotation.InvFace(mirror, other.Rotation);

            uint mask1 = ((ours.SideMask & (15U << (4 * face))) >> (4 * face)) & 15;
            uint mask2 = ((other.SideMask & (15U << (4 * mirror))) >> (4 * mirror)) & 15;

            if (mask1 == 0 || mask2 == 0)
                return mask1 == mask2;

            Log.Out("= Checking {0} vs {1} at {2} and {3}",
                mask1, mask2, ours.Rotation, other.Rotation);

            if (mask1 == 1) mask1 = 1;
            else if (mask1 == 2) mask1 = 2;
            else if (mask1 == 4) mask1 = 4;
            else if (mask1 == 8) mask1 = 5;
            else Log.Warning("Invalid mask1");

            if (mask2 == 1) mask2 = 1;
            else if (mask2 == 2) mask2 = 2;
            else if (mask2 == 4) mask2 = 4;
            else if (mask2 == 8) mask2 = 5;
            else Log.Warning("Invalid mask2");

            Log.Out("1 Checking {0} vs {1}", mask1, mask2);

            mask1 = FullRotation.GetFace((byte)mask1, ours.Rotation);
            mask2 = FullRotation.GetFace((byte)mask2, other.Rotation);
            mask2 = FullRotation.Mirror((byte)mask2);

            Log.Out("2 Checking {0} vs {1}", mask1, mask2);
            return mask1 == mask2;
        }

        private static readonly int[] GridCache = new int[6];

        public static bool CanAddConnector(BlockConnector block, BlockConnector[] NB)
        {
            // Count existing connections and distances
            int connections = 0, longest = 0, second = 0;

            // Check for a neighbour on every side
            for (byte face = 0; face < 6; face++)
            {
                // Skip over non-connector neighbours
                if (NB[face].ConnectMask == 255) continue;
                // Condition may look weird, but we allow to place blocks
                // next to each other if they don't share any exit. If only
                // one exit aligns with the new block, we don't allow it.

                bool a = CanConnect(block, face);
                byte mirror = FullRotation.Mirror(face);
                bool b = CanConnect(NB[face], mirror);

                // Both faces will connect?
                if (a && b)
                {
                    if (!AreFacesAligned(block, face, NB[face], mirror)) return false;

                    // Check if we create any circular grids
                    for (int n = 0; n < connections; n += 1)
                    {
                        if (GridCache[n] == NB[face].Grid)
                        {
                            Log.Out("Cyclic grid {0} vs {1}",
                                GridCache[n], NB[face].Grid);
                            return false;
                        }
                    }
                    // Remember the grid for this connector
                    GridCache[connections] = NB[face].Grid;

                    // Count longest and second longest connection
                    // In case grid will be joined, we need both
                    if (NB[face].Distance > longest)
                    {
                        second = longest;
                        longest = NB[face].Distance;
                    }
                    else if (NB[face].Distance > second)
                    {
                        second = NB[face].Distance;
                    }

                    // Check for distance if grid is joined
                    // That is where we need the longest and
                    // second longest count. Both plus 1 must
                    // be below the maximum allowed length.
                    if (!block.IsBreaker)
                    {
                        int dist = longest + second + 1;
                        if (dist >= MaxDistance)
                        {
                            Log.Out("Distance to big {0} + {1} > {2}",
                                longest, second, MaxDistance);
                            return false;
                        }
                        // Log.Out("Block is not a breaker");
                    }

                    // Found connector
                    connections += 1;
                }
                // Only one face would connect
                else if (a || b)
                {
                    // Log.Out("Only one connector {0} or {1}", a, b);
                    return false;
                }
                // else continue; // No connectors
            }

            return true;

        }

    }
}
