namespace Importer.Directions;

public enum Direction
{
    South = 0,
    North = 1,
    East = 2,
    West = 3,
    SouthEast = 4,
    SouthWest = 5,
    NorthEast = 6,
    NorthWest = 7,
}

/// <summary>
/// Flags (bits kept in sync with Direction's values for consistency)
/// </summary>
public enum DirectionFlags : int
{
    South = 1,
    North = 2,
    East = 4,
    West = 8,
    SouthEast = 16,
    SouthWest = 32,
    NorthEast = 64,
    NorthWest = 128,
}

