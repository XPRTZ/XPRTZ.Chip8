namespace XPRTZ.Chip8.ROMData.JsonConverters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

internal class ColorConverter : JsonConverter<Color>
{
    private Color GetColor(string colorName)
    {
        foreach (var property in typeof(Color).GetProperties())
        {
            if (property.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase))
            {
                return (Color)property.GetValue(null, null)!;
            }
        }

        return Color.Transparent;
    }

    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number: return new Color(reader.GetUInt32());
            case JsonTokenType.String:
                var stringValue = reader.GetString();

                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return Color.Transparent;
                }

                if (stringValue.StartsWith('#'))
                {
                    stringValue = stringValue.TrimStart('#');

                    if (stringValue.Length >= 6)
                    {
                        var red = Convert.ToInt32(stringValue[..2], 16);
                        var green = Convert.ToInt32(stringValue[2..4], 16);
                        var blue = Convert.ToInt32(stringValue[4..6], 16);

                        return new Color(red, green, blue);
                    }

                    return Color.Transparent;
                }

                return GetColor(stringValue);
        }

        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) => writer.WriteStringValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
}
