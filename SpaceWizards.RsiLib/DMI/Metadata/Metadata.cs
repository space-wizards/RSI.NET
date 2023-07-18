using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SpaceWizards.RsiLib.RSI;

namespace SpaceWizards.RsiLib.DMI.Metadata;

public class Metadata : IMetadata
{
    public Metadata(Version version, List<DmiState> states)
    {
        Version = version.VersionNumber;
        Width = version.Width;
        Height = version.Height;
        States = states;
    }

    public double Version { get; }

    public int Width { get; }

    public int Height { get; }

    public List<DmiState> States { get; }

    public Rsi ToRsi(Image<Rgba32> image)
    {
        var size = new RsiSize(Width, Height);
        var rsiStates = new List<RsiState>(States.Count);
        var currentX = 0;
        var currentY = 0;

        foreach (var dmiState in States)
        {
            var frameAmount = dmiState.Delay?.Count ?? 1;
            var images = new Image<Rgba32>[8, frameAmount];

            for (var frame = 0; frame < dmiState.Frames; frame++)
            {
                for (var direction = 0; direction < (int) dmiState.Directions; direction++)
                {
                    var rectangle = new Rectangle(currentX, currentY, size.X, size.Y);
                    var crop = image.Clone(x => x.Crop(rectangle));

                    images[direction, frame] = crop;

                    currentX += size.X;

                    if (currentX >= image.Width)
                    {
                        currentX = 0;
                        currentY += size.Y;
                    }
                }
            }

            if (dmiState.Rewind)
            {
                for (var frame = frameAmount - 1; frame >= dmiState.Frames; frame--)
                {
                    for (var direction = 0; direction < (int) dmiState.Directions; direction++)
                    {
                        var rewindIndex = DmiState.GetRewindIndex(frame, dmiState.Frames);
                        images[direction, frame] = images[direction, rewindIndex].Clone();
                    }
                }
            }

            var rsiState = dmiState.ToRsiState(images);
            rsiStates.Add(rsiState);
        }

        return new Rsi(Rsi.CurrentRsiVersion, size, rsiStates);
    }
}