using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Toolkit.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SpaceWizards.RsiLib.Directions;

namespace SpaceWizards.RsiLib.RSI;

[PublicAPI]
public sealed class RsiState : IDisposable
{
    private static readonly char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();

    public RsiState(
        string name,
        DirectionType directions = DirectionType.None,
        List<List<float>>? delays = null,
        Dictionary<string, object>? flags = null,
        string? imagePath = null,
        Image<Rgba32>[,]? frames = null,
        string? invalidCharacterReplace = "_")
    {
        if (invalidCharacterReplace != null)
        {
            name = string.Join(invalidCharacterReplace, name.Split(Path.GetInvalidFileNameChars()));
        }

        Name = name;
        Directions = directions;
        Delays = delays;
        Flags = flags;

        ImagePath = imagePath;
        DelayLength = Delays is {Count: > 0} ? Delays[0].Count : 1;
        Frames = frames ?? new Image<Rgba32>[8, DelayLength];

        Guard.IsEqualTo(Frames.Length, 8 * DelayLength, "Frames.Length");
    }

    public RsiState(
        string name,
        DirectionType directions = DirectionType.None,
        List<List<float>>? delays = null,
        Dictionary<string, object>? flags = null,
        string? imagePath = null)
        : this(name, directions, delays, flags, imagePath, null, null)
    {
    }

    public RsiState() : this("", DirectionType.None, null, null, null)
    {
    }

    public string Name { get; set; }

    public DirectionType Directions { get; set; }

    public List<List<float>>? Delays { get; set; }

    public Dictionary<string, object>? Flags { get; set; }

    /// <summary>
    ///     The path of the image to be copied when saving this state.
    /// </summary>
    public string? ImagePath { get; set; }

    public int DelayLength { get; private set; }

    public Image<Rgba32>?[,] Frames { get; private set; }

    public static (int rows, int columns) GetRowsAndColumns(int images)
    {
        var sqrt = Math.Sqrt(images);
        var rows = (int) Math.Ceiling(sqrt);
        var columns = (int) Math.Round(sqrt);

        return (rows, columns);
    }

    public Image<Rgba32> GetFullImage(RsiSize size)
    {
        var totalImages = Frames.Cast<Image<Rgba32>>().Count(x => x != null);
        var (rows, columns) = GetRowsAndColumns(totalImages);
        var image = new Image<Rgba32>(size.X * columns, size.Y * rows);

        var currentX = 0;
        var currentY = 0;

        foreach (var frame in Frames)
        {
            if (frame == null)
            {
                continue;
            }

            var point = new Point(currentX, currentY);
            image.Mutate(x => x.DrawImage(frame, point, 1));

            currentX += size.X;

            if (currentX >= image.Width)
            {
                currentX = 0;
                currentY += size.Y;
            }
        }

        return image;
    }

    public int FirstFrameIndexFor(RsiSize size, Direction direction)
    {
        var directionIndex = (int) direction;

        if (Delays == null)
        {
            return directionIndex;
        }

        if (directionIndex >= Delays.Count)
        {
            return (int) direction;
        }

        var currentFrame = 0;
        for (var i = 0; i < directionIndex; i++)
        {
            var frames = Delays[i].Count;
            currentFrame += frames;
        }

        return currentFrame;
    }

    public Image<Rgba32>?[] FirstImageFor(Direction direction)
    {
        var image = new Image<Rgba32>?[DelayLength];

        for (var i = 0; i < Frames.GetUpperBound((int) direction); i++)
        {
            image[i] = Frames[(int) direction, i];
        }

        return image;
    }

    public Image<Rgba32>? FirstFrameFor(Direction direction)
    {
        return FirstImageFor(direction)[0];
    }

    public Image<Rgba32>?[] FirstFrameForAll(DirectionType directionType)
    {
        var frames = new Image<Rgba32>?[8];

        for (var i = 0; i < (int) directionType; i++)
        {
            var direction = (Direction) i;
            frames[i] = FirstFrameFor(direction);
        }

        return frames;
    }

    /// <summary>
    /// Overwrites this RsiState with the specified gif image.
    /// </summary>
    public void LoadGif(Image<Rgba32> image)
    {
        DelayLength = image.Frames.Count;
        Delays = new List<List<float>> { new() };
        Frames = new Image<Rgba32>[8, DelayLength];

        for (var frame = 0; frame < DelayLength; frame++)
        {
            var frameImage = image.Frames.CloneFrame(frame);

            var frameData = image.Frames[frame].Metadata.GetGifMetadata();
            // frame delays stored as 10 ms increments hence this.
            Delays[0].Add(frameData.FrameDelay / 100f);
            Frames[0, frame] = frameImage;
        }
    }

    public void LoadImage(Image<Rgba32> image, RsiSize size)
    {
        var currentX = 0;
        var currentY = 0;

        for (var direction = 0; direction < (int) Directions; direction++)
        {
            for (var frame = 0; frame < DelayLength; frame++)
            {
                var rectangle = new Rectangle(currentX, currentY, size.X, size.Y);
                var crop = image.Clone(x => x.Crop(rectangle));

                Frames[direction, frame] = crop;

                currentX += size.X;

                if (currentX >= image.Width)
                {
                    currentX = 0;
                    currentY += size.Y;
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var image in Frames)
        {
            image?.Dispose();
        }

        Array.Clear(Frames, 0, Frames.Length);
    }
}
