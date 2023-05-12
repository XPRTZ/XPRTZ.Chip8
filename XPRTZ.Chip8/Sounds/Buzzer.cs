namespace XPRTZ.Chip8.Sounds;

using System;
using Microsoft.Xna.Framework.Audio;
using XPRTZ.Chip8.Interfaces;

internal class Buzzer : ISound
{
    private byte[] _soundBuffer = Array.Empty<byte>();

    private SoundEffect? _soundEffect;

    private SoundEffectInstance? _soundEffectInstance;
    private bool _disposedValue;

    // https://laurencescotford.com/chip-8-on-the-cosmac-vip-sound/
    public void InitializeSoundBuffer(int frequency, int samplesPerSecond)
    {
        _soundBuffer = new byte[SoundEffect.GetSampleSizeInBytes(TimeSpan.FromSeconds(0xFF / 60), samplesPerSecond, AudioChannels.Mono)];

        var theta = frequency * Math.Tau / samplesPerSecond;

        int bufferIndex;

        for (var step = 0; step < _soundBuffer.Length / 2; step++)
        {
            var floatSample = Math.Sign(Math.Sin(theta * step));

            var sample = (short)(floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

            bufferIndex = step * 2;

            if (BitConverter.IsLittleEndian)
            {
                _soundBuffer[bufferIndex] = (byte)sample;
                _soundBuffer[bufferIndex + 1] = (byte)(sample >> 8);
            }
            else
            {
                _soundBuffer[bufferIndex + 1] = (byte)(sample >> 8);
                _soundBuffer[bufferIndex] = (byte)sample;
            }
        }

        _soundEffect = new SoundEffect(_soundBuffer, samplesPerSecond, AudioChannels.Mono);
        _soundEffectInstance = _soundEffect.CreateInstance();

        _soundEffectInstance.IsLooped = true;
    }

    public void Play() => _soundEffectInstance?.Play();

    public void Stop() => _soundEffectInstance?.Stop();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _soundEffectInstance?.Dispose();
                _soundEffect?.Dispose();
            }

            _disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
