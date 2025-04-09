using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using System.Text;

namespace LegendsViewer.Backend.Legends.Events.PlusEvents;

public class HistoricalEventRelationShip : WorldEvent
{
    private int _occasionType; // TODO unknown field
    private int _unk1; // TODO unknown field
    private int _reason; // TODO unknown field

    public HistoricalFigure? SourceHf { get; set; }
    public HistoricalFigure? TargetHf { get; set; }
    public VagueRelationshipType RelationshipType { get; set; }
    private Site? Site { get; set; }

    public HistoricalEventRelationShip(List<Property> properties, World world) : base(properties, world)
    {
        Type = "historical event relationship";
        Seconds72 = -1;
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "event":
                    Id = Convert.ToInt32(property.Value);
                    break;
                case "source_hf":
                    SourceHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "target_hf":
                    TargetHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "year":
                    Year = Convert.ToInt32(property.Value);
                    break;
                case "relationship":
                    RelationshipType = VagueRelationship.GetVagueRelationshipTypeByProperty(property, property.Value);
                    break;
            }
        }

        SourceHf?.AddEvent(this);
        TargetHf?.AddEvent(this);
    }

    public static void ResolveSupplements(List<Property> properties, World world)
    {
        HistoricalEventRelationShip? historicalEventRelationShip = null;
        int occasionType = -1;
        Site? site = null;
        int unk1 = -1;
        int reason = -1;
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "event":
                    int id = Convert.ToInt32(property.Value);
                    if (world.SpecialEventsById.ContainsKey(id))
                    {
                        historicalEventRelationShip = world.SpecialEventsById[id] as HistoricalEventRelationShip;
                    }
                    break;
                case "occasion_type":
                    occasionType = Convert.ToInt32(property.Value);
                    break;
                case "site":
                    site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "unk_1":
                    unk1 = Convert.ToInt32(property.Value);
                    break;
                case "reason":
                    reason = Convert.ToInt32(property.Value);
                    break;
            }
        }

        historicalEventRelationShip?.AddSupplements(occasionType, site, unk1, reason);
    }

    private void AddSupplements(int occasionType, Site? site, int unk1, int reason)
    {
        Site = site;
        _unk1 = unk1;
        _occasionType = occasionType;
        _reason = reason;
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        StringBuilder eventString = new StringBuilder(GetYearTime());

        string source = SourceHf != null ? SourceHf.ToLink(link, pov, this) : "UNKNOWN HISTORICAL FIGURE";
        string target = TargetHf != null ? TargetHf.ToLink(link, pov, this) : "UNKNOWN HISTORICAL FIGURE";
        string relationship = RelationshipType.GetDescription().ToLower();

        switch (RelationshipType)
        {
            case VagueRelationshipType.JealousObsession:
                eventString.Append($"{source} became infatuated with {target}");
                break;
            case VagueRelationshipType.Lieutenant:
                eventString.Append($"{source} recognized {target} as a capable {relationship}");
                break;
            case VagueRelationshipType.FormerLover:
                eventString.Append($"{source} and {target} broke up");
                break;
            default:
                eventString.Append($"{source} and {target} became {relationship}s");
                break;
        }

        if (Site != null)
        {
            eventString.Append($" in {Site.ToLink(link, pov, this)}");
        }

        eventString.Append(".");
        return eventString.ToString();
    }
}