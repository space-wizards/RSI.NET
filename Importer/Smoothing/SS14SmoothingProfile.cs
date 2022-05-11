using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Importer.Directions;

namespace RSI.Smoothing;

/// <summary>
/// An SS14 smoothing profile is a "compressed" QuadSmoothingProfile, which only considers the states used by SS14 icon smoothing.
/// </summary>
public sealed class SS14SmoothingProfile : BaseSmoothingProfileMetrics
{
    public const int QuadSubtiles = QuadSmoothingProfile.QuadSubtiles;
    public const int SS14Indices = 8;

    /// <summary>
    /// The mapping table for every possible situation.
    /// The first index is the neighbourhood (SS14IndexFlags), and the second index is the subtile (QuadSubtileIndex).
    /// </summary>
    public readonly int[,] Sources = new int[SS14Indices, QuadSubtiles];

    public SS14SmoothingProfile(int[] table, string[] stateNames, DirectionType dirType) : base(stateNames, dirType)
    {
        int tableIdx = 0;
        for (int i = 0; i < SS14Indices; i++)
            for (int j = 0; j < QuadSubtiles; j++)
                Sources[i, j] = table[tableIdx++];
    }

    /// <summary>
    /// The source substate for a given subtile.
    /// </summary>
    public int this[SS14IndexFlags idx, QuadSubtileIndex sub]
    {
        get
        {
            return Sources[(int) idx, (int) sub];
        }
        set
        {
            Sources[(int) idx, (int) sub] = value;
        }
    }

    /// <summary>
    /// Convert this SS14SmoothingProfile into a QuadSmoothingProfile.
    /// </summary>
    public QuadSmoothingProfile Compile(string name)
    {
        QuadSmoothingProfile res = new(this, name);
        for (int subTileIdx = 0; subTileIdx < QuadSubtiles; subTileIdx++)
        {
            var subTile = (QuadSubtileIndex) subTileIdx;
            var relevantDirFlags = SS14IndexFlags.All.ToDirectionFlags(subTile);
            for (int neighbourIdx = 0; neighbourIdx < SS14Indices; neighbourIdx++)
            {
                var neighbourFlags = (SS14IndexFlags) neighbourIdx;
                var neighbourDirFlags = neighbourFlags.ToDirectionFlags(subTile);
                for (int dirFlagsIdx = 0; dirFlagsIdx < TilesetTiles; dirFlagsIdx++)
                {
                    // See if this matches
                    var dirFlags = (DirectionFlags) dirFlagsIdx;
                    if ((dirFlags & relevantDirFlags) != neighbourDirFlags)
                        continue;
                    // It matches, so we're using this
                    res[dirFlags, subTile] = this[neighbourFlags, subTile];
                }
            }
        }
        return res;
    }
}

