namespace NodeManager
{
    public static class ConnectionHelper
    {

        static readonly byte MaxDistance = 5;

        public static bool CanConnect(byte mask, byte rotation, byte side)
        {
            side = FullRotation.InvSide(side, rotation);
            return (mask & (byte)(1 << (byte)side)) != 0;
        }

        public static bool CanConnect(BlockConnector neighbour, byte side)
            => CanConnect(neighbour.ConnectMask, neighbour.Rotation, side);

        private static readonly int[] GridCache = new int[6];

        public static bool CanAddConnector(BlockConnector block, BlockConnector[] NB)
        {
            // Count existing connections and distances
            int connections = 0, longest = 0, second = 0;

            // Check for a neighbour on every side
            for (byte side = 0; side < 6; side++)
            {
                // Skip over non-connector neighbours
                if (NB[side].ConnectMask == 255) continue;
                // Condition may look weird, but we allow to place blocks
                // next to each other if they don't share any exit. If only
                // one exit aligns with the new block, we don't allow it.

                bool a = CanConnect(block, side);
                byte mirror = FullRotation.Mirror(side);
                bool b = CanConnect(NB[side], mirror);

                // Both sides will connect?
                if (a && b)
                {
                    // Check if we create any circular grids
                    for (int n = 0; n < connections; n += 1)
                    {
                        if (GridCache[n] == NB[side].Grid)
                        {
                            Log.Out("Cyclic grid {0} vs {1}",
                                GridCache[n], NB[side].Grid);
                            return false;
                        }
                    }
                    // Remember the grid for this connector
                    GridCache[connections] = NB[side].Grid;

                    // Count longest and second longest connection
                    // In case grid will be joined, we need both
                    if (NB[side].Distance > longest)
                    {
                        second = longest;
                        longest = NB[side].Distance;
                    }
                    else if (NB[side].Distance > second)
                    {
                        second = NB[side].Distance;
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
                // Only one side would connect
                else if (a || b)
                {
                    Log.Out("Only one connector {0} or {1}", a, b);
                    return false;
                }
                // else continue; // No connectors
            }

            return true;

        }

    }
}
