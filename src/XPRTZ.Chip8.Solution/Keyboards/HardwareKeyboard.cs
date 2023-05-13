namespace XPRTZ.Chip8.Solution.Keyboards;

using System.Linq;
using Microsoft.Xna.Framework.Input;
using XPRTZ.Chip8.Solution.Interfaces;

public class HardwareKeyboard : IKeyboard
{
    private static byte KeyToByte(Keys key) =>
        key switch
        {
            Keys.NumPad1 => 0x1,
            Keys.NumPad2 => 0x2,
            Keys.NumPad3 => 0x3,
            Keys.C => 0xC,

            Keys.NumPad4 => 0x4,
            Keys.NumPad5 => 0x5,
            Keys.NumPad6 => 0x6,
            Keys.D => 0xD,

            Keys.NumPad7 => 0x7,
            Keys.NumPad8 => 0x8,
            Keys.NumPad9 => 0x9,
            Keys.E => 0xE,

            Keys.A => 0xA,
            Keys.NumPad0 => 0x0,
            Keys.B => 0xB,
            Keys.F => 0xF,
            _ => 0xFF,
        };

    public byte[] GetPressedKeys() =>
        Keyboard
        .GetState()
        .GetPressedKeys()
        .Select(key => KeyToByte(key))
        .Where(key => key is not 0xFF)
        .ToArray();
}
