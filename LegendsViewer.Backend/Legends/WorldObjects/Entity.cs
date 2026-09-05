using System.Text;
using System.Drawing;
using System.Text.Json.Serialization;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Cytoscape;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Maps;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldLinks;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class Entity : WorldObject, IHasCoordinates
{
    [JsonIgnore]
    public Entity? Parent { get; set; }
    public string? ParentLink => Parent?.ToLink(true, this);

    public CreatureInfo Race { get; set; } = CreatureInfo.Unknown;

    [JsonIgnore]
    public List<HistoricalFigure> Worshipped { get; set; } = [];
    public List<ListItemDto> WorshippedLinks
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (var deity in Worshipped)
            {
                string associatedSpheres = string.Join(", ", deity.Spheres);
                list.Add(new ListItemDto
                {
                    Title = deity.ToLink(true, this),
                    Subtitle = associatedSpheres
                });
            }
            if (Worshipped.Count == 0 && EntityEntityLinks.Count > 0)
            {
                foreach (EntityEntityLink link in EntityEntityLinks)
                {
                    var deity = link.Target?.Worshipped?.FirstOrDefault();
                    if (deity == null)
                    {
                        continue;
                    }
                    string associatedSpheres = string.Join(", ", deity.Spheres);
                    var newItem = new ListItemDto
                    {
                        Title = $"{deity.ToLink(true, this)}",
                        Subtitle = associatedSpheres
                    };
                    if (list.Exists(existingItem => existingItem.Title == newItem.Title))
                    {
                        continue;
                    }
                    list.Add(newItem);
                }
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<string> LeaderTypes { get; set; } = [];

    [JsonIgnore]
    public List<List<HistoricalFigure>> Leaders { get; set; } = [];
    // TODO Leaders API

    public EntityPopulation? EntityPopulation { get; set; }

    public List<Population> Populations { get; set; } = [];

    [JsonIgnore]
    public Structure? OriginStructure { get; set; }
    public string? OriginStructureLink => OriginStructure?.ToLink(true, this);

    [JsonIgnore]
    public List<Entity> Groups { get; set; } = [];
    public List<string> GroupLinks => Groups.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<OwnerPeriod> SiteHistory { get; set; } = [];

    [JsonIgnore]
    public List<Site> CurrentSites => SiteHistory.Where(site => site.EndYear == -1).Select(site => site.Site).ToList();
    public List<ListItemDto> CurrentSiteList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (OwnerPeriod link in SiteHistory.Where(site => site.EndYear == -1))
            {
                if (link.Site == null)
                {
                    continue;
                }
                var subtitle = $"{Formatting.InitCaps(link.StartCause ?? "founded")} in " + (link.StartYear >= 0 ? link.StartYear.ToString() : "a time before time");
                if (link.Founder != null)
                {
                    subtitle += $" by {link.Founder.ToLink(true, this)}";
                }
                else if (link.Owner != null)
                {
                    subtitle += $" by {link.Owner.ToLink(true, this)}";
                }
                list.Add(new ListItemDto
                {
                    Title = link.Site.ToLink(true, this),
                    Subtitle = subtitle
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<Site> LostSites => SiteHistory.Where(site => site.EndYear >= 0).Select(site => site.Site).ToList();
    public List<ListItemDto> LostSiteList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (OwnerPeriod link in SiteHistory.Where(site => site.EndYear >= 0))
            {
                if (link.Site == null)
                {
                    continue;
                }
                var subtitle = $"{Formatting.InitCaps(link.StartCause ?? "founded")} in " + (link.StartYear >= 0 ? link.StartYear.ToString() : "a time before time");
                if (link.Founder != null)
                {
                    subtitle += $" by {link.Founder.ToLink(true, this)}";
                }
                else if (link.Owner != null)
                {
                    subtitle += $" by {link.Owner.ToLink(true, this)}";
                }
                if (!string.IsNullOrWhiteSpace(link.EndCause))
                {
                    subtitle += $" and {link.EndCause} in {link.EndYear}";
                    if (link.Destroyer != null)
                    {
                        subtitle += $" by {link.Destroyer.ToLink(true, this)}";
                    }
                    else if (link.Ender != null)
                    {
                        subtitle += $" by {link.Ender.ToLink(true, this)}";
                    }
                }
                list.Add(new ListItemDto
                {
                    Title = link.Site.ToLink(true, this),
                    Subtitle = subtitle
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<Site> Sites => SiteHistory.ConvertAll(site => site.Site);
    public List<string> SiteLinks => Sites.ConvertAll(x => x.ToLink(true, this));

    public List<Honor> Honors { get; set; } = [];

    public EntityType EntityType { get; set; } = EntityType.Unknown; // legends_plus.xml
    public bool IsCiv { get; set; }

    [JsonIgnore]
    public List<EntitySiteLink> EntitySiteLinks { get; set; } = []; // legends_plus.xml
    public List<ListItemDto> EntitySiteLinkList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (EntitySiteLink link in EntitySiteLinks)
            {
                if (link.Site == null)
                {
                    continue;
                }
                list.Add(new ListItemDto
                {
                    Title = $"{link.Type.GetDescription()} ({link.Site?.Type.GetDescription()})",
                    Subtitle = $"{link.Site?.ToLink(true, this)}",
                    Append = HtmlStyleUtil.GetChipString(link.Strength.ToString())
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<EntityEntityLink> EntityEntityLinks { get; set; } = []; // legends_plus.xml
    public List<ListItemDto> EntityEntityLinkList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (EntityEntityLink link in EntityEntityLinks)
            {
                if (link.Target == null)
                {
                    continue;
                }
                list.Add(new ListItemDto
                {
                    Title = $"{link.Type.GetDescription()} ({link.Target?.Type.GetDescription()})",
                    Subtitle = $"{link.Target?.ToLink(true, this)}",
                    Append = HtmlStyleUtil.GetChipString(link.Strength.ToString())
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<EntityPosition> EntityPositions { get; set; } = []; // legends_plus.xml

    public ListItemDto? CurrentLeader
    {
        get
        {
            if (EntityPositionAssignments.Count == 0)
            {
                return null;
            }
            EntityPositionAssignment? assignment = EntityPositionAssignments.OrderBy(a => a.PositionId).FirstOrDefault(a => a.PositionId >= 0);
            if (assignment == null)
            {
                return null;
            }
            EntityPosition? position = EntityPositions.Find(pos => pos.Id == assignment.PositionId);
            if (position == null || assignment.HistoricalFigure == null)
            {
                return null;
            }
            string positionName = position.GetTitleByCaste(assignment.HistoricalFigure.Caste);
            string positionHolder = assignment.HistoricalFigure.ToLink(true, this);
            return new ListItemDto
            {
                Title = positionName,
                Subtitle = positionHolder
            };
        }
    }


    [JsonIgnore]
    public Entity? CurrentCiv
    {
        get
        {
            if (IsCiv)
            {
                return this;
            }
            Entity? parent = Parent;
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

    [JsonIgnore]
    public List<EntityPositionAssignment> EntityPositionAssignments { get; set; } = []; // legends_plus.xml
    public List<ListItemDto> EntityPositionAssignmentsList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (EntityPositionAssignment assignment in EntityPositionAssignments.OrderBy(a => a.PositionId))
            {
                EntityPosition? position = EntityPositions.Find(pos => pos.Id == assignment.PositionId);
                if (position == null || assignment.HistoricalFigure == null)
                {
                    continue;
                }
                string positionName = position.GetTitleByCaste(assignment.HistoricalFigure.Caste);
                string positionHolder = assignment.HistoricalFigure.ToLink(true, this);

                list.Add(new ListItemDto
                {
                    Title = positionName,
                    Subtitle = positionHolder
                });

                if (string.IsNullOrEmpty(position.Spouse))
                {
                    continue;
                }
                HistoricalFigureLink? spouseHfLink = assignment.HistoricalFigure.RelatedHistoricalFigures.Find(hfLink => hfLink.Type == HistoricalFigureLinkType.Spouse);
                if (spouseHfLink == null)
                {
                    continue;
                }
                HistoricalFigure? spouse = spouseHfLink.HistoricalFigure;
                if (spouse == null)
                {
                    continue;
                }
                string spousePositionName = position.GetTitleByCaste(spouse.Caste, true);
                string spousePositionHolder = spouse.ToLink(true, this);
                list.Add(new ListItemDto
                {
                    Title = spousePositionName,
                    Subtitle = spousePositionHolder
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public Location? FormedAt { get; set; }
    [JsonIgnore]
    public List<Location> Coordinates
    {
        get
        {
            List<Location> locations = [];
            locations.AddRange(CurrentSites.SelectMany(s => s.Coordinates));
            if (FormedAt != null)
            {
                locations.Add(FormedAt);
            }
            return locations; // legends_plus.xml
        }
    }

    [JsonIgnore]
    public List<EntityOccasion> Occassions { get; set; } = []; // legends_plus.xml

    public List<string> Weapons { get; set; } = [];
    public string? Profession { get; set; }

    [JsonIgnore]
    public List<War> Wars { get; set; } = [];
    public List<ListItemDto> WarList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (var war in Wars)
            {
                list.Add(new ListItemDto
                {
                    Title = war.ToLink(true, this),
                    Subtitle = war.Subtype,
                    Append = HtmlStyleUtil.GetChipString($"{war.DeathCount} ✝")
                });
            }
            return list;
        }
    }

    private CytoscapeData? _warGraphData;
    public CytoscapeData? WarGraphData
    {
        get
        {
            if (_warGraphData == null && Wars.Count > 0)
            {
                Dictionary<int, CytoscapeNodeElement> nodes = [];
                List<CytoscapeEdgeElement> edges = [];

                foreach (var item in Wars)
                {
                    if (item.Attacker != null && !nodes.ContainsKey(item.Attacker.Id))
                    {
                        nodes.Add(item.Attacker.Id, item.Attacker.GetCytoscapeNode(this));
                    }
                    if (item.Defender != null && !nodes.ContainsKey(item.Defender.Id))
                    {
                        nodes.Add(item.Defender.Id, item.Defender.GetCytoscapeNode(this));
                    }
                    if (item.Attacker != null && item.Defender != null)
                    {
                        int edgeWidth = item.DeathCount / 10;
                        edges.Add(new CytoscapeEdgeElement(new CytoscapeEdgeData
                        {
                            Source = $"node-{item.Attacker.Id}",
                            Target = $"node-{item.Defender.Id}",
                            Href = $"/war/{item.Id}",
                            BackgroundColor = item.Attacker.LineColor.ToRgbaString(0.6f),
                            ForegroundColor = Formatting.GetReadableForegroundColor(item.Attacker.LineColor),
                            Width = edgeWidth > 15 ? 15 : edgeWidth == 0 ? 1 : edgeWidth,
                            Label = $"{item.DeathCount} ✝",
                            Tooltip = $"{item.ToLink(true, item)}<br/>{item.Subtype}"
                        }));
                    }
                }

                var warGraphData = new CytoscapeData();
                warGraphData.Nodes.AddRange(nodes.Values);
                warGraphData.Edges.AddRange(edges);
                _warGraphData = warGraphData;
            }
            return _warGraphData;
        }

        set => _warGraphData = value;
    }

    [JsonIgnore]
    public List<War> WarsAttacking => Wars.Where(war => war.Attacker == this).ToList();
    public List<string> WarAttackingLinks => WarsAttacking.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<War> WarsDefending => Wars.Where(war => war.Defender == this).ToList();
    public List<string> WarDefendingLinks => WarsDefending.ConvertAll(x => x.ToLink(true, this));

    public int WarVictories => WarsAttacking.Sum(war => war.AttackerBattleVictories.Count) + WarsDefending.Sum(war => war.DefenderBattleVictories.Count);
    public int WarLosses => WarsAttacking.Sum(war => war.DefenderBattleVictories.Count) + WarsDefending.Sum(war => war.AttackerBattleVictories.Count);
    public int WarKills => WarsAttacking.Sum(war => war.DefenderDeathCount) + WarsDefending.Sum(war => war.AttackerDeathCount);
    public int WarDeaths => WarsAttacking.Sum(war => war.AttackerDeathCount) + WarsDefending.Sum(war => war.DefenderDeathCount);

    [JsonIgnore]
    public List<HistoricalFigure> AllLeaders => Leaders.SelectMany(l => l).ToList();
    public List<string> AllLeaderLinks => AllLeaders.ConvertAll(x => x.ToLink(true, this));

    public List<string> PopulationsAsList
    {
        get
        {
            var populations = new List<string>();
            foreach (var population in Populations)
            {
                for (var i = 0; i < population.Count; i++)
                {
                    populations.Add(population.Race.NamePlural);
                }
            }

            return populations;
        }
    }

    public double WarKillDeathRatio
    {
        get
        {
            if (WarDeaths == 0 && WarKills == 0)
            {
                return 0;
            }

            return WarDeaths == 0 ? double.MaxValue : Math.Round(WarKills / Convert.ToDouble(WarDeaths), 2);
        }
    }

    public Color LineColor { get; set; }

    private string? _icon;

    public new string Icon
    {
        get
        {
            if (string.IsNullOrEmpty(_icon))
            {
                string coloredIcon;
                if (IsCiv)
                {
                    coloredIcon = HtmlStyleUtil.GetCivIconString(Formatting.GetInitials(Name), ColorTranslator.ToHtml(LineColor), Formatting.GetReadableForegroundColor(LineColor));
                }
                else if (World != null && World.MainRaces.ContainsKey(Race))
                {
                    Color civilizedPopColor = LineColor;
                    if (civilizedPopColor == Color.Empty)
                    {
                        civilizedPopColor = World.MainRaces.FirstOrDefault(r => r.Key == Race).Value;
                        LineColor = civilizedPopColor;
                    }
                    coloredIcon = HtmlStyleUtil.GetIconString("account-group", ColorTranslator.ToHtml(civilizedPopColor));
                }
                else
                {
                    coloredIcon = HtmlStyleUtil.GetIconString("account-multiple");
                }
                _icon = coloredIcon;
            }
            return _icon;
        }
        set => _icon = value;
    }

    public Entity(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "race":
                    Race = world.GetCreatureInfo(property.Value);
                    break;
                case "type":
                    switch (property.Value)
                    {
                        case "civilization":
                            EntityType = EntityType.Civilization;
                            break;
                        case "religion":
                            EntityType = EntityType.Religion;
                            break;
                        case "sitegovernment":
                            EntityType = EntityType.SiteGovernment;
                            break;
                        case "nomadicgroup":
                            EntityType = EntityType.NomadicGroup;
                            break;
                        case "outcast":
                            EntityType = EntityType.Outcast;
                            break;
                        case "migratinggroup":
                            EntityType = EntityType.MigratingGroup;
                            break;
                        case "performancetroupe":
                            EntityType = EntityType.PerformanceTroupe;
                            break;
                        case "guild":
                            EntityType = EntityType.Guild;
                            break;
                        case "militaryunit":
                            EntityType = EntityType.MilitaryUnit;
                            break;
                        case "merchantcompany":
                            EntityType = EntityType.MerchantCompany;
                            break;
                        default:
                            EntityType = EntityType.Unknown;
                            property.Known = false;
                            break;
                    }
                    break;
                case "child":
                    property.Known = true; // TODO child entities
                    break;
                case "site_link":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        EntitySiteLinks.Add(new EntitySiteLink(property.SubProperties, world));
                    }

                    break;
                case "entity_link":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (Property subProperty in property.SubProperties)
                        {
                            subProperty.Known = true;
                        }
                    }

                    world.AddEntityEntityLink(this, property);
                    break;
                case "worship_id":
                    var worshippedDeity = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    if (worshippedDeity != null)
                    {
                        Worshipped.Add(worshippedDeity);
                        worshippedDeity.AddWorshipper(this);
                    }
                    break;
                //case "claims":
                //    string[] coordinateStrings = property.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                //    foreach (var coordinateString in coordinateStrings)
                //    {
                //        string[] xYCoordinates = coordinateString.Split(',');
                //        if (xYCoordinates.Length == 2)
                //        {
                //            int x = Convert.ToInt32(xYCoordinates[0]);
                //            int y = Convert.ToInt32(xYCoordinates[1]);
                //            Coordinates.Add(new Location(x, y));
                //        }
                //    }
                //    break;
                case "entity_position":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        EntityPositions.Add(new EntityPosition(property.SubProperties, world));
                    }
                    break;
                case "entity_position_assignment":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        EntityPositionAssignments.Add(new EntityPositionAssignment(property.SubProperties, world));
                    }
                    break;
                case "histfig_id":
                    property.Known = true; // TODO historical figure == living members?
                    break;
                case "occasion":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        Occassions.Add(new EntityOccasion(property.SubProperties, world, this));
                    }
                    break;
                case "honor":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        Honors.Add(new Honor(property.SubProperties, world, this));
                    }
                    break;
                case "weapon":
                    Weapons.Add(property.Value);
                    break;
                case "profession":
                    Profession = property.Value.Replace("_", " ");
                    break;
            }
        }
        Type = EntityType.GetDescription();
        Subtype = Race?.NamePlural ?? string.Empty;
    }

    public override string ToString() { return Name ?? "UNKNOWN"; }

    public bool EqualsOrParentEquals(Entity? entity)
    {
        return entity != null && (this == entity || Parent == entity);
    }

    public string PrintEntity(bool link = true, DwarfObject? pov = null)
    {
        string entityString = ToLink(link, pov);
        if (Parent != null)
        {
            entityString += " of " + Parent.ToLink(link, pov);
        }
        return entityString;
    }

    //TODO: Check and possibly move logic
    public void AddOwnedSite(OwnerPeriod ownerPeriod)
    {
        if (SiteHistory.Exists(sh => sh.StartCause == ownerPeriod.StartCause && sh.Site == ownerPeriod.Site && sh.StartYear == ownerPeriod.StartYear))
        {
            return;
        }
        if ((ownerPeriod.StartCause == "UNKNOWN" || ownerPeriod.StartCause == "ancestral claim"))
        {
            SiteHistory.Insert(0, ownerPeriod);
        }
        else
        {
            SiteHistory.Add(ownerPeriod);
        }

        if (ownerPeriod.Owner != null && ownerPeriod.Owner != this)
        {
            Groups.Add(ownerPeriod.Owner);
        }

        if (!IsCiv && Parent != null)
        {
            Parent.AddOwnedSite(ownerPeriod);
            Race = Parent.Race;
        }
    }

    public void AddPopulations(List<Population> populations)
    {
        foreach (Population population in populations)
        {
            Population? popMatch = Populations.Find(pop => pop.Race.NamePlural.Equals(population.Race.NamePlural, StringComparison.InvariantCultureIgnoreCase));
            if (popMatch != null)
            {
                popMatch.Count += population.Count;
            }
            else
            {
                Populations.Add(new Population(World, population.Race, population.Count));
            }
        }
        Populations = Populations.OrderByDescending(pop => pop.Count).ToList();
        Parent?.AddPopulations(populations);
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "entity", Id, Title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, Title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    private string? _title;
    private string Title
    {
        get
        {
            if (string.IsNullOrEmpty(_title))
            {
                _title = GetAnchorTitle();
            }
            return _title;
        }
    }

    private string GetAnchorTitle()
    {
        var sb = new StringBuilder();
        sb.Append(GetTitle());
        if (Parent != null)
        {
            sb.Append("&#13");
            sb.Append("Part of ");
            sb.Append(Parent.Name);
        }
        if (CurrentSites.Count > 0)
        {
            sb.Append("&#13");
            sb.Append("Current Sites: ");
            sb.Append(CurrentSites.Count);
        }
        if (LostSites.Count > 0)
        {
            sb.Append("&#13");
            sb.Append("Lost Sites: ");
            sb.Append(LostSites.Count);
        }
        sb.Append("&#13");
        sb.Append("Events: ");
        sb.Append(EventCount);
        sb.Append("&#13");
        sb.Append("Chronicles: ");
        sb.Append(EventCollectionCount);
        return sb.ToString();
    }

    public string GetTitle()
    {
        var title = EntityType.GetDescription();
        if (Race != null && Race != CreatureInfo.Unknown)
        {
            title += " of ";
            title += Race.NamePlural;
        }
        return title;
    }

    public string GetSummary(bool link = true, DwarfObject? pov = null)
    {
        var sb = new StringBuilder();
        sb.Append(ToLink(link, pov));
        sb.Append(" was a ");
        sb.Append(EntityType.GetDescription().ToLower());
        if (Race != CreatureInfo.Unknown)
        {
            sb.Append(" of ");
            sb.Append(Race.NamePlural.ToLower());
        }
        if (Parent != null)
        {
            sb.Append(" of ");
            sb.Append(Parent.ToLink(link, pov));
        }

        switch (EntityType)
        {
            case EntityType.Religion:
                if (Worshipped.Count > 0)
                {
                    sb.Append(" centered around the worship of ");
                    sb.Append(Worshipped[0].ToLink(link, pov));
                }
                break;
            case EntityType.MilitaryUnit:
                bool isWorshipping = false;
                if (Worshipped.Count > 0)
                {
                    sb.Append(" devoted to the worship of ");
                    sb.Append(Worshipped[0].ToLink(link, pov));
                    isWorshipping = true;
                }
                if (Weapons.Count > 0)
                {
                    if (isWorshipping)
                    {
                        sb.Append(" and");
                    }
                    sb.Append(" dedicated to the mastery of the ");
                    sb.Append(string.Join(", the ", Weapons));
                }
                break;
            case EntityType.Guild:
                sb.Append(" of ");
                sb.Append(Profession);
                sb.Append("s");
                break;
        }
        sb.Append('.');
        return sb.ToString();
    }

    public override string GetIcon()
    {
        return Icon;
    }

    public void SetParent(Entity? parent)
    {
        if (parent == null || parent == this)
        {
            return;
        }

        if (parent.Name == Name)
        {
            return;
        }

        Parent = parent;
    }

    private MainCivilizationDto? _civilizationInfo;
    public MainCivilizationDto GetCivilizationInfo(IWorldMapImageGenerator worldMapImageGenerator)
    {
        if (_civilizationInfo == null)
        {
            _civilizationInfo = new MainCivilizationDto(worldMapImageGenerator, this);
        }
        return _civilizationInfo;
    }

    public List<LeaderTimelineDto> GetLeaderTimelines()
    {
        List<LeaderTimelineDto> timelines = [];

        // Primary source: Leaders and LeaderTypes parsed from history.txt
        if (LeaderTypes.Count > 0 && Leaders.Count == LeaderTypes.Count)
        {
            for (int i = 0; i < LeaderTypes.Count; i++)
            {
                string leaderType = LeaderTypes[i];
                var leadersList = Leaders[i];
                if (leadersList == null || leadersList.Count == 0)
                {
                    continue;
                }

                int posId = int.MaxValue;
                EntityPosition? epMatch = EntityPositions.FirstOrDefault(ep =>
                    string.Equals(ep.Name, leaderType, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ep.NameMale, leaderType, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ep.NameFemale, leaderType, StringComparison.OrdinalIgnoreCase) ||
                    (ep.Name != null && leaderType.Contains(ep.Name, StringComparison.OrdinalIgnoreCase)) ||
                    (ep.NameMale != null && leaderType.Contains(ep.NameMale, StringComparison.OrdinalIgnoreCase)) ||
                    (ep.NameFemale != null && leaderType.Contains(ep.NameFemale, StringComparison.OrdinalIgnoreCase)));

                if (epMatch != null)
                {
                    posId = epMatch.Id;
                }
                else
                {
                    var sampleHfPos = leadersList
                        .SelectMany(l => l.Positions ?? [])
                        .FirstOrDefault(p => p.Entity == this && (string.Equals(p.Title, leaderType, StringComparison.OrdinalIgnoreCase) || p.Title.Contains(leaderType, StringComparison.OrdinalIgnoreCase)));
                    if (sampleHfPos != null)
                    {
                        posId = sampleHfPos.PositionId;
                    }
                    else
                    {
                        posId = i;
                    }
                }

                var timelineDto = new LeaderTimelineDto
                {
                    PositionId = posId,
                    LeaderType = leaderType,
                    Leaders = []
                };

                for (int j = leadersList.Count - 1; j >= 0; j--)
                {
                    HistoricalFigure leader = leadersList[j];
                    HistoricalFigure? prevLeader = j > 0 ? leadersList[j - 1] : null;

                    timelineDto.Leaders.Add(CreateLeaderTimelineItemDto(leader, prevLeader, leaderType));
                }

                if (timelineDto.Leaders.Count > 0)
                {
                    timelines.Add(timelineDto);
                }
            }
        }

        // Secondary source: If no LeaderTypes from history.txt, build from World HistoricalFigures positions
        if (timelines.Count == 0 && World != null)
        {
            var hfPositionsForEntity = World.HistoricalFigures
                .SelectMany(hf => (hf.Positions ?? []).Where(p => p.Entity == this).Select(p => new { HistoricalFigure = hf, Position = p }))
                .ToList();

            if (hfPositionsForEntity.Count > 0)
            {
                var groupedByTitle = hfPositionsForEntity
                    .GroupBy(x => !string.IsNullOrEmpty(x.Position.Title) ? x.Position.Title : "Leader")
                    .ToList();

                foreach (var group in groupedByTitle)
                {
                    var sortedLeaders = group
                        .OrderBy(x => x.Position.StartYear ?? -1)
                        .ThenBy(x => x.HistoricalFigure.BirthYear)
                        .Select(x => x.HistoricalFigure)
                        .Distinct()
                        .ToList();

                    int posId = int.MaxValue;
                    EntityPosition? epMatch = EntityPositions.FirstOrDefault(ep =>
                        string.Equals(ep.Name, group.Key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ep.NameMale, group.Key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ep.NameFemale, group.Key, StringComparison.OrdinalIgnoreCase) ||
                        (ep.Name != null && group.Key.Contains(ep.Name, StringComparison.OrdinalIgnoreCase)) ||
                        (ep.NameMale != null && group.Key.Contains(ep.NameMale, StringComparison.OrdinalIgnoreCase)) ||
                        (ep.NameFemale != null && group.Key.Contains(ep.NameFemale, StringComparison.OrdinalIgnoreCase)));

                    if (epMatch != null)
                    {
                        posId = epMatch.Id;
                    }
                    else
                    {
                        var sampleHfPos = group.FirstOrDefault(x => x.Position.Entity == this)?.Position;
                        if (sampleHfPos != null)
                        {
                            posId = sampleHfPos.PositionId;
                        }
                    }

                    var timelineDto = new LeaderTimelineDto
                    {
                        PositionId = posId,
                        LeaderType = group.Key,
                        Leaders = []
                    };

                    for (int j = sortedLeaders.Count - 1; j >= 0; j--)
                    {
                        HistoricalFigure leader = sortedLeaders[j];
                        HistoricalFigure? prevLeader = j > 0 ? sortedLeaders[j - 1] : null;

                        timelineDto.Leaders.Add(CreateLeaderTimelineItemDto(leader, prevLeader, group.Key));
                    }

                    if (timelineDto.Leaders.Count > 0)
                    {
                        timelines.Add(timelineDto);
                    }
                }
            }
        }

        return timelines.OrderBy(t => t.PositionId).ThenBy(t => t.LeaderType).ToList();
    }

    private LeaderTimelineItemDto CreateLeaderTimelineItemDto(HistoricalFigure leader, HistoricalFigure? prevLeader, string leaderType)
    {
        var item = new LeaderTimelineItemDto
        {
            Id = leader.Id,
            Name = leader.Name,
            Link = leader.ToLink(true, this),
            Caste = leader.Caste,
            Race = leader.Race?.NameSingular ?? string.Empty,
            IsAlive = leader.IsAlive,
            BirthYear = leader.BirthYear,
            DeathYear = leader.DeathYear,
            DeathCause = leader.DeathCause != DeathCause.None ? leader.DeathCause.GetDescription() : string.Empty
        };

        var pos = leader.Positions?.OrderBy(p => p.StartYear ?? -1)
            .FirstOrDefault(p => p.Entity == this && (string.Equals(p.Title, leaderType, StringComparison.OrdinalIgnoreCase) || p.Title.Contains(leaderType, StringComparison.OrdinalIgnoreCase)))
            ?? leader.Positions?.FirstOrDefault(p => p.Entity == this);

        item.StartYear = pos?.StartYear;
        item.EndYear = pos?.EndYear ?? (leader.DeathYear > -1 ? leader.DeathYear : null);

        item.StartYearDisplay = pos?.StartYear == null || pos.StartYear == -1 ? "a time before time" : $"Year {pos.StartYear}";
        item.EndYearDisplay = item.EndYear == null ? "Present Day" : $"Year {item.EndYear}";

        int currentYear = World?.CurrentYear ?? 0;
        if (item.StartYear != null)
        {
            int startVal = item.StartYear.Value < 0 ? 0 : item.StartYear.Value;
            int endVal = item.EndYear ?? currentYear;
            int duration = endVal - startVal;
            if (duration <= 0)
            {
                item.ReignDuration = "Less than 1 year";
            }
            else
            {
                item.ReignDuration = item.EndYear == null ? $"{duration} years (reigning)" : $"{duration} years";
            }
        }
        else
        {
            item.ReignDuration = "Unknown duration";
        }

        EntityPosition? entityPos = EntityPositions.FirstOrDefault(ep =>
            string.Equals(ep.Name, leaderType, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ep.NameMale, leaderType, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ep.NameFemale, leaderType, StringComparison.OrdinalIgnoreCase) ||
            ep.Id == pos?.PositionId);

        if (entityPos != null)
        {
            item.PositionTitle = entityPos.GetTitleByCaste(leader.Caste);
        }
        else if (pos != null && !string.IsNullOrWhiteSpace(pos.Title))
        {
            item.PositionTitle = pos.Title;
        }
        else
        {
            item.PositionTitle = leaderType;
        }

        if (prevLeader == null)
        {
            item.PredecessorRelation = "Founder";
        }
        else
        {
            var link = leader.RelatedHistoricalFigures.FirstOrDefault(f => f.HistoricalFigure == prevLeader);
            bool isFemale = string.Equals(leader.Caste, "Female", StringComparison.OrdinalIgnoreCase);
            bool isMale = string.Equals(leader.Caste, "Male", StringComparison.OrdinalIgnoreCase);

            if (link != null)
            {
                switch (link.Type)
                {
                    case HistoricalFigureLinkType.Father:
                    case HistoricalFigureLinkType.Mother:
                        item.PredecessorRelation = isFemale ? $"Daughter of {prevLeader.Name}" : isMale ? $"Son of {prevLeader.Name}" : $"Child of {prevLeader.Name}";
                        break;
                    case HistoricalFigureLinkType.Child:
                        item.PredecessorRelation = isFemale ? $"Mother of {prevLeader.Name}" : isMale ? $"Father of {prevLeader.Name}" : $"Parent of {prevLeader.Name}";
                        break;
                    case HistoricalFigureLinkType.Spouse:
                    case HistoricalFigureLinkType.FormerSpouse:
                    case HistoricalFigureLinkType.DeceasedSpouse:
                        item.PredecessorRelation = isFemale ? $"Wife of {prevLeader.Name}" : isMale ? $"Husband of {prevLeader.Name}" : $"Spouse of {prevLeader.Name}";
                        break;
                    case HistoricalFigureLinkType.Apprentice:
                        item.PredecessorRelation = $"Apprentice of {prevLeader.Name}";
                        break;
                    case HistoricalFigureLinkType.Master:
                        item.PredecessorRelation = $"Master of {prevLeader.Name}";
                        break;
                    default:
                        item.PredecessorRelation = $"Replaced {prevLeader.Name} ({link.Type.GetDescription()})";
                        break;
                }
            }
            else
            {
                bool sharesFather = leader.RelatedHistoricalFigures.Any(r => r.Type == HistoricalFigureLinkType.Father && r.HistoricalFigure != null && prevLeader.RelatedHistoricalFigures.Any(p => p.Type == HistoricalFigureLinkType.Father && p.HistoricalFigure == r.HistoricalFigure));
                bool sharesMother = leader.RelatedHistoricalFigures.Any(r => r.Type == HistoricalFigureLinkType.Mother && r.HistoricalFigure != null && prevLeader.RelatedHistoricalFigures.Any(p => p.Type == HistoricalFigureLinkType.Mother && p.HistoricalFigure == r.HistoricalFigure));
                if (sharesFather || sharesMother)
                {
                    item.PredecessorRelation = isFemale ? $"Sister of {prevLeader.Name}" : isMale ? $"Brother of {prevLeader.Name}" : $"Sibling of {prevLeader.Name}";
                }
                else
                {
                    item.PredecessorRelation = $"Successor to {prevLeader.Name}";
                }
            }
        }

        return item;
    }
}

