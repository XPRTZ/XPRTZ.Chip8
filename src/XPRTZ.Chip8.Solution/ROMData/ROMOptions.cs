namespace XPRTZ.Chip8.Solution.ROMData;

using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using XPRTZ.Chip8.Solution.ROMData.JsonConverters;
using ColorConverter = JsonConverters.ColorConverter;

public sealed record ROMOptions
{
    public int Tickrate { get; set; }

    [JsonConverter(typeof(ColorConverter))]
    public Color FillColor { get; set; }

    [JsonConverter(typeof(ColorConverter))]
    public Color BackgroundColor { get; set; }

    [JsonConverter(typeof(ColorConverter))]
    public Color BuzzColor { get; set; }

    [JsonConverter(typeof(ColorConverter))]
    public Color QuietColor { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool ShiftQuirks { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool LoadStoreQuirks { get; set; }

    [JsonConverter(typeof(ColorConverter))]
    public Color FillColor2 { get; set; }

    [JsonConverter(typeof(ColorConverter))]
    public Color BlendColor { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool VFOrderQuirks { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool EnableXO { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool ClipQuirks { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool JumpQuirks { get; set; }

    public int ScreenRotation { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool VBlankQuirks { get; set; }

    [JsonConverter(typeof(BooleanConverter))]
    public bool LogicQuirks { get; set; }

    public int MaxSize { get; set; }

    public string TouchInputMode { get; set; } = string.Empty;

    public string FontStyle { get; set; } = string.Empty;

    public string DisplayScale { get; set; } = string.Empty;
}
