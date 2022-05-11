using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// A smoothing profile that splits up and joins tiles.
/// </summary>
public sealed class SplitJoinSmoothingProfile : ISmoothingProfile
{
    public string Name { get; }
    public bool UsesQuadSplit => Underlying.UsesQuadSplit;
    public readonly ISmoothingProfile Underlying;
    public readonly int Columns;

    public SplitJoinSmoothingProfile(string name, ISmoothingProfile underlying, int columns)
    {
        Name = name;
        Underlying = underlying;
        Columns = columns;
    }
    public BaseSmoothingInstance Instance(QuadMetrics qm) => new SplitJoinSmoothingInstance(Underlying.Instance(qm), Columns);
}

/// <summary>
/// A ready-to-use instance of a ReferenceSheetSmoothingProfile for given QuadMetrics.
/// </summary>
public sealed class SplitJoinSmoothingInstance : BaseSmoothingInstance
{
    public readonly BaseSmoothingInstance Underlying;
    public readonly int Columns;

    private static BaseSmoothingProfileMetrics DetermineMetrics()
    {
        return new BaseSmoothingProfileMetrics(new string[] {"sheet"}, DirectionType.None);
    }
    private static RsiSize DetermineRSISize(BaseSmoothingInstance underlying, int columns)
    {
        var rows = (underlying.SubstateCount + (columns - 1)) / columns;
        if (rows == 0)
            rows = 1;
        return new RsiSize(underlying.RsiSize.X * columns, underlying.RsiSize.Y * rows);
    }

    public SplitJoinSmoothingInstance(BaseSmoothingInstance underlying, int columns) : base(DetermineRSISize(underlying, columns), DetermineMetrics())
    {
        Underlying = underlying;
        Columns = columns;
    }

    public Rectangle GetTileRect(int i)
    {
        var x = i % Columns;
        var y = i / Columns;
        var xP = x * Underlying.RsiSize.X;
        var yP = y * Underlying.RsiSize.Y;
        return new Rectangle(xP, yP, Underlying.RsiSize.X, Underlying.RsiSize.Y);
    }

    public override Tileset SubstatesToTileset(Image<Rgba32>[] substates)
    {
        var sheet = substates[0];
        Image<Rgba32>[] substatesActual = new Image<Rgba32>[Underlying.SubstateCount];
        for (var i = 0; i < substatesActual.Length; i++)
        {
            substatesActual[i] = sheet.Clone(x => x.Crop(GetTileRect(i)));
        }
        return Underlying.SubstatesToTileset(substatesActual);
    }

    public override Image<Rgba32>[] TilesetToSubstates(Tileset tileset)
    {
        var substates = Underlying.TilesetToSubstates(tileset);

        Image<Rgba32> sheet = new Image<Rgba32>(RsiSize.X, RsiSize.Y);

        for (var i = 0; i < substates.Length; i++)
        {
            var rect = GetTileRect(i);
            sheet.Mutate(x => x.DrawImage(substates[i], new Point(rect.X, rect.Y), 1));
        }

        return new Image<Rgba32>[] { sheet };
    }
}

