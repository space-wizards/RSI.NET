using System;
using System.Threading.Tasks;
using Importer.RSI;
using RSI.Smoothing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SmoothCLI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rsiPath = args[0];
        Rsi input = await Rsi.FromFolder(rsiPath);
        await input.TryLoadFolderImages(rsiPath);

        var profile = SmoothingProfiles.SS14.Parameterize(new QuadMetrics(new Size(32, 32)));

        Console.WriteLine($"profile dirtype {profile.SourceDirectionType}");

        Image<Rgba32>[] substates = new Image<Rgba32>[8 * 4];
        for (int i = 0; i < substates.Length; i++)
        {
            var stateIdx = profile.SubstateToState(i);
            var direction = profile.SubstateToDirection(i);
            Console.WriteLine($"trying substate {i} as {stateIdx}, {direction}");
            var state = input.States[stateIdx];
            Console.WriteLine($" is {state.Name} / {state.Directions} / {state.Frames.Length}");
            substates[i] = state.FirstFrameFor(direction)!;
        }

        Image<Rgba32>[] tiles = profile.SubstatesToTileset(substates);
        for (int i = 0; i < tiles.Length; i++)
            await tiles[i].SaveAsPngAsync($"tile{i}.png");
    }
}

