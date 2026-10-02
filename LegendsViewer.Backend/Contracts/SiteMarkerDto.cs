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
    public string Owner { get; set; } = GetDisplayedCiv(site)?.ToLink() ?? "Others";
    public string OwnerText { get; set; } = GetDisplayedCiv(site)?.Name ?? "Others";
    public int? OwnerId { get; set; } = GetDisplayedCiv(site)?.Id;
    public int? CurrentOwnerId { get; set; } = site.CurrentOwner?.Id;
    public string Color { get; set; } = site.CurrentOwner?.LineColor.ToRgbaString() ?? "#666";
    public string TypeAsString { get; set; } = site.SiteType.GetDescription();
    public SiteType Type { get; set; } = site.SiteType;
    public List<Location> Coordinates { get; set; } = site.Coordinates;
    public List<Population> Populations { get; set; } = site.Populations;

    private static Entity? GetDisplayedCiv(Site site) => site.CurrentCiv is { } civ
        && !civ.Race.NamePlural.EndsWith("men", StringComparison.OrdinalIgnoreCase) ? civ : null;
}
