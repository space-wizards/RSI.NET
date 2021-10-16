using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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

        public static async Task<Rsi> FromFolder(
            string rsiFolder,
            JsonSerializerOptions? options = null,
            CancellationToken token = default)
        {
            var metaJsonPath = $"{rsiFolder}{Path.DirectorySeparatorChar}meta.json";
            await using var metaJsonStream = File.OpenRead(metaJsonPath);

            return await FromMetaJson(metaJsonStream, options, token);
        }

        public static async Task<Rsi> FromMetaJson(
            Stream metaJson,
            JsonSerializerOptions? options = null,
            CancellationToken token = default)
        {
            var rsi = await JsonSerializer.DeserializeAsync<Rsi>(metaJson, options, token);

            return rsi ?? throw new NullReferenceException();
        }

        public async Task TryLoadFolderImages(string rsiFolder)
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
                state.ImagePath = fileName;
            }
        }

        public async Task SaveToFolder(string rsiFolder, JsonSerializerOptions? options = null)
        {
            Directory.CreateDirectory(rsiFolder);

            await SaveImagesToFolder(rsiFolder);
            await SaveRsiToFolder(rsiFolder, options);
        }

        public async Task SaveToStream(Stream stream, JsonSerializerOptions? options = null)
        {
            await SaveImagesToStream(stream);
            await SaveRsiToStream(stream, options);
        }

        public async Task SaveImagesToFolder(string rsiFolder)
        {
            foreach (var state in States)
            {
                var image = state.GetFullImage(Size);
                var path = $"{rsiFolder}{Path.DirectorySeparatorChar}{state.Name}.png";

                if (state.ImagePath == null)
                {
                    await image.SaveAsPngAsync(path);
                }
                else
                {
                    File.Copy(state.ImagePath, path, true);
                }
            }
        }

        public async Task SaveImagesToStream(Stream stream)
        {
            foreach (var state in States)
            {
                var image = state.GetFullImage(Size);
                await image.SaveAsPngAsync(stream);
            }

            await stream.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
        }

        public async Task SaveRsiToFolder(string rsiFolder, JsonSerializerOptions? options = null)
        {
            var metaJsonPath = $"{rsiFolder}{Path.DirectorySeparatorChar}meta.json";
            await File.WriteAllTextAsync(metaJsonPath, string.Empty);

            await using var metaJsonFile = File.OpenWrite(metaJsonPath);
            await SaveRsiToStream(metaJsonFile, options);
        }

        public async Task SaveRsiToStream(Stream stream, JsonSerializerOptions? options = null)
        {
            options ??= new JsonSerializerOptions();
            options.Converters.Add(RsiStateConverter.Instance);

            await JsonSerializer.SerializeAsync(stream, this, options);
            await stream.FlushAsync();

            stream.Seek(0, SeekOrigin.Begin);
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
