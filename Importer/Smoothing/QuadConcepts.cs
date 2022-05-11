using System;
using SixLabors.ImageSharp;
using Importer.RSI;

namespace RSI.Smoothing;

/// <summary>
/// Metrics for the separate subtiles.
/// </summary>
public struct QuadMetrics
{
    /// <summary>
    /// Tile size.
    /// </summary>
    public Size TileSize;

    public RsiSize TileSizeAsRSISize => new RsiSize(TileSize.Width, TileSize.Height);

    /// <summary>
    /// North-west subtile size.
    /// Used as a reference for the other subtiles.
    /// </summary>
    public Size NorthWestSubtileSize;

    /// <summary>
    /// Create an evenly spaced set of quad metrics.
    /// </summary>
    public QuadMetrics(Size tileSize)
    {
        TileSize = tileSize;
        NorthWestSubtileSize = new Size(tileSize.Width / 2, tileSize.Height / 2);
    }

    /// <summary>
    /// Create a custom (uneven) set of quad metrics.
    /// </summary>
    public QuadMetrics(Size tileSize, Size nwSize)
    {
        TileSize = tileSize;
        NorthWestSubtileSize = nwSize;
    }

    /// <summary>
    /// Return a rectangle for a given subtile.
    /// </summary>
    public Rectangle this[QuadSubtileIndex index]
    {
        get
        {
            switch (index)
            {
                case QuadSubtileIndex.SouthEast:
                    return new Rectangle(new Point(NorthWestSubtileSize), TileSize - NorthWestSubtileSize);
                case QuadSubtileIndex.NorthWest:
                    return new Rectangle(Point.Empty, NorthWestSubtileSize);
                case QuadSubtileIndex.NorthEast:
                    return new Rectangle(new Point(NorthWestSubtileSize.Width, 0), new Size(TileSize.Width - NorthWestSubtileSize.Width, NorthWestSubtileSize.Height));
                case QuadSubtileIndex.SouthWest:
                    return new Rectangle(new Point(0, NorthWestSubtileSize.Height), new Size(NorthWestSubtileSize.Width, TileSize.Height - NorthWestSubtileSize.Height));
            }
            throw new Exception("Unknown QuadSubtileIndex! That sure isn't supposed to happen!");
        }
    }
}

/// <summary>
/// Indices for subtiles.
/// These specific numbers must match with:
/// + RSI directions
/// + Content.Client icon smoothing
/// </summary>
public enum QuadSubtileIndex : int
{
    SouthEast = 0,
    NorthWest = 1,
    NorthEast = 2,
    SouthWest = 3
}

