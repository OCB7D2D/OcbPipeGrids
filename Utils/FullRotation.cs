using System;
using UnityEngine;

public static class FullRotation
{

    //    up, left, forward,
    //    down, right, back,
    //    invalid = byte.MaxValue

    public enum Face : byte
    {
        up, left, forward,
        down, right, back,
        invalid = byte.MaxValue

        // up,
        // down,
        // forward,
        // left,
        // back,
        // right,
        // invalid = byte.MaxValue
    };

    // ToDo: re-use enum (need to adjust a lot of code)
    public static Face BlockFaceToFace(BlockFace face)
    {
        switch(face)
        {
            case BlockFace.Top: return Face.up;
            case BlockFace.Bottom: return Face.down;
            case BlockFace.North: return Face.forward;
            case BlockFace.West: return Face.right;
            case BlockFace.South: return Face.down;
            case BlockFace.East: return Face.left;
        }
        return (Face)0;
    }

    public static Vector3i[] Vector = new Vector3i[6]
    {
        // Vector3i.up, Vector3i.down, Vector3i.forward,
        // Vector3i.left, Vector3i.back, Vector3i.right
        Vector3i.up, Vector3i.left, Vector3i.forward,
        Vector3i.down, Vector3i.right, Vector3i.back
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


    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
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

    public static byte InvSide(byte side, byte rotation)
    {
        side += 1; // First Face is Up (skip that)
        if (side > 2) side += 1; // And skip down face
        return InvFace(side, rotation);
    }

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

