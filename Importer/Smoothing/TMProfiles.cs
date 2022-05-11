using Importer.Directions;

namespace RSI.Smoothing;

public static class TMProfiles
{
    public static TileMapperSmoothingProfile CreateTGProfile()
    {
        int[] s2t = {
            0, 1, 2, 3, 4, 5, 6, 7,
            8, 9, 10, 11, 12, 13, 14, 15,
            21, 23, 29, 31, 38, 39, 46, 47,
            55, 63, 74, 75, 78, 79, 95, 110,
            111, 127, 137, 139, 141, 143, 157, 159,
            175, 191, 203, 207, 223, 239, 255
        };
        return new TileMapperSmoothingProfile("tg", s2t, AutoT2S(s2t));
    }

    public static TileMapperSmoothingProfile CreateTestPatternProfile()
    {
        TGFlags[] tgf = new TGFlags[]
        {
            // Y0X0
            (int) 0, TGFlags.E, TGFlags.W | TGFlags.S | TGFlags.SE | TGFlags.E, TGFlags.W | TGFlags.SW | TGFlags.S,
            // Y0X4
            TGFlags.S | TGFlags.SE | TGFlags.E, TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.SE | TGFlags.E,
            // Y0X6
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.E, TGFlags.W,

            // Y1X0
            TGFlags.S | TGFlags.E, TGFlags.W | TGFlags.S | TGFlags.E,
            // Y1X2
            TGFlags.W | TGFlags.S | TGFlags.E | TGFlags.NE | TGFlags.N,
            // Y1X3
            TGFlags.W | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.N | TGFlags.NW,
            // Y1X4
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.NE | TGFlags.N,
            // Y1X5
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.NE | TGFlags.N | TGFlags.NW,
            // Y1X6
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.E | TGFlags.N | TGFlags.NW,
            // Y1X7
            TGFlags.W | TGFlags.S,

            // Y2X0
            TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.N, TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.E | TGFlags.N,
            // Y2X2
            TGFlags.W | TGFlags.S | TGFlags.E | TGFlags.N, TGFlags.W | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.NE | TGFlags.N,
            // Y2X4
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.E | TGFlags.NE | TGFlags.N | TGFlags.NW, TGFlags.W | TGFlags.S | TGFlags.E | TGFlags.NE | TGFlags.N | TGFlags.NW,
            // Y2X6
            TGFlags.W | TGFlags.S | TGFlags.N | TGFlags.NW, TGFlags.N,

            // Y3X0
            TGFlags.S | TGFlags.E | TGFlags.NE | TGFlags.N, TGFlags.W | TGFlags.S | TGFlags.E | TGFlags.N | TGFlags.NW,
            // Y3X2
            TGFlags.W | TGFlags.S | TGFlags.N, TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.NE | TGFlags.N,
            // Y3X4
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.N | TGFlags.NW,
            // Y3X5
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.N,
            // Y3X6
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.N,
            // Y3X7
            TGFlags.S,

            // Y4X0
            TGFlags.S | TGFlags.E | TGFlags.N, TGFlags.W | TGFlags.E | TGFlags.N,
            // Y4X2
            TGFlags.W | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.N, TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.E | TGFlags.NE | TGFlags.N,
            // Y4X4
            TGFlags.W | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.NE | TGFlags.N | TGFlags.NW,
            // Y4X5
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.SE | TGFlags.E | TGFlags.NE | TGFlags.N | TGFlags.NW,
            // Y4X6
            TGFlags.W | TGFlags.SW | TGFlags.S | TGFlags.N | TGFlags.NW,
            // Y4X7
            TGFlags.N | TGFlags.S,

            // Y5X0
            TGFlags.E | TGFlags.N, TGFlags.W | TGFlags.E, TGFlags.W | TGFlags.E | TGFlags.NE | TGFlags.N, TGFlags.W | TGFlags.N | TGFlags.NW,
            // Y5X4
            TGFlags.E | TGFlags.NE | TGFlags.N, TGFlags.W | TGFlags.E | TGFlags.NE | TGFlags.N | TGFlags.NW,
            // Y5X6
            TGFlags.W | TGFlags.E | TGFlags.N | TGFlags.NW, TGFlags.W | TGFlags.N
        };
        int[] s2t = new int[tgf.Length];
        for (var i = 0; i < s2t.Length; i++)
            s2t[i] = (int) tgf[i];
        return new TileMapperSmoothingProfile("testpattern-split", s2t, AutoT2S(s2t));
    }

    private static int[] AutoT2S(int[] s2t)
    {
        int[] t2s = new int[256];
        for (var i = 0; i < t2s.Length; i++)
        {
            // basically a deduplication mechanism for those corners that don't usually matter
            // this is where all the holes come from
            var x = i;
            // You might be wondering why I'm not using the usual enums here.
            // Answer is because I checked my work in Lua and didn't have the enums there.
            if ((x & 0x05) != 0x05)
                x &= 0xFF ^ 0x10;
            if ((x & 0x06) != 0x06)
                x &= 0xFF ^ 0x20;
            if ((x & 0x0A) != 0x0A)
                x &= 0xFF ^ 0x40;
            if ((x & 0x09) != 0x09)
                x &= 0xFF ^ 0x80;
            // now to convert X (TGFlags) to a substate index
            for (var j = 0; j < s2t.Length; j++)
            {
                if (s2t[j] == x)
                {
                    t2s[i] = j;
                    break;
                }
            }
        }
        return t2s;
    }
}

