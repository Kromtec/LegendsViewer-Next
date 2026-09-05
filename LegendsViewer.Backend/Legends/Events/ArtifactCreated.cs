using System.Text;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class ArtifactCreated : WorldEvent
{
    public Artifact? Artifact { get; set; }
    public bool ReceivedName { get; set; }
    public HistoricalFigure? HistoricalFigure { get; set; }
    public Site? Site { get; set; }
    public HistoricalFigure? SanctifyFigure { get; set; }
    public ArtifactReason Reason { get; set; }
    public Circumstance Circumstance { get; set; }
    public HistoricalFigure? DefeatedFigure { get; set; }
    public Entity? Entity { get; set; }

    public ArtifactCreated(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); break;
                case "hist_figure_id":
                case "creator_hfid":
                    HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "entity":
                case "entity_id":
                    if (property.Value != "-1")
                    {
                        Entity = world.GetEntity(Convert.ToInt32(property.Value));
                    }
                    break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "name_only": ReceivedName = true; property.Known = true; break;
                case "hfid": if (HistoricalFigure == null) { HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "site": if (Site == null) { Site = world.GetSite(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "unit_id":
                case "creator_unit_id":
                case "anon_3":
                    if (property.Value != "-1")
                    {
                        property.Known = false;
                    }
                    break;
                case "anon_4":
                case "sanctify_hf":
                    SanctifyFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "reason":
                    switch (property.Value)
                    {
                        case "sanctify_hf":
                            Reason = ArtifactReason.SanctifyHistoricalFigure;
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    if (!property.Known && property.Value.Contains("unknown"))
                    {
                        property.Known = true;
                    }
                    break;
                case "circumstance":
                    property.Known = true;
                    if (property.SubProperties == null)
                    {
                        continue;
                    }
                    foreach (var subProperty in property.SubProperties)
                    {
                        switch (subProperty.Name)
                        {
                            case "type":
                                switch (subProperty.Value)
                                {
                                    case "defeated":
                                        Circumstance = Circumstance.DefeatedHf;
                                        break;
                                    case "conflict":
                                        Circumstance = Circumstance.Conflict;
                                        break;
                                    case "trauma":
                                        Circumstance = Circumstance.Trauma;
                                        break;
                                    case "favoritepossession":
                                        Circumstance = Circumstance.FavoritePossession;
                                        break;
                                    case "preservebody":
                                        Circumstance = Circumstance.PreserveBody;
                                        break;
                                    default:
                                        property.Known = false;
                                        break;
                                }
                                break;
                            case "defeated":
                                DefeatedFigure = world.GetHistoricalFigure(Convert.ToInt32(subProperty.Value));
                                break;
                        }
                    }
                    break;
            }
        }

        if (Artifact != null && HistoricalFigure != null)
        {
            Artifact.Creator = HistoricalFigure;
        }
        Artifact?.AddEvent(this);
        HistoricalFigure?.AddEvent(this);
        Site?.AddEvent(this);
        SanctifyFigure?.AddEvent(this);
        DefeatedFigure?.AddEvent(this);
        Entity?.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        var sb = new StringBuilder();
        sb.Append(GetYearTime());
        sb.Append(Artifact?.ToLink(link, pov, this));
        if (ReceivedName)
        {
            sb.Append(" received its name");
        }
        else
        {
            sb.Append(" was created");
        }

        if (Site != null)
        {
            sb.Append(" in ");
            sb.Append(Site.ToLink(link, pov, this));
        }

        if (ReceivedName)
        {
            sb.Append(" from ");
        }
        else
        {
            sb.Append(" by ");
        }

        sb.Append(HistoricalFigure != null ? HistoricalFigure.ToLink(link, pov, this) : "UNKNOWN HISTORICAL FIGURE");
        if (SanctifyFigure != null)
        {
            sb.Append(" in order to sanctify ");
            sb.Append(SanctifyFigure.ToLink(link, pov, this));
            if (Circumstance == Circumstance.PreserveBody)
            {
                sb.Append(" by preserving a part of the body");
            }
            else if (Circumstance == Circumstance.FavoritePossession)
            {
                sb.Append("'s favorite possession");
            }
        }

        if (DefeatedFigure != null)
        {
            sb.Append(" after defeating ");
            sb.Append(DefeatedFigure.ToLink(link, pov, this));
        }
        sb.Append(PrintParentCollection(link, pov));
        sb.Append('.');
        return sb.ToString();
    }
}

