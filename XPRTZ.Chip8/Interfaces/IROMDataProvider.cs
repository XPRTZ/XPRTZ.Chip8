namespace XPRTZ.Chip8.Interfaces;

using XPRTZ.Chip8.ROMData;

public interface IROMDataProvider
{
    ROMData GetROMData(string filename);
}
