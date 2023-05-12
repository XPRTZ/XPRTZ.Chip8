namespace XPRTZ.Chip8.Solution.ROMData;

using System;

public record ROMData
{
    public ROMMetadata Metadata { get; init; } = new ROMMetadata();

    public byte[] Rom { get; init; } = Array.Empty<byte>();
}
