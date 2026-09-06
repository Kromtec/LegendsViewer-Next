using System.Text;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Cytoscape;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;
using System.Drawing;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.EventCollections;

public class War : EventCollection, IHasComplexSubtype
{
    public int Length { get; set; }
    public int DeathCount { get; set; }
    public int AttackerDeathCount { get; set; }
    public int DefenderDeathCount { get; set; }

    public List<ListItemDto> MiscList
    {
        get
        {
            var list = new List<ListItemDto>();

            if (Attacker != null)
            {
                list.Add(new ListItemDto
                {
                    Title = $"Attacker",
                    Subtitle = $"{Attacker.ToLink(true, this)} (Deaths: {AttackerDeathCount} ✝)",
                });
            }

            if (Defender != null)
            {
                list.Add(new ListItemDto
                {
                    Title = $"Defender",
                    Subtitle = $"{Defender.ToLink(true, this)} (Deaths: {DefenderDeathCount} ✝)",
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<HistoricalFigure> NotableDeaths => AllEvents?.OfType<HfDied>().Where(death => death.HistoricalFigure != null).Select(death => death.HistoricalFigure!).ToList() ?? [];
    public List<string> NotableDeathLinks => NotableDeaths.ConvertAll(x => x.ToLink(true, this));
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

    private CytoscapeData? _battleGraphData;
    public CytoscapeData? BattleGraphData
    {
        get
        {
            if (_battleGraphData == null && Battles.Count > 0)
            {
                Dictionary<int, CytoscapeNodeElement> nodes = [];
                List<CytoscapeEdgeElement> edges = [];

                foreach (var item in Battles)
                {
                    if (item.Attacker != null && !nodes.ContainsKey(item.Attacker.Id))
                    {
                        nodes.Add(item.Attacker.Id, item.Attacker.GetCytoscapeNode());
                    }
                    if (item.Defender != null && !nodes.ContainsKey(item.Defender.Id))
                    {
                        nodes.Add(item.Defender.Id, item.Defender.GetCytoscapeNode());
                    }
                    if (item.Attacker != null && item.Defender != null)
                    {
                        int edgeWidth = item.DeathCount / 10;
                        edges.Add(new CytoscapeEdgeElement(new CytoscapeEdgeData
                        {
                            Source = $"node-{item.Attacker.Id}",
                            Target = $"node-{item.Defender.Id}",
                            Href = $"/battle/{item.Id}",
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
                _battleGraphData = warGraphData;
            }
            return _battleGraphData;
        }

        set => _battleGraphData = value;
    }

    [JsonIgnore]
    public Entity? Attacker { get; set; }
    [JsonIgnore]
    public Entity? Defender { get; set; }
    [JsonIgnore]
    public List<Battle> Battles => EventCollections.OfType<Battle>().ToList();

    public List<ListItemDto> BattleList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (var battle in Battles)
            {
                list.Add(new ListItemDto
                {
                    Title = battle.ToLink(true, this),
                    Subtitle = battle.Subtype,
                    Append = HtmlStyleUtil.GetChipString($"{battle.DeathCount} ✝")
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<SiteConquered> Conquerings => EventCollections.OfType<SiteConquered>().ToList();
    [JsonIgnore]
    public List<Site?> SitesLost => Conquerings
        .Where(conquering => conquering.ConquerType == SiteConqueredType.Conquest || conquering.ConquerType == SiteConqueredType.Destruction)
        .Select(conquering => conquering.Site)
        .ToList();
    [JsonIgnore]
    public List<Site?> AttackerSitesLost => DefenderConquerings
        .Where(conquering => conquering.ConquerType == SiteConqueredType.Conquest || conquering.ConquerType == SiteConqueredType.Destruction)
        .Select(conquering => conquering.Site)
        .ToList();
    [JsonIgnore]
    public List<Site?> DefenderSitesLost => AttackerConquerings
        .Where(conquering => conquering.ConquerType == SiteConqueredType.Conquest || conquering.ConquerType == SiteConqueredType.Destruction)
        .Select(conquering => conquering.Site)
        .ToList();
    [JsonIgnore]
    public List<EventCollection> AttackerVictories { get; set; } = [];
    [JsonIgnore]
    public List<EventCollection> DefenderVictories { get; set; } = [];
    [JsonIgnore]
    public List<Battle> AttackerBattleVictories => EventCollections.OfType<Battle>().Where(battle => battle.Victor?.EqualsOrParentEquals(Attacker) ?? false).ToList();
    [JsonIgnore]
    public List<Battle> DefenderBattleVictories => EventCollections.OfType<Battle>().Where(battle => battle.Victor?.EqualsOrParentEquals(Defender) ?? false).ToList();
    [JsonIgnore]
    public List<Battle> ErrorBattles => EventCollections.OfType<Battle>()
        .Where(battle =>
            (!battle.Attacker?.EqualsOrParentEquals(Attacker) ?? false) && (!battle.Attacker?.EqualsOrParentEquals(Defender) ?? false) ||
            (!battle.Defender?.EqualsOrParentEquals(Defender) ?? false) && (!battle.Defender?.EqualsOrParentEquals(Attacker) ?? false))
        .ToList();
    [JsonIgnore]
    public List<SiteConquered> AttackerConquerings => EventCollections.OfType<SiteConquered>().Where(conquering => conquering.Attacker?.EqualsOrParentEquals(Attacker) ?? false).ToList();
    [JsonIgnore]
    public List<SiteConquered> DefenderConquerings => EventCollections.OfType<SiteConquered>().Where(conquering => conquering.Attacker?.EqualsOrParentEquals(Defender) ?? false).ToList();
    public double AttackerToDefenderKills
    {
        get
        {
            if (AttackerDeathCount == 0 && DefenderDeathCount == 0)
            {
                return 0;
            }

            return AttackerDeathCount == 0 ? double.MaxValue : Math.Round(DefenderDeathCount / Convert.ToDouble(AttackerDeathCount), 2);
        }
    }
    public double AttackerToDefenderVictories
    {
        get
        {
            if (DefenderBattleVictories.Count == 0 && AttackerBattleVictories.Count == 0)
            {
                return 0;
            }

            return DefenderBattleVictories.Count == 0
                ? double.MaxValue
                : Math.Round(AttackerBattleVictories.Count / Convert.ToDouble(DefenderBattleVictories.Count), 2);
        }
    }

    public War(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "aggressor_ent_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "defender_ent_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
            }
        }

        Defender?.Wars.Add(this);
        Defender?.Parent?.Wars.Add(this);

        Attacker?.Wars.Add(this);
        Attacker?.Parent?.Wars.Add(this);

        if (EndYear >= 0)
        {
            Length = EndYear - StartYear;
        }
        else if (world.Events.Count > 0)
        {
            Length = world.Events.Last().Year - StartYear;
        }
        Attacker?.AddEventCollection(this);
        if (Defender != Attacker)
        {
            Defender?.AddEventCollection(this);
        }

        Icon = HtmlStyleUtil.GetIconString("sword-cross");
    }

    public void GenerateComplexSubType()
    {
        if (string.IsNullOrEmpty(Subtype))
        {
            Subtype = $"{Attacker?.ToLink(true, this)} => {Defender?.ToLink(true, this)}";
        }
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = GetTitle();

            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "war", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    private string GetTitle()
    {
        var sb = new StringBuilder();
        sb.Append(Type);
        sb.Append("&#13");
        sb.Append(Attacker?.PrintEntity(false));
        sb.Append(" (Attacker)");
        sb.Append("&#13");
        sb.Append(Defender?.PrintEntity(false));
        sb.Append(" (Defender)");
        sb.Append("&#13");
        sb.Append("Deaths: ");
        sb.Append(DeathCount);
        sb.Append(" | (");
        sb.Append(StartYear);
        sb.Append(" - ");
        sb.Append(EndYear == -1 ? "Present" : EndYear.ToString());
        sb.Append(")");
        return sb.ToString();
    }

    public override string ToString()
    {
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
            if (rule.PropertyName.Equals("IsOngoing", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(EndYear == -1))
            {
                return false;
            }

            if (rule.PropertyName.Equals(nameof(DeathCount), StringComparison.InvariantCultureIgnoreCase) &&
                int.TryParse(rule.Value, out int deathCount) &&
                rule.ViolatesIntegerCriteria(DeathCount, deathCount))
            {
                return false;
            }

            if (rule.PropertyName.Equals(nameof(AttackerDeathCount), StringComparison.InvariantCultureIgnoreCase) &&
                int.TryParse(rule.Value, out int attackerDeaths) &&
                rule.ViolatesIntegerCriteria(AttackerDeathCount, attackerDeaths))
            {
                return false;
            }

            if (rule.PropertyName.Equals(nameof(DefenderDeathCount), StringComparison.InvariantCultureIgnoreCase) &&
                int.TryParse(rule.Value, out int defenderDeaths) &&
                rule.ViolatesIntegerCriteria(DefenderDeathCount, defenderDeaths))
            {
                return false;
            }

            if ((rule.PropertyName.Equals("BattleCount", StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals("Battles", StringComparison.InvariantCultureIgnoreCase)) &&
                int.TryParse(rule.Value, out int battleCount) &&
                rule.ViolatesIntegerCriteria(Battles.Count, battleCount))
            {
                return false;
            }

            if ((rule.PropertyName.Equals("SitesLostCount", StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals("SitesLost", StringComparison.InvariantCultureIgnoreCase)) &&
                int.TryParse(rule.Value, out int sitesLostCount) &&
                rule.ViolatesIntegerCriteria(SitesLost.Count, sitesLostCount))
            {
                return false;
            }

            if (rule.PropertyName.Equals(nameof(Length), StringComparison.InvariantCultureIgnoreCase) &&
                int.TryParse(rule.Value, out int length) &&
                rule.ViolatesIntegerCriteria(Length, length))
            {
                return false;
            }
        }

        return true;
    }
}

