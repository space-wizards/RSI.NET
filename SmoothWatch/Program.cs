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
        if (args.Length == 3)
        {
            argSrcRSI = 1;
            // tile size stuff left at default
        }
        else if (args.Length == 4)
        {
            tileWidth = tileHeight = int.Parse(args[2]);
            subtileWidth = tileWidth / 2;
            subtileHeight = tileHeight / 2;
            argSrcRSI = 2;
        }
        else if (args.Length == 5)
        {
            tileWidth = int.Parse(args[2]);
            tileHeight = int.Parse(args[3]);
            subtileWidth = tileWidth / 2;
            subtileHeight = tileHeight / 2;
            argSrcRSI = 3;
        }
        else if (args.Length == 7)
        {
            tileWidth = int.Parse(args[1]);
            tileHeight = int.Parse(args[2]);
            subtileWidth = int.Parse(args[3]);
            subtileHeight = int.Parse(args[4]);
            argSrcRSI = 5;
        }
        else
        {
            Console.WriteLine("./SmoothWatch <source type> [<width> [<height> [<split X> <split Y>]]] <source PNG> <destination PNG>");
            Console.WriteLine("This is for testing iconsmooths. It will rewrite a PNG roughly every 2 seconds, displaying the iconsmooth in an example configuration.");
            Console.WriteLine("split X/Y is for formats that split up the tile into pieces. if split X/Y are omitted, it's assumed to be the centre.");
            Console.WriteLine("if height is also omitted, it defaults to width. if width is also omitted, it defaults to 32.");
            Console.WriteLine("example:");
            Console.WriteLine("./SmoothWatch vxap vxap.png watch.png");
            Console.WriteLine("types:");
            foreach (var v in SmoothingProfiles.AllProfiles)
                Console.WriteLine(v.Name);
            return;
        }

        var importProfile = SmoothingProfiles.ProfileByName(args[0]);
        if (importProfile == null)
            throw new Exception($"Unknown profile {args[0]}");

        var inRSIPath = args[argSrcRSI];
        var outPNGPath = args[argSrcRSI + 1];

        var metrics = new QuadMetrics(new Size(tileWidth, tileHeight), new Size(subtileWidth, subtileHeight));

        var importProfileInst = importProfile.Instance(metrics);
        var exportProfileInst = SmoothingProfiles.TestPattern.Instance(metrics);

        while (true)
        {
            Image<Rgba32>? inputImage = null;

            try
            {
                inputImage = await Image.LoadAsync<Rgba32>(inRSIPath);
            }
            catch (Exception)
            {
                // nevermind then
            }

            Rsi output;

            if (inputImage != null)
            {
                output = SmoothingWorkflow.Transform(inputImage, importProfileInst, "", "full", exportProfileInst);
            }
            else
            {
                var input = await Rsi.FromFolder(inRSIPath);
                await input.TryLoadFolderImages(inRSIPath);
                output = SmoothingWorkflow.Transform(input, "", importProfileInst, "", "full", exportProfileInst);
            }

            var fullImage = output.States[0].GetFullImage(output.Size);
            await fullImage.SaveAsPngAsync(outPNGPath);

            await Task.Delay(2000);
        }
    }
}

