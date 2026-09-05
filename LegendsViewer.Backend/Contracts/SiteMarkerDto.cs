using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Contracts;

public class SiteMarkerDto(Site site)
{
    public int Id { get; set; } = site.Id;
    public string Name { get; set; } = site.ToLink();
    public string Owner { get; set; } = site.CurrentCiv?.ToLink() ?? "Others";
    public string OwnerText { get; set; } = site.CurrentCiv?.Name ?? "Others";
    public int? OwnerId { get; set; } = site.CurrentCiv?.Id;
    public int? CurrentOwnerId { get; set; } = site.CurrentOwner?.Id;
    public string Color { get; set; } = site.CurrentOwner?.LineColor.ToRgbaString() ?? "#666";
    public string TypeAsString { get; set; } = site.SiteType.GetDescription();
    public SiteType Type { get; set; } = site.SiteType;
    public List<Location> Coordinates { get; set; } = site.Coordinates;
}
