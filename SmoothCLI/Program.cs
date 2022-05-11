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
        var argSrcRSI = 0;
        // tile size options
        var tileWidth = 32;
        var tileHeight = 32;
        var subtileWidth = 16;
        var subtileHeight = 16;
        if (args.Length == 4)
        {
            argSrcRSI = 2;
            // tile size stuff left at default
        }
        else if (args.Length == 5)
        {
            tileWidth = tileHeight = int.Parse(args[2]);
            subtileWidth = tileWidth / 2;
            subtileHeight = tileHeight / 2;
            argSrcRSI = 3;
        }
        else if (args.Length == 6)
        {
            tileWidth = int.Parse(args[2]);
            tileHeight = int.Parse(args[3]);
            subtileWidth = tileWidth / 2;
            subtileHeight = tileHeight / 2;
            argSrcRSI = 4;
        }
        else if (args.Length == 8)
        {
            tileWidth = int.Parse(args[2]);
            tileHeight = int.Parse(args[3]);
            subtileWidth = int.Parse(args[4]);
            subtileHeight = int.Parse(args[5]);
            argSrcRSI = 6;
        }
        else
        {
            Console.WriteLine("./SmoothCLI <source type> <destination type> [<width> [<height> [<split X> <split Y>]]] <source RSI> <destination RSI>");
            Console.WriteLine("split X/Y is for formats that split up the tile into pieces. if split X/Y are omitted, it's assumed to be the centre.");
            Console.WriteLine("if height is also omitted, it defaults to width.");
            Console.WriteLine("if width is also omitted, it defaults to 32.");
            Console.WriteLine("example:");
            Console.WriteLine("./SmoothCLI ss14 vxap src.rsi dst.rsi");
            Console.WriteLine("types:");
            foreach (var v in SmoothingProfiles.AllProfiles)
                Console.WriteLine(v.Name);
            return;
        }

        var importProfile = SmoothingProfiles.ProfileByName(args[0]);
        if (importProfile == null)
            throw new Exception($"Unknown profile {args[0]}");

        var exportProfile = SmoothingProfiles.ProfileByName(args[1]);
        if (exportProfile == null)
            throw new Exception($"Unknown profile {args[1]}");

        var inRSIPath = args[argSrcRSI];
        var outRSIPath = args[argSrcRSI + 1];

        var input = await Rsi.FromFolder(inRSIPath);
        await input.TryLoadFolderImages(inRSIPath);

        var metrics = new QuadMetrics(new Size(tileWidth, tileHeight), new Size(subtileWidth, subtileHeight));

        var importProfileInst = importProfile.Instance(metrics);
        var exportProfileInst = exportProfile.Instance(metrics);

        var output = SmoothingWorkflow.Transform(input, importProfileInst, exportProfileInst);
        await output.SaveToFolder(outRSIPath);
    }
}

