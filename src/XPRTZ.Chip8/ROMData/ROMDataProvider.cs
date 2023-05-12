namespace XPRTZ.Chip8.ROMData;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using XPRTZ.Chip8.Interfaces;

public class ROMDataProvider : IROMDataProvider
{
    private readonly IDictionary<string, ROMMetadata> _metaData;
    private readonly IDictionary<string, string> _hashes;

    private static readonly Dictionary<Platform, ROMOptions> _defaultOptions = new()
    {
        {
            Platform.Chip8, new ROMOptions
            {
                FillColor = Color.White,
                BackgroundColor = Color.Black,
                ShiftQuirks = false,
                LoadStoreQuirks = false,
                ClipQuirks = true,
                JumpQuirks = false,
                LogicQuirks = true,
                VBlankQuirks = true,
                Tickrate = 500,
                MaxSize = 3215
            }
        },
        {
            Platform.SuperChip, new ROMOptions
            {
                FillColor = Color.White,
                BackgroundColor = Color.Black,
                ShiftQuirks = true,
                LoadStoreQuirks = true,
                ClipQuirks = true,
                JumpQuirks = true,
                LogicQuirks = false,
                VBlankQuirks = false,
                Tickrate = 500,
                MaxSize = 3583
            }
        },
        {
            Platform.Octo, new ROMOptions
            {
                FillColor = Color.White,
                BackgroundColor = Color.Black,
                ShiftQuirks = false,
                LoadStoreQuirks = false,
                ClipQuirks = false,
                JumpQuirks = false,
                LogicQuirks = false,
                VBlankQuirks = false,
                Tickrate = 500,
                MaxSize = 3584
            }
        },
        {
            Platform.XOChip, new ROMOptions
            {
                FillColor = Color.White,
                BackgroundColor = Color.Black,
                ShiftQuirks = false,
                LoadStoreQuirks = false,
                ClipQuirks = false,
                JumpQuirks = false,
                LogicQuirks = false,
                VBlankQuirks = false,
                Tickrate = 500,
                MaxSize = 65024
            }
        }
    };

    public ROMDataProvider()
    {
        var jsonString = File.ReadAllText("./ROMData/programs.json");

        _metaData = JsonSerializer.Deserialize<IDictionary<string, ROMMetadata>>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        }) ?? new Dictionary<string, ROMMetadata>();

        jsonString = File.ReadAllText("./ROMData/hashes.json");
        _hashes = JsonSerializer.Deserialize<IDictionary<string, string>>(jsonString) ?? new Dictionary<string, string>();
    }

    public void UpdateMetadata(string hash, ROMMetadata metadata)
    {
        _hashes[hash] = metadata.Key;
        _metaData[metadata.Key] = metadata;
    }

    public IReadOnlyDictionary<string, string> GetHashes() => _hashes.ToImmutableSortedDictionary();

    public IReadOnlyDictionary<string, ROMMetadata> GetMetadata() => _metaData.ToImmutableSortedDictionary();

    private ROMMetadata GenerateMetadata(string filename, string hash)
    {
        var fileInfo = new FileInfo(filename);

        var title = fileInfo.Name.Replace(fileInfo.Extension, "");

        var regex = Regex.Match(title, @"\[(.*)\]");

        var authors = new List<string>();
        string? release = null;

        if (regex.Success)
        {
            var metadataValues = regex.Groups[1].Value.Split(',');
            authors.Add(metadataValues[0]);

            if (metadataValues.Length > 1)
            {
                release = metadataValues[1];
            }

            title = title.Replace(regex.Value, string.Empty).Trim();
        }

        var platform = fileInfo.Extension switch
        {
            ".8o" => Platform.XOChip,
            ".sch8" => Platform.SuperChip,
            _ => Platform.Chip8
        };

        var metadata = new ROMMetadata
        {
            Authors = authors,
            Desc = string.Empty,
            Event = string.Empty,
            Images = new List<string>(),
            Platform = platform,
            Release = release ?? string.Empty,
            Title = title,
            Options = _defaultOptions[platform]
        };

        _hashes[hash] = metadata.Key;
        _metaData[metadata.Key] = metadata;

        return metadata;
    }

    public ROMData GetROMData(string filename)
    {
        var romData = File.ReadAllBytes(filename);
        var hash = GenerateHash(romData);

        return _hashes.TryGetValue(hash, out var key) &&
            _metaData.TryGetValue(key, out var metadata)
            ? new ROMData
            {
                Metadata = metadata,
                Rom = romData
            }
            : new ROMData
            {
                Metadata = GenerateMetadata(filename, hash),
                Rom = romData
            };
    }

    private string GenerateHash(byte[] romData)
    {
        string hash;

        using (var hashFunction = SHA256.Create())
        {
            hash = BitConverter.ToString(hashFunction.TransformFinalBlock(romData, 0, romData.Length));
        }

        return hash;
    }
}
