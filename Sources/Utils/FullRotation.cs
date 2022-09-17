using NodeFacilitator;
using System;
using UnityEngine;

public static class FullRotation
{

    //    up, left, forward,
    //    down, right, back,
    //    invalid = byte.MaxValue

    public enum Face : byte
    {
        // up, left, forward,
        // down, right, back,
        // invalid = byte.MaxValue

        up,
        down,
        forward,
        left,
        back,
        right,

        invalid = byte.MaxValue
    };

    // ToDo: re-use enum (need to adjust a lot of code)
    // public static Face BlockFaceToFace(BlockFace face)
    // {
    //     switch (face)
    //     {
    //         case BlockFace.Top: return Face.up;
    //         case BlockFace.Bottom: return Face.down;
    //         case BlockFace.North: return Face.forward;
    //         case BlockFace.East: return Face.right;
    //         case BlockFace.South: return Face.back;
    //         case BlockFace.West: return Face.left;
    //     }
    //     return (Face)0;
    // }

    public static Vector3i[] Vector = new Vector3i[6]
    {
        Vector3i.up, Vector3i.down, Vector3i.forward,
        Vector3i.left, Vector3i.back, Vector3i.right
        //Vector3i.up, Vector3i.left, Vector3i.forward,
        //Vector3i.down, Vector3i.right, Vector3i.back
    };

    public static byte Mirror(int face)
    {
        switch ((Face)face)
        {
            case Face.up: return (byte)Face.down;
            case Face.down: return (byte)Face.up;
            case Face.left: return (byte)Face.right;
            case Face.right: return (byte)Face.left;
            case Face.forward: return (byte)Face.back;
            case Face.back: return (byte)Face.forward;
            default: throw new ArgumentOutOfRangeException("face");
        }
    }

    public static Vector3i GetReach(int rotation, ReachConfig reach)
    {
        switch (rotation)
        {
            case 0: return reach.BlockReach;
            case 1: return reach.BlockReach;
            case 2: return reach.BlockReach;
            case 3: return reach.BlockReach;
            case 4: return reach.ReachUpside;
            case 5: return reach.ReachUpside;
            case 6: return reach.ReachUpside;
            case 7: return reach.ReachUpside;
            case 8: return reach.ReachSideway;
            case 9: return reach.ReachSideway;
            case 10: return reach.ReachSideway;
            case 11: return reach.ReachSideway;
            case 12: return reach.ReachSideway;
            case 13: return reach.ReachSideway;
            case 14: return reach.ReachSideway;
            case 15: return reach.ReachSideway;
            case 16: return reach.ReachSideway;
            case 17: return reach.ReachSideway;
            case 18: return reach.ReachSideway;
            case 19: return reach.ReachSideway;
            case 20: return reach.ReachSideway;
            case 21: return reach.ReachSideway;
            case 22: return reach.ReachSideway;
            case 23: return reach.ReachSideway;
            default: throw new ArgumentOutOfRangeException("rotation");
        }
    }

    public static Vector3i GetOffset(int rotation, ReachConfig reach)
    {
        switch (rotation)
        {
            case 0: return reach.ReachOffset;
            case 1: return reach.ReachOffset;
            case 2: return reach.ReachOffset;
            case 3: return reach.ReachOffset;
            case 4: return reach.OffsetUpside;
            case 5: return reach.OffsetUpside;
            case 6: return reach.OffsetUpside;
            case 7: return reach.OffsetUpside;
            case 8: return reach.OffsetSideway;
            case 9: return reach.OffsetSideway;
            case 10: return reach.OffsetSideway;
            case 11: return reach.OffsetSideway;
            case 12: return reach.OffsetSideway;
            case 13: return reach.OffsetSideway;
            case 14: return reach.OffsetSideway;
            case 15: return reach.OffsetSideway;
            case 16: return reach.OffsetSideway;
            case 17: return reach.OffsetSideway;
            case 18: return reach.OffsetSideway;
            case 19: return reach.OffsetSideway;
            case 20: return reach.OffsetSideway;
            case 21: return reach.OffsetSideway;
            case 22: return reach.OffsetSideway;
            case 23: return reach.OffsetSideway;
            default: throw new ArgumentOutOfRangeException("rotation");
        }
    }

