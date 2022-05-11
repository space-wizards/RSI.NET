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
        var input = await Rsi.FromFolder(rsiPath);
        await input.TryLoadFolderImages(rsiPath);

        var metrics = new QuadMetrics(new Size(32, 32));
        var importProfile = SmoothingProfiles.SS14.Instance(metrics);
        var exportProfile = SmoothingProfiles.Reference.Instance(metrics);

        var output = SmoothingWorkflow.Transform(input, importProfile, exportProfile);
        await output.SaveToFolder("out.rsi");
    }
}

