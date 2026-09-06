using System.Drawing;
using LegendsViewer.Backend.Extensions;

namespace LegendsViewer.Backend.Contracts;

public class WarfareMapDto
{
    public List<WarMapOverlayDto> Wars { get; set; } = [];
    public List<BattleMapMarkerDto> Battles { get; set; } = [];
}

public class WarMapOverlayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public int? AttackerId { get; set; }
    public string AttackerName { get; set; } = string.Empty;
    public string AttackerColor { get; set; } = string.Empty;
    public string? AttackerLink { get; set; }
    public int? DefenderId { get; set; }
    public string DefenderName { get; set; } = string.Empty;
    public string? DefenderLink { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public bool IsActive { get; set; }
    public int DeathCount { get; set; }
    public LocationDto? AttackerCoordinates { get; set; }
    public LocationDto? DefenderCoordinates { get; set; }
    public List<int> BattleIds { get; set; } = [];
}

public class BattleMapMarkerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public int? WarId { get; set; }
    public string WarName { get; set; } = string.Empty;
    public string? WarLink { get; set; }
    public int? AttackerId { get; set; }
    public string AttackerName { get; set; } = string.Empty;
    public string AttackerColor { get; set; } = string.Empty;
    public string? AttackerLink { get; set; }
    public int? DefenderId { get; set; }
    public string DefenderName { get; set; } = string.Empty;
    public string? DefenderLink { get; set; }
    public int? VictorId { get; set; }
    public string VictorName { get; set; } = string.Empty;
    public string? VictorLink { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public bool IsActive { get; set; }
    public int DeathCount { get; set; }
    public int AttackerDeathCount { get; set; }
    public int DefenderDeathCount { get; set; }
    public LocationDto? Coordinates { get; set; }
}
