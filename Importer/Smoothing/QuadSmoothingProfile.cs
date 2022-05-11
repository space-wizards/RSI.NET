using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// A smoothing profile defines a specific smoothing arrangement between states and directions inside an RSI and the canonical 256-tile form.
/// </summary>
public sealed class QuadSmoothingProfile : BaseSmoothingProfileMetrics, INamedSmoothingProfile
{
    public const int QuadSubtiles = 4;

    public string Name { get; }

    /// <summary>
    /// The giant mapping table for every possible situation.
    /// The first index is the tile (DirectionFlags), and the second index is the subtile (QuadSubtileIndex).
    /// </summary>
    public readonly int[,] Sources = new int[TilesetTiles, QuadSubtiles];

    public QuadSmoothingProfile(BaseSmoothingProfileMetrics metrics, string name) : base(metrics)
    {
        Name = name;
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
    public const int QuadSubtiles = QuadSmoothingProfile.QuadSubtiles;

    public readonly QuadSmoothingProfile BaseProfile;
    public readonly QuadMetrics QuadMetrics;

    public ReadyQuadSmoothingProfile(QuadMetrics qm, QuadSmoothingProfile src) : base(new RsiSize(qm.TileSize.Width, qm.TileSize.Height), src)
    {
        BaseProfile = src;
        QuadMetrics = qm;
    }

    private void TransferSubtile(Image<Rgba32> dst, Image<Rgba32> src, QuadSubtileIndex subtile)
    {
        var subRect = QuadMetrics[subtile];
        var grabbed = src.Clone(x => x.Crop(subRect));
        var point = new Point(subRect.X, subRect.Y);
        dst.Mutate(x => x.DrawImage(grabbed, point, 1));
    }

    public override Image<Rgba32>[] SubstatesToTileset(Image<Rgba32>[] substates)
    {
        Image<Rgba32>[] tileset = new Image<Rgba32>[TilesetTiles];
        for (var i = 0; i < TilesetTiles; i++)
        {
            var tile = new Image<Rgba32>(QuadMetrics.TileSize.Width, QuadMetrics.TileSize.Height);
            for (var j = 0; j < QuadSubtiles; j++)
            {
                var sourceIdx = BaseProfile.Sources[i, j];
                TransferSubtile(tile, substates[sourceIdx], (QuadSubtileIndex) j);
            }
            tileset[i] = tile;
        }
        return tileset;
    }

    public override Image<Rgba32>[] TilesetToSubstates(Image<Rgba32>[] tiles)
    {
        Image<Rgba32>[] substates = new Image<Rgba32>[SourceStateNameSuffixes.Length];
        for (var i = 0; i < substates.Length; i++)
            substates[i] = new Image<Rgba32>(RsiSize.X, RsiSize.Y);

        bool[,] firstWins = new bool[substates.Length, QuadSubtiles];
        for (var i = 0; i < TilesetTiles; i++)
        {
            var tile = tiles[i];
            for (var j = 0; j < QuadSubtiles; j++)
            {
                var targetIdx = BaseProfile.Sources[i, j];
                // Block a target subtile from being written more than once.
                // This, along with the ordering, prevents the issue of diagonals being written where they really SHOULDN'T.
                if (firstWins[targetIdx, j])
                    continue;
                firstWins[targetIdx, j] = true;
                TransferSubtile(substates[targetIdx], tile, (QuadSubtileIndex) j);
            }
        }
        return substates;
    }
}

