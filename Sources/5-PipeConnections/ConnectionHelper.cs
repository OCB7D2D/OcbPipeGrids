using JetBrains.Annotations;
using System;

namespace NodeFacilitator
{
    public static class ConnectionHelper
    {

        // ToDo: make this configurable somehow?
        // OK for initial testing, problem for another day
        private static int[] MaxDistances = { 20, 16, 12, 8, 6 };

        public static int MaxNodesPerDiameter(byte diameter)
        {
            for (int i = 0; i < 8; i++)
                if ((diameter & (1 << i)) != 0)
                    return MaxDistances[i];
            return 50;
        }

        public static bool CanConnect(byte mask, byte rotation, byte face)
        {
            face = FullRotation.InvFace(face, rotation);
            return (mask & (byte)(1 << face)) != 0;
        }

        public static bool CanConnect(BlockConnector neighbour, byte face)
            => CanConnect(neighbour.ConnectMask, neighbour.Rotation, face);
        private static bool AreFacesAligned(BlockConnector ours,
            byte face, BlockConnector other, byte mirror)
        {
            return AreFacesAligned(
                face, ours.Rotation, ours.SideMask,
                mirror, other.Rotation, other.SideMask);
        }

        private static bool AreFacesAligned(
            byte face, byte ourRotation, uint ourSideMask,
            byte mirror, byte otherRotation, uint otherSideMask)
        {
            face = FullRotation.InvFace(face, ourRotation);
            // byte mirror = FullRotation.Mirror(face);
            mirror = FullRotation.InvFace(mirror, otherRotation);

            uint mask1 = ((ourSideMask & (15U << (4 * face))) >> (4 * face)) & 15;
            uint mask2 = ((otherSideMask & (15U << (4 * mirror))) >> (4 * mirror)) & 15;

            if (mask1 == 0 || mask2 == 0)
                return mask1 == mask2;

            //Log.Out("= Checking {0} vs {1} at {2} and {3}",
            //    mask1, mask2, ourRotation, otherRotation);

            if (mask1 == 1) mask1 = (byte)FullRotation.Face.left;
            else if (mask1 == 2) mask1 = (byte)FullRotation.Face.forward;
            else if (mask1 == 4) mask1 = (byte)FullRotation.Face.right;
            else if (mask1 == 8) mask1 = (byte)FullRotation.Face.back;
            else Log.Warning("Invalid mask1");

            if (mask2 == 1) mask2 = (byte)FullRotation.Face.left;
            else if (mask2 == 2) mask2 = (byte)FullRotation.Face.forward;
            else if (mask2 == 4) mask2 = (byte)FullRotation.Face.right;
            else if (mask2 == 8) mask2 = (byte)FullRotation.Face.back;
            else Log.Warning("Invalid mask2");

            //Log.Out("1 Checking {0} vs {1}", mask1, mask2);

            mask1 = FullRotation.GetFace((byte)mask1, ourRotation);
            mask2 = FullRotation.GetFace((byte)mask2, otherRotation);
            mask2 = FullRotation.Mirror((byte)mask2);

            //Log.Out("2 Checking {0} vs {1}", mask1, mask2);
            return mask1 == mask2;
        }

        private static readonly int[] GridCache = new int[6];

        //public static bool CanConnect(BlockValue BV, BlockFace face, BlockValue TO)
        //{
        //    if (BV.Block is IBlockConnection ours)
        //    {
        //        if (TO.Block is IBlockConnection other)
        //        {
        //
        //            byte bf = (byte)FullRotation.BlockFaceToFace(face);
        //            // if (NB[face].ConnectMask == 255) continue;
        //            bool a = CanConnect(ours.ConnectMask, BV.rotation, bf);
        //            byte mirror = FullRotation.Mirror(bf);
        //            bool b = CanConnect(other.ConnectMask, TO.rotation, mirror);
        //            return a && b && AreFacesAligned(
        //                bf, BV.rotation, ours.SideMask,
        //                mirror, TO.rotation, other.SideMask);
        //        }
        //    }
        //    return false;
        //}

        public static bool CanAddConnector(BlockConnector block,
            BlockConnector[] NB, bool ignoreBlockers = false)
        {

            if (NB == null) Log.Warning("NB is null");

            int pipes = 1; // We are trying to add one
            int connections = 0; // Count connections
            ushort fluid = block.FluidType;

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

                //Log.Out("Compare {0} vs {1}", block.FluidType, NB[face].FluidType);

                if (NB[face].FluidType != 0)
                {
                    if (fluid == 0) fluid = NB[face].FluidType;
                    else if (NB[face].FluidType != fluid)
                    {
                        Log.Out("Grid Fluids not compatible");
                        return false;
                    }
                }

                // Both faces will connect?
                if (a && b)
                {
                    // Check if faces are aligned in terms of face offset, e.g.
                    // with pipes that are close to the wall and not centered.
                    // These need an additional check for matching rotations.
                    if (!AreFacesAligned(block, face, NB[face], mirror))
                    {
                        Log.Out("Faces not aligned");
                        return false;
                    }

                    // Check if we create any circular grids
                    int n; for(n = 0; n < connections; n += 1)
                    {
                        if (GridCache[n] == NB[face].Grid)
                        {
                            // Log.Out("Same grid again {0} vs {1}",
                            //     GridCache[n], NB[face].Grid);
                            break;
                        }
                    }
                    // Check if loop was aborted
                    if (n != connections) continue;
                    // Remember the grid for this connector
                    GridCache[connections] = NB[face].Grid;

                    pipes += NB[face].Pipes;

                    // Log.Out(" ++ Add pipes {0}", pipes);

                    // Count longest and second longest connection
                    // In case grid will be joined, we need both
                    //if (NB[face].Distance > longest)
                    //{
                    //    second = longest;
                    //    longest = NB[face].Distance;
                    //}
                    //else if (NB[face].Distance > second)
                    //{
                    //    second = NB[face].Distance;
                    //}

                    // Check for distance if grid is joined
                    // That is where we need the longest and
                    // second longest count. Both plus 1 must
                    // be below the maximum allowed length.
                    if (!block.IsBreaker)
                    {
                        if (pipes > MaxNodesPerDiameter(block.BLOCK.PipeDiameter))
                        {
                            Log.Out("Distance to big {0} + {1} > {2}",
                                pipes, pipes, MaxNodesPerDiameter(block.BLOCK.PipeDiameter));
                            return false;
                        }
                        // Log.Out("Block is not a breaker");
                    }

                    if (block.BLOCK == null) return false;
                    if (NB[face].BLOCK == null)
                    {
                        Log.Warning("Face neighbour is null?");
                        return false;
                    }

                    byte srcdim = block.BLOCK.PipeDiameter;
                    byte dstdim = NB[face].BLOCK.PipeDiameter;
                    if ((srcdim & dstdim) == 0)
                    {
                        Log.Out("Pipe diameter not matching {0} vs {1}",
                            srcdim, dstdim);
                        return false;
                    }

                    // if (block.BLOCK.PipeDiameter != 0)
                    // {
                    //     if (NB[face].BLOCK.PipeDiameter != 0)
                    //     {
                    //         if (block.BLOCK.PipeDiameter !=
                    //             NB[face].BLOCK.PipeDiameter)
                    //         {
                    //         }
                    //     }
                    // 
                    // }

                    // Found connector
                    connections += 1;
                }
                // Only one face would connect
                else if (a || b)
                {
                    // Log.Out("Only one connector {0} or {1}", a, b);
                    return ignoreBlockers;
                }
                // else continue; // No connectors
            }

            // Log.Out("Allowed now {0}", pipes);

            return true;

        }

    }
}