    // Generic rotation function (for full axis rotations only)
    // This does not support any 45 degree rotations (beware!)
    // Function has been unit tested to make sure it is correct!
    public static Vector3i Rotate(int rotation, Vector3i vec)
    {
        switch (rotation)
        {
            case 0: return vec;
            case 1: return new Vector3i(vec.z, vec.y, -vec.x);
            case 2: return new Vector3i(-vec.x, vec.y, -vec.z);
            case 3: return new Vector3i(-vec.z, vec.y, vec.x);
            case 4: return new Vector3i(-vec.x, -vec.y, vec.z);
            case 5: return new Vector3i(-vec.z, -vec.y, -vec.x);
            case 6: return new Vector3i(vec.x, -vec.y, -vec.z);
            case 7: return new Vector3i(vec.z, -vec.y, vec.x);
            case 8: return new Vector3i(-vec.x, vec.z, vec.y);
            case 9: return new Vector3i(-vec.z, -vec.x, vec.y);
            case 10: return new Vector3i(vec.x, -vec.z, vec.y);
            case 11: return new Vector3i(vec.z, vec.x, vec.y);
            case 12: return new Vector3i(-vec.y, vec.x, vec.z);
            case 13: return new Vector3i(-vec.y, vec.z, -vec.x);
            case 14: return new Vector3i(-vec.y, -vec.x, -vec.z);
            case 15: return new Vector3i(-vec.y, -vec.z, vec.x);
            case 16: return new Vector3i(vec.x, vec.z, -vec.y);
            case 17: return new Vector3i(vec.z, -vec.x, -vec.y);
            case 18: return new Vector3i(-vec.x, -vec.z, -vec.y);
            case 19: return new Vector3i(-vec.z, vec.x, -vec.y);
            case 20: return new Vector3i(vec.y, -vec.x, vec.z);
            case 21: return new Vector3i(vec.y, -vec.z, -vec.x);
            case 22: return new Vector3i(vec.y, vec.x, -vec.z);
            case 23: return new Vector3i(vec.y, vec.z, vec.x);
            default: throw new ArgumentOutOfRangeException("rotation");
        }
    }

    public static Vector3 Rotate(int rotation, Vector3 vec)
    {
        switch (rotation)
        {
            case 0: return vec;
            case 1: return new Vector3(vec.z, vec.y, -vec.x);
            case 2: return new Vector3(-vec.x, vec.y, -vec.z);
            case 3: return new Vector3(-vec.z, vec.y, vec.x);
            case 4: return new Vector3(-vec.x, -vec.y, vec.z);
            case 5: return new Vector3(-vec.z, -vec.y, -vec.x);
            case 6: return new Vector3(vec.x, -vec.y, -vec.z);
            case 7: return new Vector3(vec.z, -vec.y, vec.x);
            case 8: return new Vector3(-vec.x, vec.z, vec.y);
            case 9: return new Vector3(-vec.z, -vec.x, vec.y);
            case 10: return new Vector3(vec.x, -vec.z, vec.y);
            case 11: return new Vector3(vec.z, vec.x, vec.y);
            case 12: return new Vector3(-vec.y, vec.x, vec.z);
            case 13: return new Vector3(-vec.y, vec.z, -vec.x);
            case 14: return new Vector3(-vec.y, -vec.x, -vec.z);
            case 15: return new Vector3(-vec.y, -vec.z, vec.x);
            case 16: return new Vector3(vec.x, vec.z, -vec.y);
            case 17: return new Vector3(vec.z, -vec.x, -vec.y);
            case 18: return new Vector3(-vec.x, -vec.z, -vec.y);
            case 19: return new Vector3(-vec.z, vec.x, -vec.y);
            case 20: return new Vector3(vec.y, -vec.x, vec.z);
            case 21: return new Vector3(vec.y, -vec.z, -vec.x);
            case 22: return new Vector3(vec.y, vec.x, -vec.z);
            case 23: return new Vector3(vec.y, vec.z, vec.x);
            default: throw new ArgumentOutOfRangeException("rotation");
        }
    }

