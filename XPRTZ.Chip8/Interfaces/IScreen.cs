namespace XPRTZ.Chip8.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface IScreen
{
    byte this[int x, int y] { get; set; }

    int Width { get; }

    int Height { get; }

    void ClearScreen();

    void Blit(Texture2D canvas, Color backGroundColor, Color foreGroundColor);
}
