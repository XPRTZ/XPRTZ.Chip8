namespace XPRTZ.Chip8.Interfaces;

using System;

public interface ISound : IDisposable
{
    public void InitializeSoundBuffer(int frequency, int samplesPerSecond);

    public void Play();

    public void Stop();
}
