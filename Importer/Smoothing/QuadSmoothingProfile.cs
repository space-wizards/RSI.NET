using System;
using SixLabors.ImageSharp;
using Importer.Directions;

namespace RSI.Smoothing;

public class BaseSmoothingProfile
{
    // May not end up being used for lookup.
    // Implies the state count.
    public string[] SourceStateNameSuffixes = {""};
    public DirectionType SourceDirectionType = DirectionType.None;

    public void CopySourceMetricsFrom(BaseSmoothingProfile src)
    {
        SourceStateNameSuffixes = (string[]) src.SourceStateNameSuffixes.Clone();
        SourceDirectionType = src.SourceDirectionType;
    }
}

/// <summary>
/// A smoothing profile defines a specific smoothing arrangement between states and directions inside an RSI and the canonical 256-tile form.
/// </summary>
public sealed class QuadSmoothingProfile : BaseSmoothingProfile
{
    /// <summary>
    /// The giant mapping table for every possible situation.
    /// The first index is the tile (DirectionFlags), and the second index is the subtile (QuadSubtileIndex).
    /// </summary>
    public readonly QuadSmoothingProfileSource[,] Sources = new QuadSmoothingProfileSource[256, 4];

    public QuadSmoothingProfile()
    {
    }

    /// <summary>
    /// The QuadSmoothingProfileSource for a given subtile.
    /// </summary>
    public QuadSmoothingProfileSource this[DirectionFlags idx, QuadSubtileIndex sub]
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
}

/// <summary>
/// A specific entry in a smoothing profile.
/// </summary>
public struct QuadSmoothingProfileSource
{
    // Source state index & direction
    public int SourceState;
    public Direction SourceDirection;

    public QuadSmoothingProfileSource(int state, Direction dir = Direction.South)
    {
        SourceState = state;
        SourceDirection = dir;
    }
}

