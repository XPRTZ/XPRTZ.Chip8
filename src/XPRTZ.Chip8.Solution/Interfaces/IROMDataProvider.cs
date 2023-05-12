namespace XPRTZ.Chip8.Solution.Interfaces;

using XPRTZ.Chip8.Solution.ROMData;

public interface IROMDataProvider
{
    ROMData GetROMData(string filename);
}
