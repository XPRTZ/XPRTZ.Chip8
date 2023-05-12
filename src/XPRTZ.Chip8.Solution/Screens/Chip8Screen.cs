namespace XPRTZ.Chip8.Solution.Screens;

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XPRTZ.Chip8.Solution.Interfaces;

internal class Chip8Screen : IScreen
{
    private readonly byte[] _frameBuffer;

    public Chip8Screen() => _frameBuffer = new byte[Width * Height];

    public byte this[int x, int y]
    {
        get => _frameBuffer[x + (y * Width)];

        set => _frameBuffer[x + (y * Width)] = value;
    }

    public int Width => 64;

    public int Height => 32;

    public void Blit(Texture2D canvas, Color backGroundColor, Color foreGroundColor) => canvas.SetData(_frameBuffer.Select(pixel => pixel == 1 ? foreGroundColor : backGroundColor).ToArray());

    public void ClearScreen() => Array.Clear(_frameBuffer);
}
