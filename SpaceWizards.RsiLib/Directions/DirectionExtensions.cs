using System;
using System.Collections.Immutable;

namespace SpaceWizards.RsiLib.Directions;

public static class DirectionExtensions
{
    private static readonly ImmutableArray<Direction> Cardinals =
        ImmutableArray.Create(Direction.South, Direction.North, Direction.East, Direction.West);

    public static ImmutableArray<Direction> GetCardinals()
    {
        return Cardinals;
    }
}
