namespace XPRTZ.Chip8.ROMData;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using XPRTZ.Chip8.ROMData.JsonConverters;

public sealed class ROMMetadata
{
    private string? _key = null;

    [JsonIgnore]
    public string Key
    {
        get
        {
            // Remove all whitespace from string:
            _key ??= Regex.Replace(Title?.ToLowerInvariant() ?? string.Empty, @"\s+", string.Empty);

            return _key;
        }
    }

    public string Hash { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public IEnumerable<string> Authors { get; init; } = Array.Empty<string>();

    public IEnumerable<string> Images { get; init; } = Array.Empty<string>();

    public string Desc { get; init; } = string.Empty;

    public string Event { get; init; } = string.Empty;

    public string Release { get; init; } = string.Empty;

    [JsonConverter(typeof(PlatformConverter))]
    public Platform Platform { get; init; }

    public ROMOptions Options { get; init; } = new();
}
