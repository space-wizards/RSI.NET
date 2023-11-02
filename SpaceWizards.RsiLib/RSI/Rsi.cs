using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SpaceWizards.RsiLib.Directions;

namespace SpaceWizards.RsiLib.RSI;

[PublicAPI]
public sealed class Rsi : IDisposable
{
    public const int CurrentRsiVersion = 1;

    public Rsi(
        int version,
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
        OriginalStateNames = new HashSet<string>(States.Select(s => s.Name));
    }

    public Rsi(
        int version = CurrentRsiVersion,
        string? license = null,
        string? copyright = null,
        int x = 32,
        int y = 32,
        List<RsiState>? states = null)
        : this(version, new RsiSize(x, y), states?.ToList(), license, copyright)
    {
    }

    public int Version { get; set; }

    public string? License { get; set; }

    public string? Copyright { get; set; }

    public RsiSize Size { get; set; }

    public List<RsiState> States { get; set; }

    public HashSet<string> OriginalStateNames { get; set; }

    public static Rsi FromFolder(
        string rsiFolder,
        JsonSerializerOptions? options = null)
    {
        var metaJsonPath = Path.Combine(rsiFolder, "meta.json");
        using var metaJsonStream = File.OpenRead(metaJsonPath);

        return FromMetaJson(metaJsonStream, options);
    }

    public static Rsi FromMetaJson(
        Stream metaJson,
        JsonSerializerOptions? options = null)
    {
        var rsiData = JsonSerializer.Deserialize(metaJson, RsiJsonSourceGenerationContext.Default.RsiJsonData);
        if (rsiData == null)
            throw new JsonException();

        var states = rsiData.States.Select(x => new RsiState(
                x.Name, x.Directions ?? DirectionType.None, x.Delays?.Select(y => y.ToList()).ToList(), x.Flags, null))
            .ToList();

        return new Rsi(rsiData.Version, rsiData.Size, states, rsiData.License, rsiData.Copyright);
    }

    public void TryLoadFolderImages(string rsiFolder)
    {
        if (!Directory.Exists(rsiFolder))
            return;

        foreach (var state in States)
        {
            var fileName = Path.Combine(rsiFolder, $"{state.Name}.png");

            if (!File.Exists(fileName))
            {
                continue;
            }

            var image = Image.Load<Rgba32>(fileName);
            state.LoadImage(image, Size);
            state.ImagePath = fileName;
        }
    }

    public void SaveToFolder(string rsiFolder)
    {
        Directory.CreateDirectory(rsiFolder);

        SaveImagesToFolder(rsiFolder);
        SaveMetadataToFolder(rsiFolder);
    }

    public void SaveImagesToFolder(string rsiFolder)
    {
        foreach (var state in States)
        {
            OriginalStateNames.Remove(state.Name);

            var image = state.GetFullImage(Size);
            var path = Path.Combine(rsiFolder, $"{state.Name}.png");

            if (state.ImagePath == null || !File.Exists(state.ImagePath))
            {
                image.SaveAsPng(path);
            }
            else if (state.ImagePath != path)
            {
                File.Copy(state.ImagePath, path, true);
            }
        }

        foreach (var name in OriginalStateNames)
        {
            var path = Path.Combine(rsiFolder, $"{name}.png");
            File.Delete(path);
        }

        OriginalStateNames.Clear();
        OriginalStateNames.UnionWith(States.Select(s => s.Name));
    }

    public void SaveMetadataToFolder(string rsiFolder)
    {
        var metaJsonPath = Path.Combine(rsiFolder, "meta.json");

        using var metaJsonFile = File.Create(metaJsonPath);
        SaveMetadataToStream(metaJsonFile);
    }

    public void SaveMetadataToStream(Stream stream)
    {
        var statesData = States.Select(x =>
        {
            var state = new RsiStateJsonData(x.Name);
            if (x.Directions != DirectionType.None)
                state.Directions = x.Directions;

            var delays = OmitDefaultDelays(x.Delays);
            if (delays != null)
                state.Delays = delays;

            if (x.Flags is { Count: > 0 })
                state.Flags = x.Flags;

            return state;
        }).ToArray();

        var data = new RsiJsonData(Version, License, Copyright, Size, statesData);
        JsonSerializer.Serialize(stream, data, RsiJsonSourceGenerationContext.Default.RsiJsonData);
    }

    public void Dispose()
    {
        foreach (var state in States)
        {
            state.Dispose();
        }
    }

    private static float[][]? OmitDefaultDelays(List<List<float>>? valueDelays)
    {
        if (valueDelays == null)
            return null;

        var cleanedDelays = new float[valueDelays.Count][];
        var allWereEmpty = true;

        for (var i = 0; i < valueDelays.Count; i++)
        {
            var srcDelays = valueDelays[i];
            if (srcDelays is [1f])
            {
                cleanedDelays[i] = Array.Empty<float>();
            }
            else
            {
                cleanedDelays[i] = srcDelays.ToArray();
                allWereEmpty = false;
            }
        }

        if (allWereEmpty)
            return null;

        return cleanedDelays;
    }
}
