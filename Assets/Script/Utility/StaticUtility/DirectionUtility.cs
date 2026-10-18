using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TraversalMethod { Walking, Teleporting }

public enum Direction { N, E, S, W}
public static class DirectionUtility
{
        public static Direction[] AllDirections = new Direction[] { Direction.N, Direction.E, Direction.S, Direction.W };
    public static Direction GetOppositeDirection(Direction originalDir)
    {
        switch (originalDir)
        {
            default:
            case Direction.N:
                return Direction.S;
            //case Direction.NE:
            //    return Direction.SW;
            case Direction.E:
                return Direction.W;
            //case Direction.SE:
            //    return Direction.NW;
            case Direction.S:
                return Direction.N;
            //case Direction.SW:
            //    return Direction.NE;
            case Direction.W:
                return Direction.E;
            //case Direction.NW:
            //    return Direction.SE;
        }
    }
    public static float GetAngleOfDirection(Direction direction)
    {
        switch (direction)
        {
            default:
            case Direction.N:
                return 0f;
            case Direction.E:
                return 90f;
            case Direction.S:
                return 180f;
            case Direction.W:
                return 270f;
        }
    }
    public static Vector2Int GetPositionInDirection(Direction direction, Vector2Int origin)
    {
        switch (direction)
        {
            default:
            case Direction.N:
                return new Vector2Int(origin.x, origin.y +1);
            case Direction.E:
                return new Vector2Int(origin.x+1, origin.y);
            case Direction.S:
                return new Vector2Int(origin.x, origin.y -1);
            case Direction.W:
                return new Vector2Int(origin.x-1, origin.y);
        }
    }
}
