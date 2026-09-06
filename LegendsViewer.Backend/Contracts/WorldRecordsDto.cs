namespace LegendsViewer.Backend.Contracts;

public class WorldRecordsDto
{
    public List<RecordHighlightDto> Highlights { get; set; } = [];
    public List<RecordCategoryDto> Categories { get; set; } = [];
}

public class RecordHighlightDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "mdi-trophy";
    public string? LinkHtml { get; set; }
}

public class RecordCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<RecordCardDto> Cards { get; set; } = [];
}

public class RecordCardDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "mdi-trophy-outline";
    public string MetricName { get; set; } = string.Empty;
    public List<RecordEntryDto> Entries { get; set; } = [];
}

public class RecordEntryDto
{
    public int Rank { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public string LinkHtml { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double NumericValue { get; set; }
    public string? Subtitle { get; set; }
    public string? DetailText { get; set; }
    public string Race { get; set; } = string.Empty;
    public bool IsSupernatural { get; set; }
}
