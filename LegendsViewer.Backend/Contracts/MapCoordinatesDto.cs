using LegendsViewer.Backend.Legends.Various;

namespace LegendsViewer.Backend.Contracts;

public class MapCoordinatesDto
{
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }
    public List<LocationDto> Coordinates { get; set; } = [];
    public List<int> SiteIds { get; set; } = [];
}

public class LocationDto(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
}
