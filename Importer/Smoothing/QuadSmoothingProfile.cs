using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// A smoothing profile defines a specific smoothing arrangement between states and directions inside an RSI and the canonical 256-tile form.
/// </summary>
public sealed class QuadSmoothingProfile : BaseSmoothingProfileMetrics
{
    /// <summary>
    /// The giant mapping table for every possible situation.
    /// The first index is the tile (DirectionFlags), and the second index is the subtile (QuadSubtileIndex).
    /// </summary>
    public readonly int[,] Sources = new int[256, 4];

    public QuadSmoothingProfile(BaseSmoothingProfileMetrics metrics) : base(metrics)
    {
    }

    /// <summary>
    /// The source substate for a given subtile.
    /// </summary>
    public int this[DirectionFlags idx, QuadSubtileIndex sub]
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

    public ReadyQuadSmoothingProfile Parameterize(QuadMetrics qm) => new ReadyQuadSmoothingProfile(qm, this);
}

/// <summary>
/// A smoothing profile defines a specific smoothing arrangement between states and directions inside an RSI and the canonical 256-tile form.
/// </summary>
public sealed class ReadyQuadSmoothingProfile : ReadyBaseSmoothingProfile
{
    public readonly QuadSmoothingProfile BaseProfile;

    public ReadyQuadSmoothingProfile(QuadMetrics qm, QuadSmoothingProfile src) : base(new RsiSize(qm.TileSize.Width, qm.TileSize.Height), src)
    {
        BaseProfile = src;
    }

    public override Image<Rgba32>[] SubstatesToTileset(Image<Rgba32>[] substates)
    {
        throw new Exception("Whoops!");
    }

    public override Image<Rgba32>[] TilesetToSubstates(Image<Rgba32>[] tiles)
    {
        throw new Exception("Whoops!");
    }
}

