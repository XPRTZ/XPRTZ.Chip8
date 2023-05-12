namespace XPRTZ.Chip8.ROMData.JsonConverters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using XPRTZ.Chip8.ROMData;

internal class PlatformConverter : JsonConverter<Platform>
{
    public override Platform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            switch (reader.GetString())
            {
                case "chip8": return Platform.Chip8;
                case "schip": return Platform.SuperChip;
                case "xochip": return Platform.XOChip;
            }
        }

        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Platform value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case Platform.Chip8:
                writer.WriteStringValue("chip8");
                break;

            case Platform.SuperChip:
                writer.WriteStringValue("schip");
                break;

            case Platform.XOChip:
                writer.WriteStringValue("xochip");
                break;

            default:
                throw new NotSupportedException();
        }
    }
}
