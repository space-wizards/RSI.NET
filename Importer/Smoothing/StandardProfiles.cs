using Importer.Directions;

namespace RSI.Smoothing;

public static class SmoothingProfiles
{
    public static readonly QuadSmoothingProfile SS14 = new SS14SmoothingProfile(
        new int[]
        {
        // QuadSubtileIndex order matches RT direction order
        // So this is pretty much the identity matrix
        //  BR  TL  TR  BL
             0,  1,  2,  3, // 0
             4,  5,  6,  7, // 1
             8,  9, 10, 11, // 2
            12, 13, 14, 15, // 3
            16, 17, 18, 19, // 4
            20, 21, 22, 23, // 5
            24, 25, 26, 27, // 6
            28, 29, 30, 31  // 7
        },
        new string[] {"0", "1", "2", "3", "4", "5", "6", "7"},
        DirectionType.Cardinal
    ).Compile();

    public static readonly QuadSmoothingProfile VXASplit = new SS14SmoothingProfile(
        new int[]
        {
        //  BR  TL  TR  BL
             5,  2,  3,  4, // 0 X (ST)
             4,  3,  5,  2, // 1
             5,  2,  3,  4, // 2 X (ST)
             4,  3,  5,  2, // 3
             3,  4,  2,  5, // 4
             1,  1,  1,  1, // 5 X (IC)
             3,  4,  2,  5, // 6
             2,  5,  4,  3  // 7 X (F)
        },
        new string[] {"0", "1", "2", "3", "4", "5"},
        DirectionType.None
    ).Compile();

    public static readonly QuadSmoothingProfile VXAPSplit = new SS14SmoothingProfile(
        new int[]
        {
        //  BR  TL  TR  BL
             5,  2,  3,  4, // 0 X (ST)
             4,  3,  5,  2, // 1
             0,  0,  0,  0, // 2 - diagdup of 0
             6,  6,  7,  7, // 3 - diagdup of 1
             3,  4,  2,  5, // 4
             1,  1,  1,  1, // 5 X (IC)
             7,  7,  6,  6, // 6 - diagdup of 4
             2,  5,  4,  3  // 7 X (F)
        },
        new string[] {"0", "1", "2", "3", "4", "5", "6", "7"},
        DirectionType.None
    ).Compile();
}

