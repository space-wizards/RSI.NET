using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// A smoothing profile based on dividing the tile into 4 subtiles.
/// </summary>
public sealed class QuadSmoothingProfile : BaseSmoothingProfileMetrics, ISmoothingProfile
{
    public const int QuadSubtiles = 4;

    public string Name { get; }
    public bool UsesQuadSplit => true;

    /// <summary>
    /// The giant mapping table for every possible situation.
    /// The first index is the tile (TGFlags), and the second index is the subtile (QuadSubtileIndex).
    /// </summary>
    public readonly int[,] Sources = new int[Tileset.Count, QuadSubtiles];

    public QuadSmoothingProfile(BaseSmoothingProfileMetrics metrics, string name) : base(metrics)
    {
        Name = name;
    }

    /// <summary>
    /// The source substate for a given subtile.
    /// </summary>
    public int this[TGFlags idx, QuadSubtileIndex sub]
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

    public BaseSmoothingInstance Instance(QuadMetrics qm) => new QuadSmoothingInstance(qm, this);
}

/// <summary>
/// A ready-to-use instance of a QuadSmoothingProfile for given QuadMetrics.
/// </summary>
public sealed class QuadSmoothingInstance : BaseSmoothingInstance
{
    public const int QuadSubtiles = QuadSmoothingProfile.QuadSubtiles;

    public readonly QuadSmoothingProfile BaseProfile;
    public readonly QuadMetrics QuadMetrics;

    public QuadSmoothingInstance(QuadMetrics qm, QuadSmoothingProfile src) : base(qm.TileSizeAsRSISize, src)
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

    public override Tileset SubstatesToTileset(Image<Rgba32>[] substates)
    {
        Tileset tileset = new(QuadMetrics.TileSize);
        for (var i = 0; i < Tileset.Count; i++)
        {
            var tile = tileset[i];
            for (var j = 0; j < QuadSubtiles; j++)
            {
                var sourceIdx = BaseProfile.Sources[i, j];
                TransferSubtile(tile, substates[sourceIdx], (QuadSubtileIndex) j);
            }
        }
        return tileset;
    }

    public override Image<Rgba32>[] TilesetToSubstates(Tileset tileset)
    {
        Image<Rgba32>[] substates = new Image<Rgba32>[SourceStateNameSuffixes.Length];
        for (var i = 0; i < substates.Length; i++)
            substates[i] = new Image<Rgba32>(RsiSize.X, RsiSize.Y);

        bool[,] firstWins = new bool[substates.Length, QuadSubtiles];
        for (var i = 0; i < Tileset.Count; i++)
        {
            var tile = tileset[i];
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

