namespace LegendsViewer.Backend.Contracts;

public class EventFilterDto
{
    public List<string> ExcludedEventTypes { get; set; } = new();
}
