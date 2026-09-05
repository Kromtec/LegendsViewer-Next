using LegendsViewer.Backend.Legends.Interfaces;

namespace LegendsViewer.Backend.Legends.Extensions;

public static class IHasCoordinatesExtensions
{
    public static int MinX(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.Coordinates.Min(c => c.X) : 0;
    }

    public static int MaxX(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.Coordinates.Max(c => c.X) : 0;
    }

    public static int MinY(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.Coordinates.Min(c => c.Y) : 0;
    }

    public static int MaxY(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.Coordinates.Max(c => c.Y) : 0;
    }

    public static int Width(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.MaxX() + 1 - obj.MinX() : 0;
    }

    public static int Height(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.MaxY() + 1 - obj.MinY() : 0;
    }

    public static double CenterX(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.Coordinates.Average(c => c.X) : 0;
    }

    public static double CenterY(this IHasCoordinates obj)
    {
        return obj.Coordinates != null && obj.Coordinates.Count > 0 ? obj.Coordinates.Average(c => c.Y) : 0;
    }
}
