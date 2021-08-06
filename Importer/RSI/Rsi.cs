using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Importer.RSI
{
    [PublicAPI]
    public class Rsi : IDisposable
    {
        public const double CurrentRsiVersion = 1;

        [JsonConstructor]
        public Rsi(
            double version,
            RsiSize size,
            List<RsiState>? states = null,
            string? license = null,
            string? copyright = null)
        {
            Version = version;
            License = license;
            Copyright = copyright;
            Size = size;
            States = states ?? new List<RsiState>();
        }

        public Rsi(
            double version = CurrentRsiVersion,
            string? license = null,
            string? copyright = null,
            int x = 32,
            int y = 32,
            List<RsiState>? states = null)
            : this(version, new RsiSize(x, y), states?.ToList(), license, copyright)
        {
        }

        [JsonPropertyName("version")]
        public double Version { get; set; }

        [JsonPropertyName("license")]
        public string? License { get; set; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; }

        [JsonPropertyName("size")]
        public RsiSize Size { get; }

        [JsonPropertyName("states")]
        public List<RsiState> States { get; set; }

        public async Task LoadFolderImages(string rsiFolder)
        {
            if (!Directory.Exists(rsiFolder))
            {
                return;
            }

            foreach (var state in States)
            {
                var fileName = $"{rsiFolder}{Path.DirectorySeparatorChar}{state.Name}.png";

                if (!File.Exists(fileName))
                {
                    continue;
                }

                var image = await Image.LoadAsync<Rgba32>(fileName);
                state.LoadImage(image, Size);
            }
        }

        public async Task SaveTo(string rsiFolder)
        {
            foreach (var state in States)
            {
                var image = state.GetFullImage(Size);

                await image.SaveAsync($"{rsiFolder}{Path.DirectorySeparatorChar}{state.Name}.png");
            }
        }

        public void Dispose()
        {
            foreach (var state in States)
            {
                state.Dispose();
            }
        }
    }
}
