namespace XPRTZ.Chip8.Solution;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XPRTZ.Chip8.Solution.Interfaces;
using XPRTZ.Chip8.Solution.ROMData;

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
        var opcode = (_memory[ProgramCounter] << 8) | _memory[(ushort)(ProgramCounter + 1)];

        ProgramCounter += _instructionSize; //All instructions are 2 byte long

        switch (
            (opcode & 0xF000) >> 12,
            (opcode & 0x0F00) >> 8,
            (opcode & 0x00F0) >> 4,
            opcode & 0x000F)
        {
            // 00E0: Clear the screen
            case (0x0, 0x0, 0xE, 0x0):
                Screen.ClearScreen();
                break;

            // 00EE: Return from subroutine
            case (0x0, 0x0, 0xE, 0xE):
                ProgramCounter = stack.Pop();
                break;

            // 0NNN: Execute RCA 1802 machine language routine at address NNN
            case (0x0, _, _, _):
                throw new NotImplementedException();

            // 1NNN: Jump to address NNN
            case (0x1, _, _, _):
                ProgramCounter = (ushort)(opcode & 0x0FFF);
                break;

            // 2NNN: Call subroutine at address NNN
            case (0x2, _, _, _):
                stack.Push(ProgramCounter);
                ProgramCounter = (ushort)(opcode & 0x0FFF);
                break;

            // 3XNN: Skip the following instruction if the value of register VX equals NN
            case (0x3, var x, _, _):
                if (V[x] == (byte)(opcode & 0x00FF))
                {
                    ProgramCounter += _instructionSize;
                }

                break;

            // 4XNN: Skip the following instruction if the value of register VX is not equal to NN
            case (0x4, var x, _, _):
                if (V[x] != (byte)(opcode & 0x00FF))
                {
                    ProgramCounter += _instructionSize;
                }

                break;

            // 5XY0: Skip the following instruction if the value of register VX is equal to the value of register VY
            case (0x5, var x, var y, 0x0):
                if (V[x] == V[y])
                {
                    ProgramCounter += _instructionSize;
                }

                break;

            // 6XNN: Set VX to NN
            case (0x6, var x, _, _):
                V[x] = (byte)(opcode & 0x00FF);
                break;

            // 7XNN: Add NN to VX
            case (0x7, var x, _, _):
                V[x] += (byte)(opcode & 0x00FF);
                break;

            // 8XY0: Set VX to the value in VY
            case (0x8, var x, var y, 0x0):
                V[x] = V[y];
                break;

            // 8XY1: Set VX to VX OR VY
            case (0x8, var x, var y, 0x1):
                V[x] |= V[y];

                if (RomMetadata.Options.LogicQuirks)
                {
                    V[0xF] = 0;
                }

                break;

            // 8XY2: Set VX to VX AND VY
            case (0x8, var x, var y, 0x2):
                V[x] &= V[y];

                if (RomMetadata.Options.LogicQuirks)
                {
                    V[0xF] = 0;
                }

                break;

            // 8XY3: Set VX to VX XOR VY
            case (0x8, var x, var y, 0x3):
                V[x] ^= V[y];

                if (RomMetadata.Options.LogicQuirks)
                {
                    V[0xF] = 0;
                }

                break;

            // 8XY4: Add the value of register VY to register VX.Set VF to 01 if a carry occurs.Set VF to 00 if a carry does not occur
            case (0x8, var x, var y, 0x4):

                var newValue_84 = V[x] + V[y];

                WriteWithCarry(
                    ref V[x],
                    (byte)newValue_84,
                    (byte)(newValue_84 > 0xFF ? 01 : 00));
                break;

            // 8XY5: Subtract the value of register VY from register VX.Set VF to 00 if a borrow occurs.Set VF to 01 if a borrow does not occur
            case (0x8, var x, var y, 0x5):

                var newValue_85 = (byte)(V[x] - V[y]);

                WriteWithCarry(
                    ref V[x],
                    newValue_85,
                    (byte)(V[y] > V[x] ? 00 : 01));

                break;

            // 8XY6: Store the value of register VY shifted right one bit in register VX.Set register VF to the least significant bit prior to the shift
            case (0x8, var x, var y, 0x6):

                if (RomMetadata.Options.ShiftQuirks)
                {
                    y = x;
                }

                var newValue_86 = (byte)(V[y] >> 1);

                WriteWithCarry(
                    ref V[x],
                    newValue_86,
                    (byte)(V[y] & 0x1));
                break;

            // 8XY7: Set register VX to the value of VY minus VX. Set VF to 00 if a borrow occurs.Set VF to 01 if a borrow does not occur
            case (0x8, var x, var y, 0x7):

                var newValue_87 = (byte)(V[y] - V[x]);

                WriteWithCarry(
                    ref V[x],
                    newValue_87,
                    (byte)(V[x] > V[y] ? 00 : 01));
                break;

            // 8XYE: Store the value of register VY shifted left one bit in register VX. Set register VF to the most significant bit prior to the shift
            case (0x8, var x, var y, 0xE):

                if (RomMetadata.Options.ShiftQuirks)
                {
                    y = x;
                }

                var newValue_8E = (byte)(V[y] << 1);

                WriteWithCarry(
                    ref V[x],
                    newValue_8E,
                    (byte)((V[x] >> 7) & 0x01));
                break;

            // 9XY0: Skip the following instruction if the value of register VX is not equal to the value of register VY
            case (0x9, var x, var y, 0x0):
                if (V[x] != V[y])
                {
                    ProgramCounter += _instructionSize;
                }

                break;

            // ANNN: Store memory address NNN in register I
            case (0xA, _, _, _):
                I = (ushort)(opcode & 0x0FFF);
                break;

            // BNNN: Jump to address NNN +V0
            case (0xB, _, _, _):
                ProgramCounter = (ushort)((opcode & 0x0FFF) + V[0]);
                break;

            // CXNN: Set VX to a random number with a mask of NN
            case (0xC, var x, _, _):
                V[x] = (byte)(_random.Next(0, byte.MaxValue) & opcode & 0x00FF);
                break;

            // DXYN: Draw a sprite at position VX, VY with N bytes of sprite data starting at the address stored in I.Set VF to 01 if any set pixels are changed to unset, and 00 otherwise
            case (0xD, var x, var y, var spriteDataSize):
                var sprite = _memory.AsSpan(I, spriteDataSize);

                var startX = V[x] % Screen.Width;
                var startY = V[y] % Screen.Height;
                V[0xF] = 00;

                for (var row = 0; row < spriteDataSize; row++)
                {
                    var chunk = sprite[row];

                    for (var column = 7; column >= 0; column--)
                    {
                        var pixel = (byte)((chunk >> column) & 0x1);

                        var drawX = startX + (7 - column);
                        var drawY = startY + row;

                        if (drawX < Screen.Width && drawY < Screen.Height)
                        {
                            V[0xF] |= (byte)(pixel & Screen[drawX, drawY]);
                            Screen[drawX, drawY] ^= pixel;
                        }
                    }
                }

                break;

            // EX9E: Skip the following instruction if the key corresponding to the hex value currently stored in register VX is pressed
            case (0xE, var x, 0x9, 0xE):
                if (_keyboard.GetPressedKeys().Contains(V[x]))
                {
                    ProgramCounter += _instructionSize;
                }

                break;

            // EXA1: Skip the following instruction if the key corresponding to the hex value currently stored in register VX is not pressed
            case (0xE, var x, 0xA, 0x1):
                if (!_keyboard.GetPressedKeys().Contains(V[x]))
                {
                    ProgramCounter += _instructionSize;
                }

                break;

            // FX07: Store the current value of the delay timer in register VX
            case (0xF, var x, 0x0, 0x7):
                V[x] = DelayTimer;
                break;

            // FX0A: Wait for a keypress and store the result in register VX
            case (0xF, var x, 0x0, 0xA):

                var pressedKeys = _keyboard.GetPressedKeys();

                if (!_waitingForKey)
                {
                    if (pressedKeys.Any())
                    {
                        _waitingForKey = true;
                        V[x] = pressedKeys.First();
                    }

                    ProgramCounter -= _instructionSize;
                }
                else
                {
                    if (pressedKeys.Any(key => key == V[x]))
                    {
                        ProgramCounter -= _instructionSize;
                    }
                    else
                    {
                        _waitingForKey = false;
                    }
                }

                break;

            // FX15: Set the delay timer to the value of register VX
            case (0xF, var x, 0x1, 0x5):
                DelayTimer = V[x];
                break;

            // FX18: Set the sound timer to the value of register VX
            case (0xF, var x, 0x1, 0x8):
                SoundTimer = V[x];
                break;

            // FX1E: Add the value stored in register VX to register I
            case (0xF, var x, 0x1, 0xE):
                I += V[x];
                break;

            // FX29: Set I to the memory address of the font data corresponding to the hexadecimal digit stored in register VX
            case (0xF, var x, 0x2, 0x9):
                I = (byte)(V[x] * _fontSize);
                break;

            // FX33: Store the binary - coded decimal equivalent of the value stored in register VX at addresses I, I + 1, and I + 2
            case (0xF, var x, 0x3, 0x3):
                _memory[I] = (byte)(V[x] / 100);
                _memory[(ushort)(I + 1)] = (byte)(V[x] / 10 % 10);
                _memory[(ushort)(I + 2)] = (byte)(V[x] % 10);
                break;

            // FX55: Store the values of registers V0 to VX inclusive in memory starting at address I.I is set to I + X + 1 after operation
            case (0xF, var x, 0x5, 0x5):
                for (uint i = 0; i <= x; i++)
                {
                    //TODO: Overflow check
                    _memory[(ushort)(I + i)] = V[i];
                }

                if (!RomMetadata.Options.LoadStoreQuirks)
                {
                    I += (ushort)(x + 1);
                }

                break;

            // FX65: Fill registers V0 to VX inclusive with the values stored in memory starting at address I.I is set to I + X + 1 after operation
            case (0xF, var x, 0x6, 0x5):
                for (uint i = 0; i <= x; i++)
                {
                    //TODO: Overflow check
                    V[i] = _memory[(ushort)(I + i)];
                }

                if (!RomMetadata.Options.LoadStoreQuirks)
                {
                    I += (ushort)(x + 1);
                }

                break;

            default:
                throw new NotImplementedException($"Unknown opcode: {opcode}");
        }

        _timerAccumulator++;

        if (_timerAccumulator > _timerDelta)
        {
            _timerAccumulator -= _timerDelta;
            TimerTick();
        }
    }
}
