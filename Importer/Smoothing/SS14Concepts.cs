using System;
using SixLabors.ImageSharp;
using Importer.Directions;

namespace RSI.Smoothing;

/// <summary>
/// Flags for SS14 iconsmoothing states.
/// </summary>
public enum SS14IndexFlags : int
{
    CCW = 1,
    Diagonal = 2,
    CW = 4,
}

public static class SS14IndexFlagsMethods
{
    public static DirectionFlags ToDirectionFlags(this SS14IndexFlags idx, QuadSubtileIndex rotation)
    {
        int idx2 = (int) idx;
        bool ccw = (idx2 & 1) != 0;
        bool diagonal = (idx2 & 2) != 0;
        bool cw = (idx2 & 4) != 0;

        var res = (DirectionFlags) 0;
        var ccwDF = (DirectionFlags) 0;
        var dDF = (DirectionFlags) 0;
        var cwDF = (DirectionFlags) 0;

        switch (rotation)
        {
            case QuadSubtileIndex.NorthWest:
                ccwDF = DirectionFlags.West;
                dDF = DirectionFlags.NorthWest;
                cwDF = DirectionFlags.North;
                break;
            case QuadSubtileIndex.NorthEast:
                ccwDF = DirectionFlags.North;
                dDF = DirectionFlags.NorthEast;
                cwDF = DirectionFlags.East;
                break;
            case QuadSubtileIndex.SouthEast:
                ccwDF = DirectionFlags.East;
                dDF = DirectionFlags.SouthEast;
                cwDF = DirectionFlags.South;
                break;
            case QuadSubtileIndex.SouthWest:
                ccwDF = DirectionFlags.South;
                dDF = DirectionFlags.SouthWest;
                cwDF = DirectionFlags.West;
                break;
        }

        if (ccw)
            res |= ccwDF;
        if (diagonal)
            res |= dDF;
        if (cw)
            res |= cwDF;

        return res;
    }
}

