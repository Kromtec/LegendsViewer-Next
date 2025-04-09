using LegendsViewer.Backend.Legends.Interfaces;

namespace LegendsViewer.Backend.Legends.Maps;

public interface IWorldMapImageGenerator
{
    byte[]? GenerateMapByteArray(int tileSize = WorldMapImageGenerator.DefaultTileSizeMid, int? depth = null, IHasCoordinates? objectWithCoordinates = null);
    void Clear();
    Task LoadExportedWorldMapAsync(string? bmpFilePath);
}