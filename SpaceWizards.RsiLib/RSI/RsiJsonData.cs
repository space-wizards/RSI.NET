using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceWizards.RsiLib.Directions;

namespace SpaceWizards.RsiLib.RSI;

// Model declarations for the JSON data.

internal sealed class RsiJsonData
{
    [JsonRequired]
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("copyright")]
    public string? Copyright { get; set; }

    [JsonRequired]
    [JsonPropertyName("size")]
    public RsiSize Size { get; set; }

    [JsonRequired]
    [JsonPropertyName("states")]
    public RsiStateJsonData[] States { get; set; }
    
    [JsonConstructor]
    public RsiJsonData(int version, string? license, string? copyright, RsiSize size, RsiStateJsonData[] states)
    {
        Version = version;
        License = license;
        Copyright = copyright;
        Size = size;
        States = states;
    }
}

internal sealed class RsiStateJsonData
{
    [JsonRequired]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("directions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DirectionType? Directions { get; set; }

    [JsonPropertyName("delays")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float[][]? Delays { get; set; }

    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Flags { get; set; }

    [JsonConstructor]
    public RsiStateJsonData(string name)
    {
        Name = name;
    }
}

[JsonSerializable(typeof(RsiJsonData))]
// This is to pass through arbitrary JSON data in the flags field.
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class RsiJsonSourceGenerationContext : JsonSerializerContext
{
    
}