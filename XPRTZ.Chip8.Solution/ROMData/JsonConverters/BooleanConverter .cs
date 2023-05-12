namespace XPRTZ.Chip8.Solution.ROMData.JsonConverters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class BooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32() > 0,
            JsonTokenType.False or JsonTokenType.True => reader.GetBoolean(),
            _ => throw new NotSupportedException(),
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}
