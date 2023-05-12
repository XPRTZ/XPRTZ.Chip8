namespace XPRTZ.Chip8;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XPRTZ.Chip8.Interfaces;
using XPRTZ.Chip8.ROMData;
// https://chip-8.github.io/

public class Chip8
{
    private const byte _fontSize = 5;
    private const ushort _romStartAddress = 0x200;

    private readonly byte[] _memory = new byte[4096]; //4K memory

    private readonly Stack<ushort> _stack = new();

    public readonly byte[] V = new byte[16]; // Registers

    public byte DelayTimer { get; set; }

    public byte SoundTimer { get; set; }

    public ushort I { get; set; } // Addressing register

    public ushort ProgramCounter { get; set; }

    private readonly IKeyboard _keyboard;

    private readonly ISound _sound;

    private readonly IROMDataProvider _romDataProvider;

    private readonly IFont _font;

    public int ClockSpeed => RomMetadata.Options.Tickrate;

    public byte[] _soundBuffer = Array.Empty<byte>();

    public ROMMetadata RomMetadata { get; private set; } = new();

    public IScreen Screen { get; init; }

    public Chip8(
        IKeyboard keyboard,
        IScreen screen,
        ISound sound,
        IFont font,
        IROMDataProvider romDataProvider)
    {
        _keyboard = keyboard;
        Screen = screen;
        _sound = sound;
        _font = font;
        _romDataProvider = romDataProvider;

        // https://oldcomputermuseum.com/cosmac_vip.html
        _sound.InitializeSoundBuffer(1400, 8000);

        Array.Copy(_font.FontData, font.FontData, font.FontData.Length);
    }

    public void LoadRom(string path)
    {
        Array.Clear(_memory);
        Array.Clear(V);
        Screen.ClearScreen();

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found.", path);
        }

        var romData = _romDataProvider.GetROMData(path);

        if (romData.Rom.Length > romData.Metadata.Options.MaxSize)
        {
            throw new OutOfMemoryException($"ROM size to large, maximum ROM size supported is: {romData.Metadata.Options.MaxSize} bytes.");
        }

        // https://laurencescotford.com/chip-8-ram-or-memory-management-with-chip-8/
        Array.Copy(romData.Rom, 0, _memory, _romStartAddress, romData.Rom.Length);

        RomMetadata = romData.Metadata;

        ProgramCounter = _romStartAddress;
    }

    // https://laurencescotford.com/chip-8-on-the-cosmac-vip-index/
    public void Cycle()
    {
        // TODO:
        // Decode the current opcode
        // Execute instruction
    }
}
