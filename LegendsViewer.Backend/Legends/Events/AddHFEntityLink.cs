using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using System.Text;

namespace LegendsViewer.Backend.Legends.Events;

public class AddHfEntityLink : WorldEvent, IFeatured
{
    public Entity? Entity { get; set; }
    public HistoricalFigure? HistoricalFigure { get; set; }
    public HistoricalFigure? AppointerHf { get; set; }
    public HistoricalFigure? PromiseToHf { get; set; }
    public HfEntityLinkType LinkType { get; set; }
    public string? Position { get; set; }
    public int PositionId { get; set; }

    public AddHfEntityLink(List<Property> properties, World world)
        : base(properties, world)
    {
        LinkType = HfEntityLinkType.Unknown;
        PositionId = -1;
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "civ":
                case "civ_id":
                    Entity = world.GetEntity(Convert.ToInt32(property.Value));
                    break;
                case "hfid":
                case "histfig":
                    HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "link":
                case "link_type":
                    switch (property.Value)
                    {
                        case "position":
                            LinkType = HfEntityLinkType.Position;
                            break;
                        case "prisoner":
                            LinkType = HfEntityLinkType.Prisoner;
                            break;
                        case "enemy":
                            LinkType = HfEntityLinkType.Enemy;
                            break;
                        case "member":
                            LinkType = HfEntityLinkType.Member;
                            break;
                        case "slave":
                            LinkType = HfEntityLinkType.Slave;
                            break;
                        case "squad":
                            LinkType = HfEntityLinkType.Squad;
                            break;
                        case "former member":
                        case "former_member":
                            LinkType = HfEntityLinkType.FormerMember;
                            break;
                        default:
                            world.ParsingErrors.Report("Unknown HfEntityLinkType: " + property.Value);
                            break;
                    }
                    break;
                case "position": Position = property.Value; break;
                case "position_id": PositionId = Convert.ToInt32(property.Value); break;
                case "appointer_hfid": AppointerHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "promise_to_hfid": PromiseToHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
            }
        }

        if (HistoricalFigure != null)
        {
            HistoricalFigure.AddEvent(this);
            if (PositionId != -1)
            {
                HistoricalFigure.StartPositionAssignment(Entity, Year, PositionId, Position ?? string.Empty);
            }
        }
        Entity?.AddEvent(this);
        AppointerHf?.AddEvent(this);
        if (PromiseToHf != HistoricalFigure)
        {
            PromiseToHf?.AddEvent(this);
        }
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        StringBuilder eventString = new StringBuilder(GetYearTime());

        string figure = HistoricalFigure?.ToLink(link, pov, this) ?? "UNKNOWN HISTORICAL FIGURE";
        eventString.Append(figure).Append(' ');

        switch (LinkType)
        {
            case HfEntityLinkType.Prisoner:
                eventString.Append("was imprisoned by ");
                break;
            case HfEntityLinkType.Slave:
                eventString.Append("was enslaved by ");
                break;
            case HfEntityLinkType.Enemy:
                eventString.Append("became an enemy of ");
                break;
            case HfEntityLinkType.Member:
                eventString.Append("became a member of ");
                break;
            case HfEntityLinkType.FormerMember:
                eventString.Append("became a former member of ");
                break;
            case HfEntityLinkType.Squad:
            case HfEntityLinkType.Position:
                EntityPosition? position = Entity?.EntityPositions.Find(pos =>
                    string.Equals(pos.Name, Position, StringComparison.OrdinalIgnoreCase) || pos.Id == PositionId);
                string? positionName = (position != null && HistoricalFigure?.Caste != null)
                    ? position.GetTitleByCaste(HistoricalFigure.Caste)
                    : Position;

                eventString.Append(string.IsNullOrWhiteSpace(positionName)
                    ? "got an honorable position in "
                    : $"became the {positionName} of ");
                break;
            default:
                eventString.Append("linked to ");
                break;
        }

        eventString.Append(Entity?.ToLink(link, pov, this) ?? "UNKNOWN ENTITY");
        eventString.Append(PrintParentCollection(link, pov));

        if (AppointerHf != null)
        {
            eventString.Append($", appointed by {AppointerHf.ToLink(link, pov, this)}");
        }

        if (PromiseToHf != null)
        {
            eventString.Append($" as promised to {PromiseToHf.ToLink(link, pov, this)}");
        }

        eventString.Append('.');
        return eventString.ToString();
    }

    public string PrintFeature(bool link = true, DwarfObject? pov = null)
    {
        StringBuilder eventString = new StringBuilder("the ascension of ");

        string figure = HistoricalFigure?.ToLink(link, pov, this) ?? "UNKNOWN HISTORICAL FIGURE";
        eventString.Append(figure).Append(" to the position of ");

        if (Position != null)
        {
            EntityPosition? position = Entity?.EntityPositions.Find(pos => string.Equals(pos.Name, Position, StringComparison.OrdinalIgnoreCase));
            string positionName = (position != null && HistoricalFigure?.Caste != null) ? position.GetTitleByCaste(HistoricalFigure.Caste) : Position;
            eventString.Append(positionName);
        }
        else
        {
            eventString.Append("UNKNOWN POSITION");
        }

        eventString.Append(" of ").Append(Entity?.ToLink(link, pov, this) ?? "UNKNOWN ENTITY");
        eventString.Append(" in ").Append(Year);

        return eventString.ToString();
    }
}