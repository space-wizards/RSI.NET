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
    All = 7,
}

public static class SS14IndexFlagsMethods
{
    public static TGFlags ToTGFlags(this SS14IndexFlags idx, QuadSubtileIndex rotation)
    {
        int idx2 = (int) idx;
        bool ccw = (idx2 & 1) != 0;
        bool diagonal = (idx2 & 2) != 0;
        bool cw = (idx2 & 4) != 0;

        var res = (TGFlags) 0;
        var ccwDF = (TGFlags) 0;
        var dDF = (TGFlags) 0;
        var cwDF = (TGFlags) 0;

        switch (rotation)
        {
            case QuadSubtileIndex.NorthWest:
                ccwDF = TGFlags.West;
                dDF = TGFlags.NorthWest;
                cwDF = TGFlags.North;
                break;
            case QuadSubtileIndex.NorthEast:
                ccwDF = TGFlags.North;
                dDF = TGFlags.NorthEast;
                cwDF = TGFlags.East;
                break;
            case QuadSubtileIndex.SouthEast:
                ccwDF = TGFlags.East;
                dDF = TGFlags.SouthEast;
                cwDF = TGFlags.South;
                break;
            case QuadSubtileIndex.SouthWest:
                ccwDF = TGFlags.South;
                dDF = TGFlags.SouthWest;
                cwDF = TGFlags.West;
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

