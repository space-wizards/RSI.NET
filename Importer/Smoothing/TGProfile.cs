using Importer.Directions;

namespace RSI.Smoothing;

public static class TGProfile
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
        return new TileMapperSmoothingProfile("tg", s2t, t2s);
    }
}

