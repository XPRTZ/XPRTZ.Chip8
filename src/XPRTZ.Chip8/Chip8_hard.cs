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
    private const byte _instructionSize = 2;
    private const byte _fontSize = 5;
    private const ushort _romStartAddress = 0x200;

    private readonly byte[] _memory = new byte[4096]; //4K memory

    public readonly byte[] V = new byte[16]; // Registers

    private readonly Stack<ushort> stack = new();

    private readonly Random _random = new();

    public byte DelayTimer { get; set; }

    public byte SoundTimer { get; set; }

    public ushort I { get; set; } // Addressing register

    public ushort ProgramCounter { get; set; }

    private readonly IKeyboard _keyboard;

    private readonly ISound _sound;

    private readonly IROMDataProvider _romDataProvider;

    private readonly IFont _font;

    public int ClockSpeed => RomMetadata.Options.Tickrate;

    private double _timerDelta => ClockSpeed / 60.0;

    private double _timerAccumulator = 0;

    public byte[] _soundBuffer = Array.Empty<byte>();

    public bool _waitingForKey = false;

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

        Screen.ClearScreen();

        ProgramCounter = _romStartAddress;
    }

    private void WriteWithCarry(ref byte register, byte value, byte carry)
    {
        register = (byte)(value & 0xFF);
        V[0xF] = carry;

        if (RomMetadata.Options.VFOrderQuirks)
        {
            V[register] = (byte)(value & 0xFF);
        }
    }

    private void TimerTick()
    {
        if (DelayTimer > 0)
        {
            DelayTimer--;
        }

        if (SoundTimer > 0)
        {
            SoundTimer--;

            if (SoundTimer > 0)
            {
                _sound.Play();
            }

            if (SoundTimer == 0)
            {
                _sound.Stop();
            }
        }
    }

    // https://laurencescotford.com/chip-8-on-the-cosmac-vip-index/
    public void Cycle()
    {
        // This is the code you will be extending to get the Chip8 implementation working.
        // Like a lot of emulators of the past there where a lot of emulators that "got it wrong".
        // This has lead to programs being written against faulty emulators, leading to the so called 
        // "quirks" settings. You can find and use these switches in "RomMetadata.Options", these will
        // be configured automatically when loading a ROM file.
        // You can ignore these quirks for now if you want, most progams included in this repo don't require them.
        // In the Program.cs file you can switch out the roms. When you're done try some out!
        // Personal favourites: Breakout and Cave Explorer :)

        var opcode = (_memory[ProgramCounter] << 8) | _memory[(ushort)(ProgramCounter + 1)];

        ProgramCounter += _instructionSize; //All instructions are 2 byte long

        // TODO: Implement this, good luck!

        _timerAccumulator++;

        if (_timerAccumulator > _timerDelta)
        {
            _timerAccumulator -= _timerDelta;
            TimerTick();
        }
    }
}