    public static Vector3i InvRotate(int rotation, Vector3i vec)
    {
        switch (rotation)
        {
            case 0: return vec;
            case 1: return new Vector3i(-vec.z, vec.y, vec.x);
            case 2: return new Vector3i(-vec.x, vec.y, -vec.z);
            case 3: return new Vector3i(vec.z, vec.y, -vec.x);
            case 4: return new Vector3i(-vec.x, -vec.y, vec.z);
            case 5: return new Vector3i(-vec.z, -vec.y, -vec.x);
            case 6: return new Vector3i(vec.x, -vec.y, -vec.z);
            case 7: return new Vector3i(vec.z, -vec.y, vec.x);
            case 8: return new Vector3i(-vec.x, vec.z, vec.y);
            case 9: return new Vector3i(-vec.y, vec.z, -vec.x);
            case 10: return new Vector3i(vec.x, vec.z, -vec.y);
            case 11: return new Vector3i(vec.y, vec.z, vec.x);
            case 12: return new Vector3i(vec.y, -vec.x, vec.z);
            case 13: return new Vector3i(-vec.z, -vec.x, vec.y);
            case 14: return new Vector3i(-vec.y, -vec.x, -vec.z);
            case 15: return new Vector3i(vec.z, -vec.x, -vec.y);
            case 16: return new Vector3i(vec.x, -vec.z, vec.y);
            case 17: return new Vector3i(-vec.y, -vec.z, vec.x);
            case 18: return new Vector3i(-vec.x, -vec.z, -vec.y);
            case 19: return new Vector3i(vec.y, -vec.z, -vec.x);
            case 20: return new Vector3i(-vec.y, vec.x, vec.z);
            case 21: return new Vector3i(-vec.z, vec.x, -vec.y);
            case 22: return new Vector3i(vec.y, vec.x, -vec.z);
            case 23: return new Vector3i(vec.z, vec.x, vec.y);
            default: throw new ArgumentOutOfRangeException("rotation");
        }
    }

    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
    /*
    public static Vector3i GetVector(int face, int rotation)
    {
        switch (face)
        {
            case (byte)Face.up:
                switch (rotation)
                {
                    case 0: return Vector3i.up;
                    case 1: return Vector3i.up;
                    case 2: return Vector3i.up;
                    case 3: return Vector3i.up;
                    case 4: return Vector3i.down;
                    case 5: return Vector3i.down;
                    case 6: return Vector3i.down;
                    case 7: return Vector3i.down;
                    case 8: return Vector3i.forward;
                    case 9: return Vector3i.forward;
                    case 10: return Vector3i.forward;
                    case 11: return Vector3i.forward;
                    case 12: return Vector3i.left;
                    case 13: return Vector3i.left;
                    case 14: return Vector3i.left;
                    case 15: return Vector3i.left;
                    case 16: return Vector3i.back;
                    case 17: return Vector3i.back;
                    case 18: return Vector3i.back;
                    case 19: return Vector3i.back;
                    case 20: return Vector3i.right;
                    case 21: return Vector3i.right;
                    case 22: return Vector3i.right;
                    case 23: return Vector3i.right;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.left:
                switch (rotation)
                {
                    case 0: return Vector3i.left;
                    case 1: return Vector3i.forward;
                    case 2: return Vector3i.right;
                    case 3: return Vector3i.back;
                    case 4: return Vector3i.right;
                    case 5: return Vector3i.forward;
                    case 6: return Vector3i.left;
                    case 7: return Vector3i.back;
                    case 8: return Vector3i.right;
                    case 9: return Vector3i.up;
                    case 10: return Vector3i.left;
                    case 11: return Vector3i.down;
                    case 12: return Vector3i.down;
                    case 13: return Vector3i.forward;
                    case 14: return Vector3i.up;
                    case 15: return Vector3i.back;
                    case 16: return Vector3i.left;
                    case 17: return Vector3i.up;
                    case 18: return Vector3i.right;
                    case 19: return Vector3i.down;
                    case 20: return Vector3i.up;
                    case 21: return Vector3i.forward;
                    case 22: return Vector3i.down;
                    case 23: return Vector3i.back;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.forward:
                switch (rotation)
                {
                    case 0: return Vector3i.forward;
                    case 1: return Vector3i.right;
                    case 2: return Vector3i.back;
                    case 3: return Vector3i.left;
                    case 4: return Vector3i.forward;
                    case 5: return Vector3i.left;
                    case 6: return Vector3i.back;
                    case 7: return Vector3i.right;
                    case 8: return Vector3i.up;
                    case 9: return Vector3i.left;
                    case 10: return Vector3i.down;
                    case 11: return Vector3i.right;
                    case 12: return Vector3i.forward;
                    case 13: return Vector3i.up;
                    case 14: return Vector3i.back;
                    case 15: return Vector3i.down;
                    case 16: return Vector3i.up;
                    case 17: return Vector3i.right;
                    case 18: return Vector3i.down;
                    case 19: return Vector3i.left;
                    case 20: return Vector3i.forward;
                    case 21: return Vector3i.down;
                    case 22: return Vector3i.back;
                    case 23: return Vector3i.up;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.down:
                switch (rotation)
                {
                    case 0: return Vector3i.down;
                    case 1: return Vector3i.down;
                    case 2: return Vector3i.down;
                    case 3: return Vector3i.down;
                    case 4: return Vector3i.up;
                    case 5: return Vector3i.up;
                    case 6: return Vector3i.up;
                    case 7: return Vector3i.up;
                    case 8: return Vector3i.back;
                    case 9: return Vector3i.back;
                    case 10: return Vector3i.back;
                    case 11: return Vector3i.back;
                    case 12: return Vector3i.right;
                    case 13: return Vector3i.right;
                    case 14: return Vector3i.right;
                    case 15: return Vector3i.right;
                    case 16: return Vector3i.forward;
                    case 17: return Vector3i.forward;
                    case 18: return Vector3i.forward;
                    case 19: return Vector3i.forward;
                    case 20: return Vector3i.left;
                    case 21: return Vector3i.left;
                    case 22: return Vector3i.left;
                    case 23: return Vector3i.left;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.right:
                switch (rotation)
                {
                    case 0: return Vector3i.right;
                    case 1: return Vector3i.back;
                    case 2: return Vector3i.left;
                    case 3: return Vector3i.forward;
                    case 4: return Vector3i.left;
                    case 5: return Vector3i.back;
                    case 6: return Vector3i.right;
                    case 7: return Vector3i.forward;
                    case 8: return Vector3i.left;
                    case 9: return Vector3i.down;
                    case 10: return Vector3i.right;
                    case 11: return Vector3i.up;
                    case 12: return Vector3i.up;
                    case 13: return Vector3i.back;
                    case 14: return Vector3i.down;
                    case 15: return Vector3i.forward;
                    case 16: return Vector3i.right;
                    case 17: return Vector3i.down;
                    case 18: return Vector3i.left;
                    case 19: return Vector3i.up;
                    case 20: return Vector3i.down;
                    case 21: return Vector3i.back;
                    case 22: return Vector3i.up;
                    case 23: return Vector3i.forward;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.back:
                switch (rotation)
                {
                    case 0: return Vector3i.back;
                    case 1: return Vector3i.left;
                    case 2: return Vector3i.forward;
                    case 3: return Vector3i.right;
                    case 4: return Vector3i.back;
                    case 5: return Vector3i.right;
                    case 6: return Vector3i.forward;
                    case 7: return Vector3i.left;
                    case 8: return Vector3i.down;
                    case 9: return Vector3i.right;
                    case 10: return Vector3i.up;
                    case 11: return Vector3i.left;
                    case 12: return Vector3i.back;
                    case 13: return Vector3i.down;
                    case 14: return Vector3i.forward;
                    case 15: return Vector3i.up;
                    case 16: return Vector3i.down;
                    case 17: return Vector3i.left;
                    case 18: return Vector3i.up;
                    case 19: return Vector3i.right;
                    case 20: return Vector3i.back;
                    case 21: return Vector3i.up;
                    case 22: return Vector3i.forward;
                    case 23: return Vector3i.down;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
        }
        throw new ArgumentOutOfRangeException("face");
    }
    */

    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
    public static byte GetFace(byte face, byte rotation)
    {
        if (rotation == 0)
            return face;
        switch (face)
        {
            case (byte)Face.up:
                switch (rotation)
                {
                    case 0: return (byte)Face.up;
                    case 1: return (byte)Face.up;
                    case 2: return (byte)Face.up;
                    case 3: return (byte)Face.up;
                    case 4: return (byte)Face.down;
                    case 5: return (byte)Face.down;
                    case 6: return (byte)Face.down;
                    case 7: return (byte)Face.down;
                    case 8: return (byte)Face.forward;
                    case 9: return (byte)Face.forward;
                    case 10: return (byte)Face.forward;
                    case 11: return (byte)Face.forward;
                    case 12: return (byte)Face.left;
                    case 13: return (byte)Face.left;
                    case 14: return (byte)Face.left;
                    case 15: return (byte)Face.left;
                    case 16: return (byte)Face.back;
                    case 17: return (byte)Face.back;
                    case 18: return (byte)Face.back;
                    case 19: return (byte)Face.back;
                    case 20: return (byte)Face.right;
                    case 21: return (byte)Face.right;
                    case 22: return (byte)Face.right;
                    case 23: return (byte)Face.right;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.left:
                switch (rotation)
                {
                    case 0: return (byte)Face.left;
                    case 1: return (byte)Face.forward;
                    case 2: return (byte)Face.right;
                    case 3: return (byte)Face.back;
                    case 4: return (byte)Face.right;
                    case 5: return (byte)Face.forward;
                    case 6: return (byte)Face.left;
                    case 7: return (byte)Face.back;
                    case 8: return (byte)Face.right;
                    case 9: return (byte)Face.up;
                    case 10: return (byte)Face.left;
                    case 11: return (byte)Face.down;
                    case 12: return (byte)Face.down;
                    case 13: return (byte)Face.forward;
                    case 14: return (byte)Face.up;
                    case 15: return (byte)Face.back;
                    case 16: return (byte)Face.left;
                    case 17: return (byte)Face.up;
                    case 18: return (byte)Face.right;
                    case 19: return (byte)Face.down;
                    case 20: return (byte)Face.up;
                    case 21: return (byte)Face.forward;
                    case 22: return (byte)Face.down;
                    case 23: return (byte)Face.back;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.forward:
                switch (rotation)
                {
                    case 0: return (byte)Face.forward;
                    case 1: return (byte)Face.right;
                    case 2: return (byte)Face.back;
                    case 3: return (byte)Face.left;
                    case 4: return (byte)Face.forward;
                    case 5: return (byte)Face.left;
                    case 6: return (byte)Face.back;
                    case 7: return (byte)Face.right;
                    case 8: return (byte)Face.up;
                    case 9: return (byte)Face.left;
                    case 10: return (byte)Face.down;
                    case 11: return (byte)Face.right;
                    case 12: return (byte)Face.forward;
                    case 13: return (byte)Face.up;
                    case 14: return (byte)Face.back;
                    case 15: return (byte)Face.down;
                    case 16: return (byte)Face.up;
                    case 17: return (byte)Face.right;
                    case 18: return (byte)Face.down;
                    case 19: return (byte)Face.left;
                    case 20: return (byte)Face.forward;
                    case 21: return (byte)Face.down;
                    case 22: return (byte)Face.back;
                    case 23: return (byte)Face.up;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.down:
                switch (rotation)
                {
                    case 0: return (byte)Face.down;
                    case 1: return (byte)Face.down;
                    case 2: return (byte)Face.down;
                    case 3: return (byte)Face.down;
                    case 4: return (byte)Face.up;
                    case 5: return (byte)Face.up;
                    case 6: return (byte)Face.up;
                    case 7: return (byte)Face.up;
                    case 8: return (byte)Face.back;
                    case 9: return (byte)Face.back;
                    case 10: return (byte)Face.back;
                    case 11: return (byte)Face.back;
                    case 12: return (byte)Face.right;
                    case 13: return (byte)Face.right;
                    case 14: return (byte)Face.right;
                    case 15: return (byte)Face.right;
                    case 16: return (byte)Face.forward;
                    case 17: return (byte)Face.forward;
                    case 18: return (byte)Face.forward;
                    case 19: return (byte)Face.forward;
                    case 20: return (byte)Face.left;
                    case 21: return (byte)Face.left;
                    case 22: return (byte)Face.left;
                    case 23: return (byte)Face.left;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.right:
                switch (rotation)
                {
                    case 0: return (byte)Face.right;
                    case 1: return (byte)Face.back;
                    case 2: return (byte)Face.left;
                    case 3: return (byte)Face.forward;
                    case 4: return (byte)Face.left;
                    case 5: return (byte)Face.back;
                    case 6: return (byte)Face.right;
                    case 7: return (byte)Face.forward;
                    case 8: return (byte)Face.left;
                    case 9: return (byte)Face.down;
                    case 10: return (byte)Face.right;
                    case 11: return (byte)Face.up;
                    case 12: return (byte)Face.up;
                    case 13: return (byte)Face.back;
                    case 14: return (byte)Face.down;
                    case 15: return (byte)Face.forward;
                    case 16: return (byte)Face.right;
                    case 17: return (byte)Face.down;
                    case 18: return (byte)Face.left;
                    case 19: return (byte)Face.up;
                    case 20: return (byte)Face.down;
                    case 21: return (byte)Face.back;
                    case 22: return (byte)Face.up;
                    case 23: return (byte)Face.forward;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.back:
                switch (rotation)
                {
                    case 0: return (byte)Face.back;
                    case 1: return (byte)Face.left;
                    case 2: return (byte)Face.forward;
                    case 3: return (byte)Face.right;
                    case 4: return (byte)Face.back;
                    case 5: return (byte)Face.right;
                    case 6: return (byte)Face.forward;
                    case 7: return (byte)Face.left;
                    case 8: return (byte)Face.down;
                    case 9: return (byte)Face.right;
                    case 10: return (byte)Face.up;
                    case 11: return (byte)Face.left;
                    case 12: return (byte)Face.back;
                    case 13: return (byte)Face.down;
                    case 14: return (byte)Face.forward;
                    case 15: return (byte)Face.up;
                    case 16: return (byte)Face.down;
                    case 17: return (byte)Face.left;
                    case 18: return (byte)Face.up;
                    case 19: return (byte)Face.right;
                    case 20: return (byte)Face.back;
                    case 21: return (byte)Face.up;
                    case 22: return (byte)Face.forward;
                    case 23: return (byte)Face.down;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
        }
        throw new ArgumentOutOfRangeException("face");
    }

    //public static byte InvSide(byte side, byte rotation)
    //{
    //    side += 1; // First Face is Up (skip that)
    //    if (side > 2) side += 1; // And skip down face
    //    return InvFace(side, rotation);
    //}

    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
    public static byte InvFace(byte face, byte rotation)
    {
        if (rotation == 0)
            return face;
        switch (face)
        {
            case (byte)Face.up:
                switch (rotation)
                {
                    case 0: return (byte)Face.up;
                    case 1: return (byte)Face.up;
                    case 2: return (byte)Face.up;
                    case 3: return (byte)Face.up;
                    case 4: return (byte)Face.down;
                    case 5: return (byte)Face.down;
                    case 6: return (byte)Face.down;
                    case 7: return (byte)Face.down;
                    case 8: return (byte)Face.forward;
                    case 9: return (byte)Face.left;
                    case 10: return (byte)Face.back;
                    case 11: return (byte)Face.right;
                    case 12: return (byte)Face.right;
                    case 13: return (byte)Face.forward;
                    case 14: return (byte)Face.left;
                    case 15: return (byte)Face.back;
                    case 16: return (byte)Face.forward;
                    case 17: return (byte)Face.left;
                    case 18: return (byte)Face.back;
                    case 19: return (byte)Face.right;
                    case 20: return (byte)Face.left;
                    case 21: return (byte)Face.back;
                    case 22: return (byte)Face.right;
                    case 23: return (byte)Face.forward;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.left:
                switch (rotation)
                {
                    case 0: return (byte)Face.left;
                    case 1: return (byte)Face.back;
                    case 2: return (byte)Face.right;
                    case 3: return (byte)Face.forward;
                    case 4: return (byte)Face.right;
                    case 5: return (byte)Face.forward;
                    case 6: return (byte)Face.left;
                    case 7: return (byte)Face.back;
                    case 8: return (byte)Face.right;
                    case 9: return (byte)Face.forward;
                    case 10: return (byte)Face.left;
                    case 11: return (byte)Face.back;
                    case 12: return (byte)Face.up;
                    case 13: return (byte)Face.up;
                    case 14: return (byte)Face.up;
                    case 15: return (byte)Face.up;
                    case 16: return (byte)Face.left;
                    case 17: return (byte)Face.back;
                    case 18: return (byte)Face.right;
                    case 19: return (byte)Face.forward;
                    case 20: return (byte)Face.down;
                    case 21: return (byte)Face.down;
                    case 22: return (byte)Face.down;
                    case 23: return (byte)Face.down;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.forward:
                switch (rotation)
                {
                    case 0: return (byte)Face.forward;
                    case 1: return (byte)Face.left;
                    case 2: return (byte)Face.back;
                    case 3: return (byte)Face.right;
                    case 4: return (byte)Face.forward;
                    case 5: return (byte)Face.left;
                    case 6: return (byte)Face.back;
                    case 7: return (byte)Face.right;
                    case 8: return (byte)Face.up;
                    case 9: return (byte)Face.up;
                    case 10: return (byte)Face.up;
                    case 11: return (byte)Face.up;
                    case 12: return (byte)Face.forward;
                    case 13: return (byte)Face.left;
                    case 14: return (byte)Face.back;
                    case 15: return (byte)Face.right;
                    case 16: return (byte)Face.down;
                    case 17: return (byte)Face.down;
                    case 18: return (byte)Face.down;
                    case 19: return (byte)Face.down;
                    case 20: return (byte)Face.forward;
                    case 21: return (byte)Face.left;
                    case 22: return (byte)Face.back;
                    case 23: return (byte)Face.right;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.down:
                switch (rotation)
                {
                    case 0: return (byte)Face.down;
                    case 1: return (byte)Face.down;
                    case 2: return (byte)Face.down;
                    case 3: return (byte)Face.down;
                    case 4: return (byte)Face.up;
                    case 5: return (byte)Face.up;
                    case 6: return (byte)Face.up;
                    case 7: return (byte)Face.up;
                    case 8: return (byte)Face.back;
                    case 9: return (byte)Face.right;
                    case 10: return (byte)Face.forward;
                    case 11: return (byte)Face.left;
                    case 12: return (byte)Face.left;
                    case 13: return (byte)Face.back;
                    case 14: return (byte)Face.right;
                    case 15: return (byte)Face.forward;
                    case 16: return (byte)Face.back;
                    case 17: return (byte)Face.right;
                    case 18: return (byte)Face.forward;
                    case 19: return (byte)Face.left;
                    case 20: return (byte)Face.right;
                    case 21: return (byte)Face.forward;
                    case 22: return (byte)Face.left;
                    case 23: return (byte)Face.back;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.right:
                switch (rotation)
                {
                    case 0: return (byte)Face.right;
                    case 1: return (byte)Face.forward;
                    case 2: return (byte)Face.left;
                    case 3: return (byte)Face.back;
                    case 4: return (byte)Face.left;
                    case 5: return (byte)Face.back;
                    case 6: return (byte)Face.right;
                    case 7: return (byte)Face.forward;
                    case 8: return (byte)Face.left;
                    case 9: return (byte)Face.back;
                    case 10: return (byte)Face.right;
                    case 11: return (byte)Face.forward;
                    case 12: return (byte)Face.down;
                    case 13: return (byte)Face.down;
                    case 14: return (byte)Face.down;
                    case 15: return (byte)Face.down;
                    case 16: return (byte)Face.right;
                    case 17: return (byte)Face.forward;
                    case 18: return (byte)Face.left;
                    case 19: return (byte)Face.back;
                    case 20: return (byte)Face.up;
                    case 21: return (byte)Face.up;
                    case 22: return (byte)Face.up;
                    case 23: return (byte)Face.up;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case (byte)Face.back:
                switch (rotation)
                {
                    case 0: return (byte)Face.back;
                    case 1: return (byte)Face.right;
                    case 2: return (byte)Face.forward;
                    case 3: return (byte)Face.left;
                    case 4: return (byte)Face.back;
                    case 5: return (byte)Face.right;
                    case 6: return (byte)Face.forward;
                    case 7: return (byte)Face.left;
                    case 8: return (byte)Face.down;
                    case 9: return (byte)Face.down;
                    case 10: return (byte)Face.down;
                    case 11: return (byte)Face.down;
                    case 12: return (byte)Face.back;
                    case 13: return (byte)Face.right;
                    case 14: return (byte)Face.forward;
                    case 15: return (byte)Face.left;
                    case 16: return (byte)Face.up;
                    case 17: return (byte)Face.up;
                    case 18: return (byte)Face.up;
                    case 19: return (byte)Face.up;
                    case 20: return (byte)Face.back;
                    case 21: return (byte)Face.right;
                    case 22: return (byte)Face.forward;
                    case 23: return (byte)Face.left;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
        }
        throw new ArgumentOutOfRangeException("face");
    }

    // Lookup table to rotate another rotation index
    // ToDo: maybe we can optimize other function too
    private static readonly byte[] RotationMapping = new byte[]
    {
         0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
         1,  2,  3,  0,  5,  6,  7,  4,  9, 10, 11,  8, 13, 14, 15, 12, 17, 18, 19, 16, 21, 22, 23, 20,
         2,  3,  0,  1,  6,  7,  4,  5, 10, 11,  8,  9, 14, 15, 12, 13, 18, 19, 16, 17, 22, 23, 20, 21,
         3,  0,  1,  2,  7,  4,  5,  6, 11,  8,  9, 10, 15, 12, 13, 14, 19, 16, 17, 18, 23, 20, 21, 22,
         4,  7,  6,  5,  0,  3,  2,  1, 16, 19, 18, 17, 20, 23, 22, 21,  8, 11, 10,  9, 12, 15, 14, 13,
         5,  4,  7,  6,  1,  0,  3,  2, 17, 16, 19, 18, 21, 20, 23, 22,  9,  8, 11, 10, 13, 12, 15, 14,
         6,  5,  4,  7,  2,  1,  0,  3, 18, 17, 16, 19, 22, 21, 20, 23, 10,  9,  8, 11, 14, 13, 12, 15,
         7,  6,  5,  4,  3,  2,  1,  0, 19, 18, 17, 16, 23, 22, 21, 20, 11, 10,  9,  8, 15, 14, 13, 12,
         8, 23, 16, 13, 10, 15, 18, 21,  0, 12,  4, 20,  9,  3, 19,  5,  2, 22,  6, 14, 11,  7, 17,  1,
         9, 20, 17, 14, 11, 12, 19, 22,  1, 13,  5, 21, 10,  0, 16,  6,  3, 23,  7, 15,  8,  4, 18,  2,
        10, 21, 18, 15,  8, 13, 16, 23,  2, 14,  6, 22, 11,  1, 17,  7,  0, 20,  4, 12,  9,  5, 19,  3,
        11, 22, 19, 12,  9, 14, 17, 20,  3, 15,  7, 23,  8,  2, 18,  4,  1, 21,  5, 13, 10,  6, 16,  0,
        12, 11, 22, 19, 20,  9, 14, 17, 23,  3, 15,  7,  4,  8,  2, 18, 13,  1, 21,  5,  0, 10,  6, 16,
        13,  8, 23, 16, 21, 10, 15, 18, 20,  0, 12,  4,  5,  9,  3, 19, 14,  2, 22,  6,  1, 11,  7, 17,
        14,  9, 20, 17, 22, 11, 12, 19, 21,  1, 13,  5,  6, 10,  0, 16, 15,  3, 23,  7,  2,  8,  4, 18,
        15, 10, 21, 18, 23,  8, 13, 16, 22,  2, 14,  6,  7, 11,  1, 17, 12,  0, 20,  4,  3,  9,  5, 19,
        16, 13,  8, 23, 18, 21, 10, 15,  4, 20,  0, 12, 19,  5,  9,  3,  6, 14,  2, 22, 17,  1, 11,  7,
        17, 14,  9, 20, 19, 22, 11, 12,  5, 21,  1, 13, 16,  6, 10,  0,  7, 15,  3, 23, 18,  2,  8,  4,
        18, 15, 10, 21, 16, 23,  8, 13,  6, 22,  2, 14, 17,  7, 11,  1,  4, 12,  0, 20, 19,  3,  9,  5,
        19, 12, 11, 22, 17, 20,  9, 14,  7, 23,  3, 15, 18,  4,  8,  2,  5, 13,  1, 21, 16,  0, 10,  6,
        20, 17, 14,  9, 12, 19, 22, 11, 13,  5, 21,  1,  0, 16,  6, 10, 23,  7, 15,  3,  4, 18,  2,  8,
        21, 18, 15, 10, 13, 16, 23,  8, 14,  6, 22,  2,  1, 17,  7, 11, 20,  4, 12,  0,  5, 19,  3,  9,
        22, 19, 12, 11, 14, 17, 20,  9, 15,  7, 23,  3,  2, 18,  4,  8, 21,  5, 13,  1,  6, 16,  0, 10,
        23, 16, 13,  8, 15, 18, 21, 10, 12,  4, 20,  0,  3, 19,  5,  9, 22,  6, 14,  2,  7, 17,  1, 11
    };

    public static byte Rotate(byte rotation1, byte rotation2)
    {
        if (rotation1 > 23) throw new ArgumentOutOfRangeException("rotation1");
        if (rotation2 > 23) throw new ArgumentOutOfRangeException("rotation2");
        return RotationMapping[rotation1 * 24 + rotation2];
    }

    // Useful helper function (debugging only)
    public static string VectorToString(Vector3 vector)
    {
        if (vector == Vector3i.up) return "up";
        if (vector == Vector3i.down) return "down";
        if (vector == Vector3i.left) return "left";
        if (vector == Vector3i.right) return "right";
        if (vector == Vector3i.forward) return "forward";
        if (vector == Vector3i.back) return "back";
        return vector.ToString();
    }

    // Useful helper function (debugging only)
    public static byte VectorToFace(Vector3 vector)
    {
        if (vector == Vector3i.up) return (byte)Face.up;
        if (vector == Vector3i.down) return (byte)Face.down;
        if (vector == Vector3i.left) return (byte)Face.left;
        if (vector == Vector3i.right) return (byte)Face.right;
        if (vector == Vector3i.forward) return (byte)Face.forward;
        if (vector == Vector3i.back) return (byte)Face.back;
        return (byte)Face.invalid;
    }

    // Useful helper function (debugging only)
    public static string FaceToString(int face)
    {
        if (face == (byte)Face.up) return "up";
        if (face == (byte)Face.down) return "down";
        if (face == (byte)Face.left) return "left";
        if (face == (byte)Face.right) return "right";
        if (face == (byte)Face.forward) return "forward";
        if (face == (byte)Face.back) return "back";
        return face.ToString();
    }

}

