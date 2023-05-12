namespace XPRTZ.Chip8.Keyboards;

using System.Linq;
using Microsoft.Xna.Framework.Input;
using XPRTZ.Chip8.Interfaces;

public class HardwareKeyboard : IKeyboard
{
    private static byte KeyToByte(Keys key) =>
        key switch
        {
            Keys.D1 => 0x1,
            Keys.D2 => 0x2,
            Keys.D3 => 0x3,
            Keys.C => 0xC,

            Keys.D4 => 0x4,
            Keys.D5 => 0x5,
            Keys.D6 => 0x6,
            Keys.D => 0xD,

            Keys.D7 => 0x7,
            Keys.D8 => 0x8,
            Keys.D9 => 0x9,
            Keys.E => 0xE,

            Keys.A => 0xA,
            Keys.D0 => 0x0,
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
