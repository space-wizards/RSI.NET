using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// Contains the source metrics of actual-tile-size-independent smoothing profiles.
/// </summary>
public abstract class BaseSmoothingProfileMetrics
{
    // May not end up being used for lookup.
    // Implies the state count.
    public readonly string[] SourceStateNameSuffixes;
    public readonly DirectionType SourceDirectionType;

    public BaseSmoothingProfileMetrics(string[] nameSuffixes, DirectionType dt)
    {
        SourceStateNameSuffixes = nameSuffixes;
        SourceDirectionType = dt;
    }

    public BaseSmoothingProfileMetrics(BaseSmoothingProfileMetrics pm)
    {
        SourceStateNameSuffixes = pm.SourceStateNameSuffixes;
        SourceDirectionType = pm.SourceDirectionType;
    }

    public int SubstateToState(int substate)
    {
        return substate / (int) SourceDirectionType;
    }
    public Direction SubstateToDirection(int substate)
    {
        return (Direction) (substate % (int) SourceDirectionType);
    }
    public int StateDirToSubstate(int state, Direction dir)
    {
        return (state * (int) SourceDirectionType) + (int) dir;
    }
}

/// <summary>
/// Contains a ready-to-use smoothing profile with specific metrics.
/// </summary>
public abstract class ReadyBaseSmoothingProfile : BaseSmoothingProfileMetrics
{
    public readonly RsiSize RsiSize;

    public ReadyBaseSmoothingProfile(RsiSize sz, BaseSmoothingProfileMetrics metricsSrc) : base(metricsSrc)
    {
        RsiSize = sz;
    }

    /// <summary>
    /// Converts substates into a 256-tile set.
    /// </summary>
    public abstract Image<Rgba32>[] SubstatesToTileset(Image<Rgba32>[] substates);

    /// <summary>
    /// Converts a 256-tile set into the respective substates.
    /// </summary>
    public abstract Image<Rgba32>[] TilesetToSubstates(Image<Rgba32>[] tiles);
}

