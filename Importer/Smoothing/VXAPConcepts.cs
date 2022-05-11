using System;
using SixLabors.ImageSharp;
using Importer.Directions;

namespace RSI.Smoothing;

/// <summary>
/// Metrics for a VXAP autotile field.
/// The VXAP form is single-image.
/// </summary>
public struct VXAPMetrics
{
    /// <summary>
    /// Tile size.
    /// </summary>
    public QuadMetrics Base;

    public Size Size => new Size(Base.TileSize.Width * 2, Base.TileSize.Height * 3);
    public Size SizeExtended => new Size(Base.TileSize.Width * 2, Base.TileSize.Height * 4);

    /// <summary>
    /// Create VXAP metrics from QuadMetrics.
    /// </summary>
    public VXAPMetrics(QuadMetrics b)
    {
        Base = b;
    }

    /// <summary>
    /// Return a rectangle for a given tile.
    /// </summary>
    public Rectangle this[VXAPIndex idx]
    {
        get
        {
            int idx2 = (int) idx;
            Point p = new Point((idx2 % 2) * Base.TileSize.Width, (idx2 / 2) * Base.TileSize.Height);
            return new Rectangle(p, Base.TileSize);
        }
    }

    /// <summary>
    /// Return a rectangle for a given subtile.
    /// </summary>
    public Rectangle this[VXAPIndex idx, QuadSubtileIndex sub]
    {
        get
        {
            var tile = this[idx];
            var rect = Base[sub];
            rect.Offset(tile.X, tile.Y);
            return rect;
        }
    }
}

/// <summary>
/// Indices for VXAP tiles.
/// Also see this visualization:
/// 01
/// 23
/// 45
/// 67
/// </summary>
public enum VXAPIndex : int
{
/// <summary>
/// 1.1
/// .O.
/// 1.1
/// </summary>
    XDiagonals = 0,
/// <summary>
/// .1.
/// 1O1
/// .1.
/// </summary>
    InnerCorners = 1,
/// <summary>
/// ...
/// .O1
/// .11
/// </summary>
    NorthWest = 2,
/// <summary>
/// ...
/// 1O.
/// 11.
/// </summary>
    NorthEast = 3,
/// <summary>
/// .11
/// .O1
/// ...
/// </summary>
    SouthWest = 4,
/// <summary>
/// 11.
/// 1O.
/// ...
/// </summary>
    SouthEast = 5,
/// <summary>
/// 1.1
/// 1O1
/// 1.1
/// </summary>
    XHorizontalDiagonals = 6,
/// <summary>
/// 111
/// .O.
/// 111
/// </summary>
    XVerticalDiagonals = 7
}

public static class VXAPIndexMethods
{
    public static DirectionFlags ToDirectionFlags(this VXAPIndex idx)
    {
        switch (idx)
        {
            case VXAPIndex.XDiagonals:
                return DirectionFlags.NorthWest | DirectionFlags.NorthEast | DirectionFlags.SouthWest | DirectionFlags.SouthEast;
            case VXAPIndex.InnerCorners:
                return DirectionFlags.North | DirectionFlags.South | DirectionFlags.East | DirectionFlags.West;
            case VXAPIndex.NorthWest:
                return DirectionFlags.East | DirectionFlags.SouthEast | DirectionFlags.South;
            case VXAPIndex.NorthEast:
                return DirectionFlags.West | DirectionFlags.SouthWest | DirectionFlags.South;
            case VXAPIndex.SouthWest:
                return DirectionFlags.East | DirectionFlags.NorthEast | DirectionFlags.North;
            case VXAPIndex.SouthEast:
                return DirectionFlags.West | DirectionFlags.NorthWest | DirectionFlags.North;
            case VXAPIndex.XHorizontalDiagonals:
                return DirectionFlags.NorthWest | DirectionFlags.NorthEast | DirectionFlags.SouthWest | DirectionFlags.SouthEast | DirectionFlags.West | DirectionFlags.East;
            case VXAPIndex.XVerticalDiagonals:
                return DirectionFlags.NorthWest | DirectionFlags.NorthEast | DirectionFlags.SouthWest | DirectionFlags.SouthEast | DirectionFlags.North | DirectionFlags.South;
        }
        throw new Exception("Unknown VXAPIndex");
    }
}

