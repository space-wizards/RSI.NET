using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Importer.Directions;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// Smooth-wall Direction Flags.
/// **Assigned to match /tg/'s system, so don't change this!**
/// </summary>
public enum TGFlags : int
{
    North = 1,
    South = 2,
    East = 4,
    West = 8,
    NorthEast = 16,
    SouthEast = 32,
    SouthWest = 64,
    NorthWest = 128,
}

/// <summary>
/// A full tileset with all the directions.
/// </summary>
public sealed class Tileset
{
    public const int Count = 256;
    public readonly Image<Rgba32>[] Tiles = new Image<Rgba32>[Count];

    public Tileset(Size tileSize)
    {
        for (var i = 0; i < Count; i++)
            Tiles[i] = new Image<Rgba32>(tileSize.Width, tileSize.Height);
    }

    public Image<Rgba32> this[TGFlags index]
    {
        get
        {
            return Tiles[(int) index];
        }
        set
        {
            Tiles[(int) index] = value;
        }
    }

    public Image<Rgba32> this[int index]
    {
        get
        {
            return Tiles[index];
        }
        set
        {
            Tiles[index] = value;
        }
    }
}

