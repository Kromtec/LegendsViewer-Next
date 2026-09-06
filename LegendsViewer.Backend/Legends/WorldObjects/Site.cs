using System.Text;
using System.Drawing;
using System.Text.Json.Serialization;
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

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class Site : WorldObject, IHasCoordinates
{
    [JsonIgnore]
    public WorldRegion? Region { get; set; }
    public string? RegionToLink => Region?.ToLink(true, this);

    [JsonIgnore]
    public Entity? ReligionEntity { get; set; }

    public SiteType SiteType { get; set; }
    public string? UntranslatedName { get; set; }
    public List<Location> Coordinates { get; set; } = [];
    public Rectangle Rectangle { get; set; }
    public bool HasStructures { get; set; }

    [JsonIgnore]
    public List<Structure> Structures { get; set; } = [];
    public List<string> StructuresLinks => Structures.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Battle> Battles => EventCollections.OfType<Battle>().ToList();
    public List<string> BattleLinks => Battles.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<SiteConquered> Conquerings => EventCollections.OfType<SiteConquered>().ToList();
    public List<string> ConqueringLinks => Conquerings.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Raid> Raids => EventCollections.OfType<Raid>().ToList();
    public List<string> RaidLinks => Raids.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Duel> Duels => EventCollections.OfType<Duel>().ToList();
    public List<string> DuelLinks => Duels.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Purge> Purges => EventCollections.OfType<Purge>().ToList();
    public List<string> PurgeLinks => Purges.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Persecution> Persecutions => EventCollections.OfType<Persecution>().ToList();
    public List<string> PersecutionLinks => Persecutions.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Insurrection> Insurrections => EventCollections.OfType<Insurrection>().ToList();
    public List<string> InsurrectionLinks => Insurrections.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<EntityOverthrownCollection> Coups => EventCollections.OfType<EntityOverthrownCollection>().ToList();
    public List<string> CoupLinks => Coups.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Abduction> Abductions => EventCollections.OfType<Abduction>().ToList();
    public List<string> AbductionLinks => Abductions.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<BeastAttack> BeastAttacks => EventCollections.OfType<BeastAttack>().ToList();
    public List<string> BeastAttackLinks => BeastAttacks.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<HistoricalFigure> RelatedHistoricalFigures { get; set; } = [];
    public List<string> RelatedHistoricalFigureLinks => RelatedHistoricalFigures.ConvertAll(x => x.ToLink(true, this));

    public List<OwnerPeriod> OwnerHistory { get; set; } = [];

    public List<SiteProperty> SiteProperties { get; set; } = [];

    [JsonIgnore]
    public Entity? CurrentOwner
    {
        get
        {
            return OwnerHistory.Any(site => site.EndYear == -1) ? OwnerHistory.First(site => site.EndYear == -1).Owner : null;
        }
        set { }
    }

    [JsonIgnore]
    public Entity? CurrentCiv
    {
        get
        {
            if (CurrentOwner == null)
            {
                return null;
            }
            if (CurrentOwner.IsCiv)
            {
                return CurrentOwner;
            }
            Entity? parent = CurrentOwner.Parent;
            while (parent != null)
            {
                if (parent.IsCiv)
                {
                    return parent;
                }
                parent = parent.Parent;
            }
            return null;
        }
    }
    public string? CurrentOwnerToLink => CurrentOwner?.ToLink(true, this);

    [JsonIgnore]
    public List<Site> Connections { get; set; } = [];
    public List<string> ConnectionLinks => Connections.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<River> Rivers { get; set; } = [];
    public List<string> RiverLinks => Rivers.ConvertAll(r => r.ToLink(true, this));

    public List<Population> Populations { get; set; } = [];

    public List<Official> Officials { get; set; } = [];
    public ChartDataDto DeathsByRace
    {
        get
        {
            Dictionary<CreatureInfo, int> deaths = [];

            foreach (var notableDeath in NotableDeaths)
            {
                if (deaths.TryGetValue(notableDeath.Race, out int deathCount))
                {
                    deaths[notableDeath.Race] = ++deathCount;
                }
                else
                {
                    deaths[notableDeath.Race] = 1;
                }
            }

            foreach (var squad in Battles.SelectMany(battle => battle.AttackerSquads.Concat(battle.DefenderSquads)))
            {
                if (deaths.TryGetValue(squad.Race, out int deathCount))
                {
                    deaths[squad.Race] = deathCount + squad.Deaths;
                }
                else
                {
                    deaths[squad.Race] = squad.Deaths;
                }
            }
            ChartDataDto deathsByRace = new();
            ChartDatasetDto deathsByRaceDataset = new();
            foreach (var death in deaths)
            {
                deathsByRace.Labels.Add(death.Key.NamePlural);
                deathsByRaceDataset.Data.Add(death.Value);
                if (World != null && World.MainRaces.TryGetValue(death.Key, out var raceColor))
                {
                    deathsByRaceDataset.BorderColor.Add(raceColor.ToRgbaString());
                    deathsByRaceDataset.BackgroundColor.Add(raceColor.ToRgbaString(0.2f));
                }
                else
                {
                    deathsByRaceDataset.BorderColor.Add(Color.SlateGray.ToRgbaString());
                    deathsByRaceDataset.BackgroundColor.Add(Color.SlateGray.ToRgbaString(0.2f));
                }
            }
            deathsByRace.Datasets.Add(deathsByRaceDataset);
            return deathsByRace;
        }
    }

    [JsonIgnore]
    public List<HistoricalFigure> NotableDeaths => Events?.OfType<HfDied>().Where(death => death.HistoricalFigure != null).Select(death => death.HistoricalFigure!).ToList() ?? [];
    public List<string> NotableDeathLinks => NotableDeaths.ConvertAll(x => x.ToLink(true, this));

    public class Official
    {
        [JsonIgnore]
        public HistoricalFigure HistoricalFigure;
        public string? HistoricalFigureToLink => HistoricalFigure?.ToLink(true);

        public string Position;

        public Official(HistoricalFigure historicalFigure, string position)
        {
            HistoricalFigure = historicalFigure;
            Position = position;
        }
    }

    public Site(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "type":
                    Type = Formatting.InitCaps(property.Value);
                    switch (property.Value)
                    {
                        case "cave": SiteType = SiteType.Cave; break;
                        case "fortress": SiteType = SiteType.Fortress; break;
                        case "forest retreat": SiteType = SiteType.ForestRetreat; break;
                        case "dark fortress": SiteType = SiteType.DarkFortress; break;
                        case "town": SiteType = SiteType.Town; break;
                        case "hamlet": SiteType = SiteType.Hamlet; break;
                        case "vault": SiteType = SiteType.Vault; break;
                        case "dark pits": SiteType = SiteType.DarkPits; break;
                        case "hillocks": SiteType = SiteType.Hillocks; break;
                        case "tomb": SiteType = SiteType.Tomb; break;
                        case "tower": SiteType = SiteType.Tower; break;
                        case "mountain halls": SiteType = SiteType.MountainHalls; break;
                        case "camp": SiteType = SiteType.Camp; break;
                        case "lair": SiteType = SiteType.Lair; break;
                        case "labyrinth": SiteType = SiteType.Labyrinth; break;
                        case "shrine": SiteType = SiteType.Shrine; break;
                        case "important location": SiteType = SiteType.ImportantLocation; break;
                        case "fort": SiteType = SiteType.Fort; break;
                        case "monastery": SiteType = SiteType.Monastery; break;
                        case "castle": SiteType = SiteType.Castle; break;
                        case "mysterious palace": SiteType = SiteType.MysteriousPalace; break;
                        case "mysterious dungeon": SiteType = SiteType.MysteriousDungeon; break;
                        case "mysterious lair": SiteType = SiteType.MysteriousLair; break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "coords":
                    Location location = Formatting.ConvertToLocation(property.Value, world);
                    if (world.WorldGrid.ContainsKey(location))
                    {
                        var region = world.WorldGrid[location];
                        Region = region;
                        Subtype = region.RegionType.GetDescription();
                        region.Sites.Add(this);
                    }
                    Coordinates.Add(location);
                    break;
                case "structures":
                    HasStructures = true;
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (Property subProperty in property.SubProperties)
                        {
                            subProperty.Known = true;
                            if (subProperty.SubProperties == null)
                            {
                                continue;
                            }
                            Structures.Add(new Structure(subProperty.SubProperties, world, this));
                        }
                    }
                    break;
                case "civ_id":
                case "cur_owner_id": property.Known = true; break;
                case "rectangle":
                    char[] delimiterChars = { ':', ',' };
                    string[] rectArray = property.Value.Split(delimiterChars);
                    if (rectArray.Length == 4)
                    {
                        int x0 = Convert.ToInt32(rectArray[0]);
                        int y0 = Convert.ToInt32(rectArray[1]);
                        int x1 = Convert.ToInt32(rectArray[2]);
                        int y1 = Convert.ToInt32(rectArray[3]);
                        Rectangle = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                    }
                    else
                    {
                        property.Known = false;
                    }
                    break;
                case "site_properties":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (Property subProperty in property.SubProperties)
                        {
                            subProperty.Known = true;
                            if (subProperty.SubProperties == null)
                            {
                                continue;
                            }
                            SiteProperties.Add(new SiteProperty(subProperty.SubProperties, world, this));
                        }
                    }
                    break;
            }
        }
        SetIconByType(SiteType);
    }

    private void SetIconByType(SiteType siteType)
    {
        switch (siteType)
        {
            case SiteType.Cave:
                Icon = HtmlStyleUtil.GetIconString("ocarina");
                break;
            case SiteType.Fortress:
                Icon = HtmlStyleUtil.GetIconString("wall");
                break;
            case SiteType.ForestRetreat:
                Icon = HtmlStyleUtil.GetIconString("tree");
                break;
            case SiteType.DarkFortress:
                Icon = HtmlStyleUtil.GetIconString("temple-hindu");
                break;
            case SiteType.Town:
                Icon = HtmlStyleUtil.GetIconString("home-group");
                break;
            case SiteType.Hamlet:
                Icon = HtmlStyleUtil.GetIconString("barn");
                break;
            case SiteType.Vault:
                Icon = HtmlStyleUtil.GetIconString("treasure-chest");
                break;
            case SiteType.DarkPits:
                Icon = HtmlStyleUtil.GetIconString("bacteria");
                break;
            case SiteType.Hillocks:
                Icon = HtmlStyleUtil.GetIconString("hoop-house");
                break;
            case SiteType.Tomb:
                Icon = HtmlStyleUtil.GetIconString("grave-stone");
                break;
            case SiteType.Tower:
                Icon = HtmlStyleUtil.GetIconString("chess-rook");
                break;
            case SiteType.MountainHalls:
                Icon = HtmlStyleUtil.GetIconString("stadium-variant");
                break;
            case SiteType.Camp:
                Icon = HtmlStyleUtil.GetIconString("tent");
                break;
            case SiteType.Lair:
                Icon = HtmlStyleUtil.GetIconString("liquid-spot");
                break;
            case SiteType.Labyrinth:
                Icon = HtmlStyleUtil.GetIconString("floor-plan");
                break;
            case SiteType.Shrine:
                Icon = HtmlStyleUtil.GetIconString("pillar");
                break;
            case SiteType.Fort:
                Icon = HtmlStyleUtil.GetIconString("toy-brick");
                break;
            case SiteType.Monastery:
                Icon = HtmlStyleUtil.GetIconString("mosque");
                break;
            case SiteType.Castle:
                Icon = HtmlStyleUtil.GetIconString("castle");
                break;
            case SiteType.MysteriousPalace:
                Icon = HtmlStyleUtil.GetIconString("shield-crown");
                break;
            case SiteType.MysteriousDungeon:
                Icon = HtmlStyleUtil.GetIconString("checkbox-multiple-blank");
                break;
            case SiteType.MysteriousLair:
                Icon = HtmlStyleUtil.GetIconString("checkbox-multiple-blank-circle");
                break;
            default:
                Icon = HtmlStyleUtil.GetIconString("home-modern");
                break;
        }
    }

    public void AddConnection(Site connection)
    {
        if (!Connections.Contains(connection))
        {
            Connections.Add(connection);
        }
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = GetTitle();
            string linkedString = pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "site", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
            return linkedString;
        }
        return ToString();
    }

    private string GetTitle()
    {
        var sb = new StringBuilder();
        sb.Append(Type);
        sb.Append("&#13");
        sb.Append("&#13");
        sb.Append("Events: ");
        sb.Append(Events.Count);
        return sb.ToString();
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
            if (rule.PropertyName.Equals("IsOccupied", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(CurrentOwner != null))
            {
                return false;
            }
            if (rule.PropertyName.Equals("HasStructures", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(HasStructures || Structures.Count > 0))
            {
                return false;
            }
            if (rule.PropertyName.Equals(nameof(SiteType), StringComparison.InvariantCultureIgnoreCase))
            {
                string siteTypeStr = SiteType.ToString();
                string siteTypeDesc = SiteType.GetDescription();
                if (rule.Operator == FilterOperator.Equals &&
                    !siteTypeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !siteTypeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (rule.Operator == FilterOperator.NotEquals &&
                    (siteTypeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     siteTypeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }
            if (rule.PropertyName.Equals("StructureCount", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(rule.Value, out int ruleStructCount) &&
                rule.ViolatesIntegerCriteria(Structures.Count, ruleStructCount))
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


