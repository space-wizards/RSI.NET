using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// A smoothing profile that transfers the states 1:1 to substates.
/// </summary>
public sealed class ReferenceSheetSmoothingProfile : ISmoothingProfile
{
    public string Name => "reference";
    public bool UsesQuadSplit => false;
    public BaseSmoothingInstance Instance(QuadMetrics qm) => new ReferenceSheetSmoothingInstance(qm);
}

/// <summary>
/// A ready-to-use instance of a ReferenceSheetSmoothingProfile for given QuadMetrics.
/// </summary>
public sealed class ReferenceSheetSmoothingInstance : BaseSmoothingInstance
{
    private static BaseSmoothingProfileMetrics ReferenceSheetProfileMetrics()
    {
        string[] states = new string[256];
        for (var i = 0; i < 256; i++)
            states[i] = i.ToString();
        return new BaseSmoothingProfileMetrics(states, DirectionType.None);
    }

    public ReferenceSheetSmoothingInstance(QuadMetrics qm) : base(qm.TileSizeAsRSISize, ReferenceSheetProfileMetrics())
    {
    }

    public override Tileset SubstatesToTileset(Image<Rgba32>[] substates)
    {
        Tileset tileset = new(new Size(RsiSize.X, RsiSize.Y));
        for (var i = 0; i < substates.Length; i++)
            tileset[i] = substates[i].Clone();
        return tileset;
    }

    public override Image<Rgba32>[] TilesetToSubstates(Tileset tileset)
    {
        Image<Rgba32>[] substates = new Image<Rgba32>[Tileset.Count];
        for (var i = 0; i < substates.Length; i++)
            substates[i] = tileset[i].Clone();
        return substates;
    }
}

