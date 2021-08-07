using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            writer.WriteNumber("directions", (int) value.Directions);

            if (value.Delays is {Count: > 0})
            {
                writer.WriteStartArray("delays");

                foreach (var delays in value.Delays)
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

            if (value.Flags is {Count: > 0})
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
    }
}
