using System.Text;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class WorldRegion : WorldObject, IRegion
{
    public int? Depth { get; set; }
    public RegionType RegionType { get; set; }
    public List<string> Deaths
    {
        get
        {
            List<string> deaths = [.. NotableDeaths.Select(death => death.Race.Id)];
            foreach (Battle.Squad squad in Battles.SelectMany(battle => battle.AttackerSquads.Concat(battle.DefenderSquads)))
            {
                for (int i = 0; i < squad.Deaths; i++)
                {
                    deaths.Add(squad.Race.Id);
                }
            }

            return deaths;
        }
        set { }
    }
    [JsonIgnore]
    public List<HistoricalFigure> NotableDeaths => Events?.OfType<HfDied>().Where(death => death.HistoricalFigure != null).Select(death => death.HistoricalFigure!).ToList() ?? [];

    public List<string> NotableDeathLinks => NotableDeaths.ConvertAll(d => d.ToLink(true, this));

    [JsonIgnore]
    public List<Battle> Battles { get; set; }
    public List<string> BattleLinks => Battles.ConvertAll(b => b.ToLink(true, this));
    public List<Location> Coordinates { get; set; } // legends_plus.xml
    public int SquareTiles => Coordinates.Count;

    [JsonIgnore]
    public List<Site> Sites { get; set; } // legends_plus.xml
    public List<string> SiteLinks => Sites.ConvertAll(s => $"{s.ToLink(true, this)} ({s.SiteType.GetDescription()})");

    [JsonIgnore]
    public List<River> Rivers { get; set; } = [];
    public List<string> RiverLinks => Rivers.ConvertAll(r => r.ToLink(true, this));

    [JsonIgnore]
    public List<MountainPeak> MountainPeaks { get; set; } // legends_plus.xml
    public List<string> MountainPeakLinks => MountainPeaks.ConvertAll(m => m.ToLink(true, this));

    [JsonIgnore]
    public List<WorldConstruction> Constructions { get; set; } = [];
    public List<string> ConstructionLinks => Constructions.ConvertAll(c => $"{c.ToLink(true, this)} ({c.WorldConstructionType.GetDescription()})");

    public Evilness Evilness { get; set; } // legends_plus.xml

    [JsonIgnore]
    public int ForceId { get; set; } // legends_plus.xml

    [JsonIgnore]
    public HistoricalFigure? Force { get; set; } // legends_plus.xml

    public string? ForceLink => Force?.ToLink(true, this);

    private static readonly char[] coordinateSeparator = ['|'];

    public WorldRegion(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        Icon = HtmlStyleUtil.GetIconString("map-legend");
        Name = "UNKNOWN REGION";
        ForceId = -1;
        Battles = [];
        Coordinates = [];
        Sites = [];
        MountainPeaks = [];
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "type":
                    switch (property.Value)
                    {
                        case "Mountains":
                            RegionType = RegionType.Mountains;
                            Icon = HtmlStyleUtil.GetIconString("image-filter-hdr");
                            break;
                        case "Ocean":
                            RegionType = RegionType.Ocean;
                            Icon = HtmlStyleUtil.GetIconString("tsunami");
                            break;
                        case "Tundra":
                            RegionType = RegionType.Tundra;
                            Icon = HtmlStyleUtil.GetIconString("weather-snowy-heavy");
                            break;
                        case "Glacier":
                            RegionType = RegionType.Glacier;
                            Icon = HtmlStyleUtil.GetIconString("snowflake");
                            break;
                        case "Forest":
                            RegionType = RegionType.Forest;
                            Icon = HtmlStyleUtil.GetIconString("forest-outline");
                            break;
                        case "Wetland":
                            RegionType = RegionType.Wetland;
                            Icon = HtmlStyleUtil.GetIconString("home-flood");
                            break;
                        case "Grassland":
                            RegionType = RegionType.Grassland;
                            Icon = HtmlStyleUtil.GetIconString("grass");
                            break;
                        case "Desert":
                            RegionType = RegionType.Desert;
                            Icon = HtmlStyleUtil.GetIconString("cactus");
                            break;
                        case "Hills":
                            RegionType = RegionType.Hills;
                            Icon = HtmlStyleUtil.GetIconString("image-filter-hdr-outline");
                            break;
                        case "Lake":
                            RegionType = RegionType.Lake;
                            Icon = HtmlStyleUtil.GetIconString("weather-hazy");
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "coords":
                    string[] coordinateStrings = property.Value.Split(coordinateSeparator,
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (var coordinateString in coordinateStrings)
                    {
                        Location location = Formatting.ConvertToLocation(coordinateString, world);
                        world.WorldGrid[location] = this;
                        Coordinates.Add(location);
                    }
                    break;
                case "evilness":
                    switch (property.Value)
                    {
                        case "good":
                            Evilness = Evilness.Good;
                            break;
                        case "neutral":
                            Evilness = Evilness.Neutral;
                            break;
                        case "evil":
                            Evilness = Evilness.Evil;
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "force_id":
                    ForceId = Convert.ToInt32(property.Value);
                    break;
            }
        }
        Type = RegionType.GetDescription();
        Subtype = Evilness.GetDescription();
    }
    public override string ToString() { return Name; }
    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            var sb = new StringBuilder();
            sb.Append(RegionType.GetDescription());
            sb.Append("&#13");
            sb.Append("Evilness: ");
            sb.Append(Evilness);
            sb.Append("&#13");
            sb.Append("Events: ");
            sb.Append(Events.Count);
            string title = sb.ToString();

            return pov != this
                ? $"{HtmlStyleUtil.GetAnchorString(Icon, "region", Id, title, Name)}"
                : $"{HtmlStyleUtil.GetAnchorString(Icon, "region", Id, title, HtmlStyleUtil.CurrentDwarfObject(Name))}";
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }

    public void Resolve(IWorld world)
    {
        if (ForceId != -1)
        {
            Force = world.GetHistoricalFigure(ForceId);
            if (Force?.RelatedRegions.Contains(this) == false)
            {
                Force.RelatedRegions.Add(this);
            }
        }
    }

    public override bool MatchesFilterCriteria(WorldObjectFilterDto filter)
    {
        if (!base.MatchesFilterCriteria(filter))
        {
            return false;
        }

        foreach (var rule in filter.Filters)
        {
            if (rule.PropertyName.Equals("HasSites", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Sites.Count > 0))
            {
                return false;
            }
            if (rule.PropertyName.Equals("HasForce", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Force != null || ForceId != -1))
            {
                return false;
            }
            if (rule.PropertyName.Equals(nameof(RegionType), StringComparison.InvariantCultureIgnoreCase) ||
                rule.PropertyName.Equals("Biome", StringComparison.InvariantCultureIgnoreCase))
            {
                string regionTypeStr = RegionType.ToString();
                string regionTypeDesc = RegionType.GetDescription();
                if (rule.Operator == FilterOperator.Equals &&
                    !regionTypeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !regionTypeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (rule.Operator == FilterOperator.NotEquals &&
                    (regionTypeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     regionTypeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }
            if (rule.PropertyName.Equals(nameof(Evilness), StringComparison.InvariantCultureIgnoreCase))
            {
                string evilnessStr = Evilness.ToString();
                string evilnessDesc = Evilness.GetDescription();
                if (rule.Operator == FilterOperator.Equals &&
                    !evilnessStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !evilnessDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !Subtype.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (rule.Operator == FilterOperator.NotEquals &&
                    (evilnessStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     evilnessDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     Subtype.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }
            if (rule.PropertyName.Equals(nameof(SquareTiles), StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleTiles) &&
                rule.ViolatesIntegerCriteria(SquareTiles, ruleTiles))
            {
                return false;
            }
            if (rule.PropertyName.Equals("SiteCount", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleSiteCount) &&
                rule.ViolatesIntegerCriteria(Sites.Count, ruleSiteCount))
            {
                return false;
            }
            if (rule.PropertyName.Equals("BattleCount", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleBattleCount) &&
                rule.ViolatesIntegerCriteria(Battles.Count, ruleBattleCount))
            {
                return false;
            }
        }

        return true;
    }
}


