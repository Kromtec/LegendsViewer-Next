using System.Text;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class WorldConstruction : WorldObject, IHasCoordinates
{
    public WorldConstructionType WorldConstructionType { get; set; } // legends_plus.xml
    public List<Location> Coordinates { get; set; } // legends_plus.xml

    [JsonIgnore]
    public Site? Site1 { get; set; } // legends_plus.xml
    public string? Site1ToLink => Site1?.ToLink(true);

    [JsonIgnore]
    public Site? Site2 { get; set; } // legends_plus.xml
    public string? Site2ToLink => Site2?.ToLink(true);

    [JsonIgnore]
    public List<WorldConstruction> Sections { get; set; } // legends_plus.xml
    public List<string> SectionLinks => Sections.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public WorldConstruction? MasterConstruction { get; set; } // legends_plus.xml

    [JsonIgnore]
    public List<River> Rivers { get; set; } = [];
    public List<string> RiverLinks => Rivers.ConvertAll(r => r.ToLink(true, this));

    [JsonIgnore]
    public List<WorldRegion> Regions { get; set; } = [];
    public List<string> RegionLinks => Regions.ConvertAll(r => r.ToLink(true, this));

    [JsonIgnore]
    public List<UndergroundRegion> UndergroundRegions { get; set; } = [];
    public List<string> UndergroundRegionLinks => UndergroundRegions.ConvertAll(u => u.ToLink(true, this));

    public int SquareTiles => Coordinates.Count;
    public string? MasterConstructionToLink => MasterConstruction?.ToLink(true);
    public string? MasterConstructionType => MasterConstruction?.WorldConstructionType.ToString();

    public WorldConstruction(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        Icon = HtmlStyleUtil.GetIconString("sign-caution");
        Name = "Untitled";
        Coordinates = [];
        Sections = [];
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "type":
                    switch (property.Value)
                    {
                        case "road":
                            WorldConstructionType = WorldConstructionType.Road;
                            Icon = HtmlStyleUtil.GetIconString("road");
                            break;
                        case "bridge":
                            WorldConstructionType = WorldConstructionType.Bridge;
                            Icon = HtmlStyleUtil.GetIconString("bridge");
                            break;
                        case "tunnel":
                            WorldConstructionType = WorldConstructionType.Tunnel;
                            Icon = HtmlStyleUtil.GetIconString("tunnel-outline");
                            break;
                        default:
                            WorldConstructionType = WorldConstructionType.Unknown;
                            property.Known = false;
                            break;
                    }
                    break;
                case "coords":
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
            }
        }
        Type = WorldConstructionType.GetDescription();
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            var sb = new StringBuilder();
            sb.Append("World Construction");
            if (WorldConstructionType != WorldConstructionType.Unknown)
            {
                sb.Append(", ");
                sb.Append(WorldConstructionType);
            }
            sb.Append("&#13");
            sb.Append("Events: ");
            sb.Append(Events.Count);
            string title = sb.ToString();

            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "construction", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}

