namespace LegendsViewer.Backend.Contracts;

public class LeaderTimelineDto
{
    public int PositionId { get; set; } = int.MaxValue;
    public string LeaderType { get; set; } = string.Empty;
    public List<LeaderTimelineItemDto> Leaders { get; set; } = [];
}

public class LeaderTimelineItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public string Caste { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string StartYearDisplay { get; set; } = string.Empty;
    public string EndYearDisplay { get; set; } = string.Empty;
    public string ReignDuration { get; set; } = string.Empty;
    public bool IsAlive { get; set; }
    public int BirthYear { get; set; } = -1;
    public int DeathYear { get; set; } = -1;
    public string DeathCause { get; set; } = string.Empty;
    public string PredecessorRelation { get; set; } = string.Empty;
}
