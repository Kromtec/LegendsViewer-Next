using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldLinks;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;
using System.Text;

namespace LegendsViewer.Backend.Legends.Events;

public class AddHfhfLink : WorldEvent
{
    public HistoricalFigure? HistoricalFigure { get; set; }
    public HistoricalFigure? HistoricalFigureTarget { get; set; }
    public HistoricalFigureLinkType LinkType { get; set; }

    public AddHfhfLink(List<Property> properties, World world)
        : base(properties, world)
    {
        LinkType = HistoricalFigureLinkType.Unknown;
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "hfid_target": HistoricalFigureTarget = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "link_type":
                    HistoricalFigureLinkType linkType;
                    if (Enum.TryParse(Formatting.InitCaps(property.Value).Replace(" ", ""), out linkType))
                    {
                        LinkType = linkType;
                    }
                    else
                    {
                        world.ParsingErrors.Report("Unknown HF HF Link Type: " + property.Value);
                    }
                    break;
                case "histfig1":
                case "histfig2":
                    property.Known = true;
                    break;
                case "hf": if (HistoricalFigure == null) { HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "hf_target": if (HistoricalFigureTarget == null) { HistoricalFigureTarget = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
            }
        }

        //Fill in LinkType by looking at related historical figures.
        if (LinkType == HistoricalFigureLinkType.Unknown && HistoricalFigure != null && HistoricalFigureTarget != null)
        {
            List<HistoricalFigureLink>? historicalFigureToTargetLinks = HistoricalFigure?.RelatedHistoricalFigures
                .Where(link => link.Type != HistoricalFigureLinkType.Child && link.HistoricalFigure == HistoricalFigureTarget)
                .ToList();
            HistoricalFigureLink? historicalFigureToTargetLink = null;
            if (historicalFigureToTargetLinks?.Count <= 1)
            {
                historicalFigureToTargetLink = historicalFigureToTargetLinks.FirstOrDefault();
            }

            HfAbducted? abduction = HistoricalFigureTarget?.Events.OfType<HfAbducted>().FirstOrDefault(a => a.Snatcher == HistoricalFigure);
            if (historicalFigureToTargetLink != null && abduction == null)
            {
                LinkType = historicalFigureToTargetLink.Type;
            }
            else if (abduction != null)
            {
                LinkType = HistoricalFigureLinkType.Prisoner;
            }
        }

        HistoricalFigure?.AddEvent(this);
        HistoricalFigureTarget?.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        StringBuilder eventString = new StringBuilder(GetYearTime());

        string subject = (pov == HistoricalFigureTarget)
            ? HistoricalFigureTarget?.ToLink(link, pov, this) ?? "an unknown creature"
            : HistoricalFigure?.ToLink(link, pov, this) ?? "an unknown creature";

        string target = (pov == HistoricalFigureTarget)
            ? HistoricalFigure?.ToLink(link, pov, this) ?? "an unknown creature"
            : HistoricalFigureTarget?.ToLink(link, pov, this) ?? "an unknown creature";

        eventString.Append(subject).Append(' ');

        eventString.Append(LinkType switch
        {
            HistoricalFigureLinkType.Apprentice => (pov == HistoricalFigureTarget) ? "began an apprenticeship under " : "became the master of ",
            HistoricalFigureLinkType.Master => (pov == HistoricalFigureTarget) ? "became the master of " : "began an apprenticeship under ",
            HistoricalFigureLinkType.FormerApprentice => (pov == HistoricalFigure) ? "ceased being the apprentice of " : "ceased being the master of ",
            HistoricalFigureLinkType.FormerMaster => (pov == HistoricalFigure) ? "ceased being the master of " : "ceased being the apprentice of ",
            HistoricalFigureLinkType.Deity => (pov == HistoricalFigureTarget) ? "received the worship of " : "began worshipping ",
            HistoricalFigureLinkType.Lover => "became romantically involved with ",
            HistoricalFigureLinkType.Spouse => "married ",
            HistoricalFigureLinkType.FormerSpouse => "divorced ",
            HistoricalFigureLinkType.Prisoner => (pov == HistoricalFigureTarget) ? "was imprisoned by " : "imprisoned ",
            HistoricalFigureLinkType.PetOwner => (pov == HistoricalFigureTarget) ? "became the owner of " : "became the pet of ",
            _ => $"linked ({LinkType}) to "
        });

        eventString.Append(target);
        eventString.Append(PrintParentCollection(link, pov)).Append('.');

        return eventString.ToString();
    }
}