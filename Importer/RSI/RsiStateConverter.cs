using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Importer.Directions;

namespace Importer.RSI
{
    public class RsiStateConverter : JsonConverter<RsiState>
    {
        public static readonly RsiStateConverter Instance = new();

        public override RsiState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, RsiState value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("name", value.Name);
            var nonDefaultDirection = value.Directions != DirectionType.None;
            if (nonDefaultDirection)
            {
                writer.WriteNumber("directions", (int)value.Directions);
            }

            var cleanedDelays = OmitDefaultDelays(value.Delays);
            if (cleanedDelays is { Count: > 0 })
            {
                writer.WriteStartArray("delays");

                foreach (var delays in cleanedDelays)
                {
                    writer.WriteStartArray();

                    foreach (var delay in delays)
                    {
                        writer.WriteNumberValue(delay);
                    }

                    writer.WriteEndArray();
                }

                writer.WriteEndArray();
            }

            if (value.Flags is { Count: > 0 })
            {
                writer.WriteStartObject("flags");

                foreach (var kvPair in value.Flags)
                {
                    writer.WriteString(kvPair.Key.ToString()!, kvPair.Value.ToString());
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        private List<List<float>> OmitDefaultDelays(List<List<float>>? valueDelays)
        {
            var cleanedDelays = new List<List<float>>();

            var allWereEmpty = true;
            if (valueDelays != null)
            {
                foreach (var delays in valueDelays)
                {
                    if (delays.Count != 1 || !delays[0].Equals(1f))
                    {
                        allWereEmpty = false;
                        cleanedDelays.Add(delays);
                    }
                    else
                    {
                        cleanedDelays.Add(new List<float>());
                    }
                }

                if (allWereEmpty)
                {
                    return new List<List<float>>();
                }
            }

            return cleanedDelays;
        }
    }
}
