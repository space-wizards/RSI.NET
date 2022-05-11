using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// A smoothing profile that transfers the states 1:1 to substates.
/// There may be some remapping involved (duplication/deduplication).
/// For now naming is based on the tg directions because this is being used for tg stuff.
/// </summary>
public sealed class TileMapperSmoothingProfile : ISmoothingProfile
{
    public string Name { get; }
    public bool UsesQuadSplit => false;
    // Length defines the amount of substates
    public readonly int[] SubstatesToTileset;
    // Length is Tileset.Count
    public readonly int[] TilesetToSubstates;

    public TileMapperSmoothingProfile(string name, int[] s2t, int[] t2s)
    {
        Name = name;
        SubstatesToTileset = s2t;
        TilesetToSubstates = t2s;
    }

    public static TileMapperSmoothingProfile ReferenceSplit()
    {
        int[] array = new int[Tileset.Count];
        for (var i = 0; i < Tileset.Count; i++)
            array[i] = i;
        return new TileMapperSmoothingProfile("reference-split", array, array);
    }

    public BaseSmoothingInstance Instance(QuadMetrics qm) => new TileMapperSmoothingInstance(this, qm);
}

/// <summary>
/// A ready-to-use instance of a TileMapperSmoothingProfile for given QuadMetrics.
/// </summary>
public sealed class TileMapperSmoothingInstance : BaseSmoothingInstance
{
    public readonly TileMapperSmoothingProfile Profile;

    private static BaseSmoothingProfileMetrics ReferenceSheetProfileMetrics(TileMapperSmoothingProfile profile)
    {
        string[] states = new string[profile.SubstatesToTileset.Length];
        for (var i = 0; i < states.Length; i++)
            states[i] = profile.SubstatesToTileset[i].ToString();
        return new BaseSmoothingProfileMetrics(states, DirectionType.None);
    }

    public TileMapperSmoothingInstance(TileMapperSmoothingProfile profile, QuadMetrics qm) : base(qm.TileSizeAsRSISize, ReferenceSheetProfileMetrics(profile))
    {
        Profile = profile;
    }

    public override Tileset SubstatesToTileset(Image<Rgba32>[] substates)
    {
        Tileset tileset = new(new Size(RsiSize.X, RsiSize.Y));
        for (var i = 0; i < Tileset.Count; i++)
            tileset[i] = substates[Profile.TilesetToSubstates[i]].Clone();
        return tileset;
    }

    public override Image<Rgba32>[] TilesetToSubstates(Tileset tileset)
    {
        Image<Rgba32>[] substates = new Image<Rgba32>[SubstateCount];
        for (var i = 0; i < substates.Length; i++)
            substates[i] = tileset[Profile.SubstatesToTileset[i]].Clone();
        return substates;
    }
}

