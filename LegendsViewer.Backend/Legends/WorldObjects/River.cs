using System.Text;
using System.Text.Json.Serialization;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class River : WorldObject, IHasCoordinates
{
    public Location? EndPos { get; set; } // legends_plus.xml
    public string? Path { get; set; } // legends_plus.xml
    public List<Location> Coordinates { get; set; } // legends_plus.xml

    public int Length => Coordinates.Count;

    [JsonIgnore]
    public List<WorldRegion> Regions { get; set; } = [];
    public List<string> RegionLinks => Regions.ConvertAll(r => r.ToLink(true, this));

    [JsonIgnore]
    public List<Site> Sites { get; set; } = [];
    public List<string> SiteLinks => Sites.ConvertAll(s => $"{s.ToLink(true, this)} ({s.SiteType.GetDescription()})");

    [JsonIgnore]
    public List<WorldConstruction> Constructions { get; set; } = [];
    public List<string> ConstructionLinks => Constructions.ConvertAll(c => $"{c.ToLink(true, this)} ({c.WorldConstructionType.GetDescription()})");

    public River(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        Icon = HtmlStyleUtil.GetIconString("waves");
        Name = "Untitled";
        Coordinates = [];

        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "path":
                    Path = property.Value;
                    string[] coordinateStrings = property.Value.Split(new[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (var coordinateString in coordinateStrings)
                    {
                        string[] xYCoordinates = coordinateString.Split(',');
                        int x = Convert.ToInt32(xYCoordinates[0]);
                        int y = Convert.ToInt32(xYCoordinates[1]);
                        Coordinates.Add(new Location(x, y));
                    }
                    break;
                case "end_pos":
                    string[] endCoordinates = property.Value.Split(',');
                    int endX = Convert.ToInt32(endCoordinates[0]);
                    int endY = Convert.ToInt32(endCoordinates[1]);
                    EndPos = new Location(endX, endY);
                    Coordinates.Add(EndPos);
                    break;
            }
        }
        if (Id == -1)
        {
            Id = world.Rivers.Count;
        }
        Type = "River";
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            var sb = new StringBuilder();
            sb.Append("River");
            sb.Append("&#13");
            sb.Append("Events: ");
            sb.Append(Events.Count);
            string title = sb.ToString();
            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "river", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }

    public override bool MatchesFilterCriteria(WorldObjectFilterDto filter)
    {
        if (!base.MatchesFilterCriteria(filter))
        {
            return false;
        }

        foreach (var rule in filter.Filters)
        {
            if (rule.PropertyName.Equals("Length", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleLength) &&
                rule.ViolatesIntegerCriteria(Length, ruleLength))
            {
                return false;
            }
            if (rule.PropertyName.Equals("RegionCount", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleRegionCount) &&
                rule.ViolatesIntegerCriteria(Regions.Count, ruleRegionCount))
            {
                return false;
            }
            if (rule.PropertyName.Equals("SiteCount", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleSiteCount) &&
                rule.ViolatesIntegerCriteria(Sites.Count, ruleSiteCount))
            {
                return false;
            }
            if (rule.PropertyName.Equals("ConstructionCount", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleConstrCount) &&
                rule.ViolatesIntegerCriteria(Constructions.Count, ruleConstrCount))
            {
                return false;
            }
        }

        return true;
    }
}


