using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SpaceWizards.RsiLib.Directions;
using SpaceWizards.RsiLib.RSI;

namespace SpaceWizards.RsiLib.DMI;

public record DmiState(
    string Name,
    DirectionType Directions = DirectionType.None,
    int Frames = 1,
    List<float>? Delay = null,
    bool Rewind = false)
{
    public static int GetRewindIndex(int currentIndex, int totalFrames)
    {
        return totalFrames - (currentIndex - totalFrames) - 2;
    }

    public RsiState ToRsiState(Image<Rgba32>[,] frames)
    {
        var delays = new List<List<float>>();

        if (Delay != null)
        {
            for (var i = 0; i < (int) Directions; i++)
            {
                delays.Add(Delay);
            }
        }

        return new RsiState(Name, Directions, delays, frames: frames);
    }
}