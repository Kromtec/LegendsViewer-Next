using System.Globalization;
using System.Text.Json.Serialization;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Cytoscape;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldLinks;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class HistoricalFigure : WorldObject
{
    private const int MaxListItemCount = 100;

    private static readonly List<string> KnownEntitySubProperties = ["entity_id", "link_strength", "link_type", "position_profile_id", "start_year", "end_year"];
    private static readonly List<string> KnownSiteLinkSubProperties = ["link_type", "site_id", "sub_id", "entity_id", "occupation_id"];
    private static readonly List<string> KnownEntitySquadLinkProperties = ["squad_id", "squad_position", "entity_id", "start_year", "end_year"];

    public static readonly string ForceNatureIcon = HtmlStyleUtil.GetIconString("leaf");
    public static readonly string DeityIcon = HtmlStyleUtil.GetIconString("weather-sunset");
    public static readonly string NeuterIcon = HtmlStyleUtil.GetIconString("gender-non-binary");
    public static readonly string FemaleIcon = HtmlStyleUtil.GetIconString("gender-female");
    public static readonly string MaleIcon = HtmlStyleUtil.GetIconString("gender-male");

    public override string Type { get => Formatting.InitCaps(GetRaceString()); set => base.Type = value; }

    private string ShortName
    {
        get
        {
            if (string.IsNullOrEmpty(_shortName))
            {
                _shortName = Name.IndexOf(" ", StringComparison.Ordinal) >= 2 && !Name.StartsWith("The ")
                    ? Name.Substring(0, Name.IndexOf(" ", StringComparison.Ordinal))
                    : Name;
            }
            return _shortName;
        }
    }

    private string RaceString
    {
        get
        {
            if (string.IsNullOrEmpty(_raceString))
            {
                _raceString = GetRaceString();
            }
            return _raceString;
        }
    }

    public string TitleRaceString
    {
        get
        {
            if (string.IsNullOrEmpty(_TitleRaceString))
            {
                _TitleRaceString = GetRaceString();
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;
                _TitleRaceString = textInfo.ToTitleCase(_TitleRaceString);
            }
            return _TitleRaceString;
        }
    }

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

    public CreatureInfo Race { get; set; } = CreatureInfo.Unknown;
    public string Caste { get; set; } = string.Empty;
    public string AssociatedType { get; set; } = string.Empty;
    public string PreviousRace { get; set; } = string.Empty;

    [JsonIgnore]
    public int EntityPopulationId { get; set; }

    [JsonIgnore]
    public EntityPopulation? EntityPopulation { get; set; }
    public HfState CurrentState { get; set; } = HfState.None;

    [JsonIgnore]
    public List<int> UsedIdentityIds { get; set; } = [];
    [JsonIgnore]
    public int CurrentIdentityId { get; set; }

    [JsonIgnore]
    public List<Artifact> HoldingArtifacts { get; set; } = [];
    public List<string> HoldingArtifactLinks => HoldingArtifacts.ConvertAll(x => x.ToLink(true, this));

    public List<State> States { get; set; } = [];

    public List<CreatureType> CreatureTypes { get; set; } = [];

    [JsonIgnore]
    public List<HistoricalFigureLink> RelatedHistoricalFigures { get; set; } = [];
    public List<ListItemDto> RelatedHistoricalFigureList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (HistoricalFigureLink link in RelatedHistoricalFigures.Where(f => f.Type != HistoricalFigureLinkType.Deity))
            {
                if (link.HistoricalFigure == null)
                {
                    continue;
                }

                list.Add(new ListItemDto
                {
                    Title = $"{link.Type.GetDescription()}",
                    Subtitle = $"{link.HistoricalFigure?.ToLink(true, this)}",
                });
            }
            return list;
        }
    }
    public List<ListItemDto> WorshippedDeities
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (HistoricalFigureLink link in RelatedHistoricalFigures.Where(f => f.Type == HistoricalFigureLinkType.Deity).OrderByDescending(f => f.Strength))
            {
                if (link.HistoricalFigure == null)
                {
                    continue;
                }
                string associatedSpheres = string.Join(", ", link.HistoricalFigure.Spheres);
                list.Add(new ListItemDto
                {
                    Title = link.HistoricalFigure.ToLink(true, this),
                    Subtitle = associatedSpheres,
                    Append = HtmlStyleUtil.GetChipString(link.Strength.ToString())
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<(HistoricalFigure Worshipper, int Strength)>? WorshippingFigures { get; set; }
    public List<ListItemDto> WorshippingFiguresList
    {
        get
        {
            var list = new List<ListItemDto>();
            if (WorshippingFigures != null)
            {
                foreach (var (worshipper, strength) in WorshippingFigures.OrderByDescending(w => w.Strength).Take(MaxListItemCount))
                {
                    list.Add(new ListItemDto
                    {
                        Title = worshipper?.ToLink(true, this),
                        Subtitle = worshipper?.GetLastAssignmentString(),
                        Append = HtmlStyleUtil.GetChipString(strength.ToString())
                    });
                }
                if (WorshippingFigures.Count > MaxListItemCount)
                {
                    list.Add(new ListItemDto
                    {
                        Subtitle = $"... and {WorshippingFigures.Count - MaxListItemCount} more!",
                    });
                }
            }

            return list;
        }
    }

    [JsonIgnore]
    public List<Entity>? WorshippingEntities { get; set; }
    public List<ListItemDto> WorshippingEntitiesList
    {
        get
        {
            var list = new List<ListItemDto>();
            if (WorshippingEntities != null)
            {
                foreach (var worshipper in WorshippingEntities)
                {
                    list.Add(new ListItemDto
                    {
                        Title = worshipper?.ToLink(true, this),
                        Subtitle = worshipper?.EntityType.GetDescription()
                    });
                }
            }

            return list;
        }
    }

    public List<SiteProperty> SiteProperties { get; set; } = [];
    public List<EntityReputation> Reputations { get; set; } = [];
    public List<RelationshipProfileHf> RelationshipProfiles { get; set; } = [];

    [JsonIgnore]
    public Dictionary<int, RelationshipProfileHf> RelationshipProfilesOfIdentities { get; set; } = []; // TODO not used in Legends Mode

    [JsonIgnore]
    public List<EntityLink> RelatedEntities { get; set; } = [];
    public List<ListItemDto> RelatedEntityList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (EntityLink link in RelatedEntities)
            {
                if (link.Entity == null)
                {
                    continue;
                }
                string subtitle = $"{link.Type.GetDescription()} of the {link.Entity?.Type.GetDescription()}";
                if (link.PositionId >= 0)
                {
                    var assignment = link.Entity?.EntityPositionAssignments.ElementAtOrDefault(link.PositionId);
                    if (assignment != null)
                    {
                        EntityPosition? position = link.Entity?.EntityPositions.Find(pos => pos.Id == assignment.PositionId);
                        if (position != null)
                        {
                            string positionTitle = position.GetTitleByCaste(Caste);
                            if (link.EndYear > -1)
                            {
                                subtitle = $"Former {positionTitle} of the {link.Entity?.Type.GetDescription()}";
                            }
                            else
                            {
                                subtitle = $"{positionTitle} of the {link.Entity?.Type.GetDescription()}";
                            }
                        }
                    }
                }

                list.Add(new ListItemDto
                {
                    Title = subtitle,
                    Subtitle = $"{link.Entity?.ToLink(true, this)}",
                    Append = HtmlStyleUtil.GetChipString(link.Strength.ToString())
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<SiteLink> RelatedSites { get; set; } = [];
    public List<ListItemDto> RelatedSiteList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (SiteLink link in RelatedSites)
            {
                if (link.Site == null)
                {
                    continue;
                }
                list.Add(new ListItemDto
                {
                    Title = $"{link.Type.GetDescription()} ({link.Site?.Type.GetDescription()})",
                    Subtitle = $"{link.Site?.ToLink(true, this)}",
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    // Forces can have related regions
    public List<WorldRegion> RelatedRegions { get; set; } = [];

    [JsonIgnore]
    public List<Skill> Skills { get; set; } = [];
    public List<SkillDescription> SkillDescriptions => [.. Skills.Select(SkillDictionary.LookupSkill).OrderByDescending(d => d.Points)];

    public List<VagueRelationship> VagueRelationships { get; set; } = [];
    public List<ListItemDto> VagueRelationshipList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (VagueRelationship link in VagueRelationships)
            {
                var hf = World?.GetHistoricalFigure(link.HfId);
                if (hf == null)
                {
                    continue;
                }
                list.Add(new ListItemDto
                {
                    Title = $"{link.Type.GetDescription()}",
                    Subtitle = $"{hf.ToLink(true, this)}",
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<Structure> DedicatedStructures { get; set; } = [];
    public List<string> DedicatedStructuresLinks => DedicatedStructures.ConvertAll(x => x.ToLink(true, this));

    public int Age { get; set; } = -1;
    public int Appeared { get; set; } = -1;
    public int BirthYear { get; set; } = -1;
    public int BirthSeconds72 { get; set; } = -1;
    public int DeathYear { get; set; } = -1;
    public int DeathSeconds72 { get; set; } = -1;

    [JsonIgnore]
    public HfDied? DeathEvent { get; set; }
    public DeathCause DeathCause { get; set; } = DeathCause.None;
    public List<string> ActiveInteractions { get; set; } = [];
    public List<string> InteractionKnowledge { get; set; } = [];
    public string Goal { get; set; } = string.Empty;
    public string Interaction { get; set; } = string.Empty;

    public List<ListItemDto> MiscList
    {
        get
        {
            var list = new List<ListItemDto>();
            if (!string.IsNullOrEmpty(Goal))
            {
                list.Add(new ListItemDto
                {
                    Title = "Goal",
                    Subtitle = Goal
                });
            }

            if (Positions?.Count > 0)
            {
                var lastPosition = Positions[^1];
                list.Add(new ListItemDto
                {
                    Title = lastPosition.PrintTitle(true, this),
                    Subtitle = lastPosition.PrintReign(this),
                });
            }

            if (Age > -1)
            {
                list.Add(new ListItemDto
                {
                    Title = "Age",
                    Subtitle = Age.ToString() + (Alive ? "" : " ✝")
                });
            }
            if (BirthYear != -1)
            {
                list.Add(new ListItemDto
                {
                    Title = "Born",
                    Subtitle = Formatting.YearPlusSeconds72ToProsa(BirthYear, BirthSeconds72)
                });
            }
            if (DeathYear > -1)
            {
                list.Add(new ListItemDto
                {
                    Title = "Death",
                    Subtitle = $"{Formatting.YearPlusSeconds72ToProsa(DeathYear, DeathSeconds72)} {(DeathEvent != null ? DeathEvent.GetDeathString(true, this) : "")}"
                });
            }
            if (Spheres.Count > 0)
            {
                list.Add(new ListItemDto
                {
                    Title = "Spheres",
                    Subtitle = string.Join(", ", Spheres)
                });
            }
            if (RelatedRegions.Count > 0)
            {
                list.Add(new ListItemDto
                {
                    Title = "Related Regions",
                    Subtitle = string.Join(", ", RelatedRegions.ConvertAll(region => region.ToLink()))
                });
            }
            if (WorshippingFigures != null)
            {
                list.Add(new ListItemDto
                {
                    Title = "Worshipped By",
                    Subtitle = $"{WorshippingFigures.Count} historical figures"
                });
            }
            if (LineageCurseParent != null)
            {
                string curse = "Curse";
                if (!string.IsNullOrWhiteSpace(Interaction))
                {
                    curse = Formatting.InitCaps(Interaction);
                }
                list.Add(new ListItemDto
                {
                    Title = $"Lineage {curse} Parent",
                    Subtitle = LineageCurseParent.ToLink(true, this)
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public HistoricalFigure? LineageCurseParent { get; set; }

    [JsonIgnore]
    public List<HistoricalFigure> LineageCurseChilds { get; set; } = [];
    public List<string> LineageCurseChildLinks => LineageCurseChilds.ConvertAll(x => x.ToLink(true, this));

    public List<string> Spheres { get; set; } = [];

    public List<ListItemDto> JourneyPets { get; set; } = [];

    [JsonIgnore]
    public List<HfDied> NotableKills { get; set; } = [];
    public List<ListItemDto> NotableKillList
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (var killEvent in NotableKills)
            {
                list.Add(new ListItemDto
                {
                    Title = $"{killEvent.HistoricalFigure?.ToLink(true, this)}",
                    Subtitle = $"{killEvent.GetDeathString(true, this)}",
                });
            }
            return list;
        }
    }

    [JsonIgnore]
    public List<HistoricalFigure> SnatchedHfs => Events
            .OfType<HfAbducted>()
            .Where(abduction => abduction.Snatcher == this && abduction.Target != null)
            .Select(abduction => abduction.Target!)
            .ToList();
    public List<string> SnatchedHfLinks => SnatchedHfs.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Battle> Battles { get; set; } = [];
    public List<string> BattleLinks => Battles.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Battle> BattlesAttacking => Battles.Where(battle => battle.NotableAttackers.Contains(this)).ToList();
    public List<string> BattlesAttackingLinks => BattlesAttacking.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Battle> BattlesDefending => Battles.Where(battle => battle.NotableDefenders.Contains(this)).ToList();
    public List<string> BattlesDefendingLinks => BattlesDefending.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<Battle> BattlesNonCombatant => Battles.Where(battle => battle.NonCombatants.Contains(this)).ToList();
    public List<string> BattlesNonCombatantLinks => BattlesNonCombatant.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public List<HfPosition>? Positions { get; set; }
    public List<ListItemDto> PositionList
    {
        get
        {
            var list = new List<ListItemDto>();
            if (Positions?.Count > 1)
            {
                foreach (var position in Positions)
                {
                    list.Add(new ListItemDto
                    {
                        Title = position.PrintTitle(true, this),
                        Subtitle = position.PrintReign(this),
                    });
                }
            }
            return list;
        }
    }

    [JsonIgnore]
    public Entity? WorshippedBy { get; set; }

    [JsonIgnore]
    public List<BeastAttack> BeastAttacks { get; set; } = [];
    public List<string> BeastAttackLinks => BeastAttacks.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public HonorEntity? HonorEntity { get; set; }
    [JsonIgnore]
    public List<IntrigueActor> IntrigueActors { get; set; } = [];
    [JsonIgnore]
    public List<IntriguePlot> IntriguePlots { get; set; } = [];
    [JsonIgnore]
    public List<Identity> Identities { get; set; } = [];

    private CytoscapeData? _familyTreeData;
    public CytoscapeData? FamilyTreeData
    {
        get
        {
            if (_familyTreeData == null &&
            RelatedHistoricalFigures.Any(rel => rel.Type == HistoricalFigureLinkType.Mother ||
                                                rel.Type == HistoricalFigureLinkType.Father ||
                                                rel.Type == HistoricalFigureLinkType.Child))
            {
                _familyTreeData = this.CreateFamilyTreeElements();
            }
            return _familyTreeData;
        }

        set => _familyTreeData = value;
    }

    public bool Alive
    {
        get => DeathYear == -1;
        set { }
    }

    public bool Deity { get; set; }
    public bool Skeleton { get; set; }
    public bool Force { get; set; }
    public bool Zombie { get; set; }
    public bool Ghost { get; set; }
    public bool Animated { get; set; }
    public string AnimatedType { get; set; } = string.Empty;
    public bool Adventurer { get; set; }
    public string? BreedId { get; set; }
    public bool IsMainCivLeader { get; internal set; }

    private string? _shortName;
    private string? _raceString;
    private string? _TitleRaceString;
    private string? _title;

    public HistoricalFigure()
    {
        Name = "an unknown creature";
    }

    public override string ToString()
    {
        return Name;
    }

    public HistoricalFigure(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "appeared": Appeared = Convert.ToInt32(property.Value); break;
                case "birth_year": BirthYear = Convert.ToInt32(property.Value); break;
                case "birth_seconds72": BirthSeconds72 = Convert.ToInt32(property.Value); break;
                case "death_year": DeathYear = Convert.ToInt32(property.Value); break;
                case "death_seconds72": DeathSeconds72 = Convert.ToInt32(property.Value); break;
                case "name": Name = Formatting.InitCaps(property.Value.Replace("'", "`")); break;
                case "race": Race = world.GetCreatureInfo(property.Value); break;
                case "caste": Caste = Formatting.InitCaps(property.Value); break;
                case "associated_type": AssociatedType = Formatting.InitCaps(property.Value); break;
                case "deity": Deity = true; property.Known = true; break;
                case "skeleton": Skeleton = true; property.Known = true; break;
                case "force": Force = true; property.Known = true; Race = world.GetCreatureInfo("Force"); break;
                case "zombie": Zombie = true; property.Known = true; break;
                case "ghost": Ghost = true; property.Known = true; break;
                case "hf_link": //Will be processed after all HFs have been loaded
                    world.AddHFtoHfLink(this, property);
                    property.Known = true;
                    List<string> knownSubProperties = ["hfid", "link_strength", "link_type"];
                    if (property.SubProperties != null)
                    {
                        foreach (string subPropertyName in knownSubProperties)
                        {
                            Property? subProperty = property.SubProperties.Find(property1 => property1.Name == subPropertyName);
                            if (subProperty != null)
                            {
                                subProperty.Known = true;
                            }
                        }
                    }

                    break;

                case "entity_link":
                case "entity_former_position_link":
                case "entity_position_link":
                    world.AddHFtoEntityLink(this, property);
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (string subPropertyName in KnownEntitySubProperties)
                        {
                            Property? subProperty = property.SubProperties.Find(property1 => property1.Name == subPropertyName);
                            if (subProperty != null)
                            {
                                subProperty.Known = true;
                            }
                        }
                    }

                    break;

                case "entity_reputation":
                    world.AddReputation(this, property);
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (string subPropertyName in Reputation.KnownReputationSubProperties)
                        {
                            Property? subProperty = property.SubProperties.Find(property1 => property1.Name == subPropertyName);
                            if (subProperty != null)
                            {
                                subProperty.Known = true;
                            }
                        }
                    }

                    break;

                case "entity_squad_link":
                case "entity_former_squad_link":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (string subPropertyName in KnownEntitySquadLinkProperties)
                        {
                            Property? subProperty = property.SubProperties.Find(property1 => property1.Name == subPropertyName);
                            if (subProperty != null)
                            {
                                subProperty.Known = true;
                            }
                        }
                    }

                    break;

                case "relationship_profile_hf":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        RelationshipProfiles.Add(new RelationshipProfileHf(property.SubProperties, RelationShipProfileType.Unknown));
                    }
                    break;
                case "relationship_profile_hf_identity":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        var relationshipProfileHfIdentity = new RelationshipProfileHf(property.SubProperties, RelationShipProfileType.Identity);
                        RelationshipProfilesOfIdentities.Add(relationshipProfileHfIdentity.Id, relationshipProfileHfIdentity);
                    }
                    break;

                case "relationship_profile_hf_visual":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        RelationshipProfiles.Add(new RelationshipProfileHf(property.SubProperties, RelationShipProfileType.Visual));
                    }
                    break;

                case "relationship_profile_hf_historical":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        RelationshipProfiles.Add(new RelationshipProfileHf(property.SubProperties, RelationShipProfileType.Historical));
                    }
                    break;

                case "site_link":
                    world.AddHFtoSiteLink(this, property);
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (string subPropertyName in KnownSiteLinkSubProperties)
                        {
                            Property? subProperty = property.SubProperties.Find(property1 => property1.Name == subPropertyName);
                            if (subProperty != null)
                            {
                                subProperty.Known = true;
                            }
                        }
                    }

                    break;

                case "hf_skill":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        var skill = new Skill(property.SubProperties);
                        Skills.Add(skill);
                    }
                    break;

                case "active_interaction": ActiveInteractions.Add(string.Intern(property.Value)); break;
                case "interaction_knowledge": InteractionKnowledge.Add(string.Intern(property.Value)); break;
                case "animated": Animated = true; property.Known = true; break;
                case "animated_string": if (AnimatedType != "") { throw new Exception("Animated Type already exists."); } AnimatedType = Formatting.InitCaps(property.Value); break;
                case "journey_pet":
                    var creatureInfo = world.GetCreatureInfo(property.Value);
                    JourneyPets.Add(new ListItemDto
                    {
                        Title = creatureInfo != CreatureInfo.Unknown ? creatureInfo.NameSingular : Formatting.FormatRace(property.Value)
                    });
                    break;
                case "goal": Goal = Formatting.InitCaps(property.Value); break;
                case "sphere": Spheres.Add(property.Value); break;
                case "current_identity_id": CurrentIdentityId = Convert.ToInt32(property.Value); break;
                case "used_identity_id": UsedIdentityIds.Add(Convert.ToInt32(property.Value)); break;
                case "ent_pop_id": EntityPopulationId = Convert.ToInt32(property.Value); break;
                case "holds_artifact":
                    var artifact = world.GetArtifact(Convert.ToInt32(property.Value));
                    if (artifact != null)
                    {
                        HoldingArtifacts.Add(artifact);
                        artifact.Holder = this;
                    }
                    break;

                case "adventurer":
                    Adventurer = true;
                    property.Known = true;
                    break;

                case "breed_id":
                    BreedId = property.Value;
                    if (!string.IsNullOrWhiteSpace(BreedId))
                    {
                        if (world.Breeds.ContainsKey(BreedId))
                        {
                            world.Breeds[BreedId].Add(this);
                        }
                        else
                        {
                            world.Breeds.Add(BreedId, [this]);
                        }
                    }
                    break;

                case "sex": property.Known = true; break;
                case "site_property":
                    // is resolved in SiteProperty.Resolve()
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        foreach (Property subProperty in property.SubProperties)
                        {
                            switch (subProperty.Name)
                            {
                                case "site_id":
                                case "property_id":
                                    subProperty.Known = true;
                                    break;
                            }
                        }
                    }
                    break;

                case "vague_relationship":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        VagueRelationships.Add(new VagueRelationship(property.SubProperties));
                    }
                    break;

                case "honor_entity":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        HonorEntity = new HonorEntity(property.SubProperties, world);
                    }
                    break;

                case "intrigue_actor":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        IntrigueActors.Add(new IntrigueActor(property.SubProperties));
                    }
                    break;

                case "intrigue_plot":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        IntriguePlots.Add(new IntriguePlot(property.SubProperties));
                    }
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = !string.IsNullOrWhiteSpace(AnimatedType) ? Formatting.InitCaps(AnimatedType) : "(Unnamed)";
        }
        Subtype = Caste ?? string.Empty;
        if (Adventurer)
        {
            world.AddPlayerRelatedDwarfObjects(this);
        }
        Icon = GetIcon();
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            if (pov == null || pov != this)
            {
                if (pov != null && pov.GetType() == typeof(BeastAttack) && (pov as BeastAttack)?.Beast == this) //Highlight Beast when printing Beast Attack Log
                {
                    return $"{HtmlStyleUtil.GetAnchorString(Icon, "hf", Id, Title, ShortName)}";
                }

                return worldEvent != null
                    ? $"the {GetRaceStringByWorldEvent(worldEvent)} {HtmlStyleUtil.GetAnchorString(Icon, "hf", Id, Title, Name)}"
                    : $"the {RaceString} {HtmlStyleUtil.GetAnchorString(Icon, "hf", Id, Title, Name)}";
            }
            return $"{HtmlStyleUtil.GetAnchorString("", "hf", Id, Title, HtmlStyleUtil.CurrentDwarfObject(ShortName))}";
        }
        if (pov == null || pov != this)
        {
            return worldEvent != null ? $"{GetRaceStringByWorldEvent(worldEvent)} {Name}" : $"{RaceString} {Name}";
        }
        return ShortName;
    }

    public override string GetIcon()
    {
        if (Force)
        {
            return ForceNatureIcon;
        }
        if (Deity)
        {
            return DeityIcon;
        }
        if (Caste == "Female")
        {
            return FemaleIcon;
        }
        if (Caste == "Male")
        {
            return MaleIcon;
        }
        return Caste == "Default" ? NeuterIcon : "";
    }

    private string GetAnchorTitle()
    {
        string title = "";

        var lastNoblePosition = GetLastNoblePosition();
        if (!string.IsNullOrWhiteSpace(lastNoblePosition))
        {
            title += lastNoblePosition;
            title += "&#13";
        }
        else
        {
            var assignmentString = GetLastAssignmentString();
            if (!string.IsNullOrWhiteSpace(assignmentString))
            {
                title += assignmentString;
                title += "&#13";
            }
        }
        if (!string.IsNullOrWhiteSpace(AssociatedType) && AssociatedType != "Standard")
        {
            title += AssociatedType;
            title += "&#13";
        }
        title += !string.IsNullOrWhiteSpace(Caste) && Caste != "Default" ? Caste + " " : "";
        title += Formatting.InitCaps(RaceString);
        if (BirthYear != -1)
        {
            title += "&#13";
            title += $"Born: {BirthYear}";
        }
        if (!Alive)
        {
            title += "&#13";
            title += $"Died: {DeathYear}";
        }
        if (Age > -1)
        {
            title += "&#13";
            title += $"Age: {Age} years {(Alive ? "" : "✝")}";
        }
        title += "&#13";
        title += "Events: " + Events.Count;
        return title;
    }

    public string GetLastNoblePosition()
    {
        string title = "";
        if (Positions?.Count > 0)
        {
            var lastHfposition = Positions[^1];
            title += lastHfposition.PrintTitle(false, this);
        }
        return title;
    }
    private List<HfJob>? _jobs;

    public void StartPositionAssignment(Entity? entity, int? startYear, int positionId, string title)
    {
        if (Positions == null)
        {
            Positions = [];
        }
        if (Positions.Exists(p => string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase) && p.StartYear == startYear && p.Entity == entity))
        {
            return;
        }
        Positions.Add(new HfPosition(entity, startYear, null, positionId, title));
        if (entity != null && entity.IsCiv && positionId == 0)
        {
            IsMainCivLeader = true;
        }
    }

    public void EndPositionAssignment(Entity? entity, int? endYear, int positionId, string title)
    {
        if (Positions == null)
        {
            Positions = [];
        }
        var position = Positions.LastOrDefault(p => string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase) && p.Entity == entity && p.PositionId == positionId);
        if (position != null)
        {
            position.EndYear = endYear;
        }
        else
        {
            Positions.Add(new HfPosition(entity, null, endYear, positionId, title));
            if (entity != null && entity.IsCiv && positionId == 0)
            {
                IsMainCivLeader = true;
            }
        }
    }

    public string GetLastAssignmentString()
    {
        var lastAssignmentString = GetLastNoblePosition();
        if (!string.IsNullOrEmpty(lastAssignmentString))
        {
            return lastAssignmentString;
        }
        if (_jobs == null)
        {
            _jobs = [];
            foreach (var relevantEvent in Events.OfType<ChangeHfJob>())
            {
                var lastJob = _jobs.LastOrDefault();
                if (!string.Equals(relevantEvent.NewJob, "UNKNOWN JOB", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastJob != null)
                    {
                        lastJob.EndYear = relevantEvent.Year;
                    }
                    _jobs.Add(new HfJob(relevantEvent.Site, relevantEvent.Year, null, Formatting.InitCaps(relevantEvent.NewJob)));
                }
                else if (lastJob != null && relevantEvent.OldJob == lastJob.Title)
                {
                    lastJob.EndYear = relevantEvent.Year;
                }
            }
        }
        HfJob? lastAssignment = _jobs?.LastOrDefault();
        if (lastAssignment != null)
        {
            if (lastAssignment.EndYear != null)
            {
                return $"Former {lastAssignment.Title}";
            }
            return lastAssignment.Title;
        }
        return AssociatedType ?? string.Empty;
    }

    public string GetHighestSkillAsString()
    {
        if (Skills.Count > 0)
        {
            var highestSkill = Skills.OrderBy(skill => skill.Points).Last();
            var highestSkillDescription = SkillDictionary.LookupSkill(highestSkill);
            return $"{highestSkillDescription.Rank} {highestSkillDescription.Name}";
        }
        return string.Empty;
    }

    public class State
    {
        public HfState HfState { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }

        public State(HfState state, int start)
        {
            HfState = state;
            StartYear = start;
            EndYear = -1;
        }
    }

    public class CreatureType
    {
        public string Type { get; set; }
        public int StartYear { get; set; }
        public int StartMonth { get; set; }
        public int StartDay { get; set; }

        public CreatureType(string type, int startYear, int startMonth, int startDay)
        {
            Type = type;
            StartYear = startYear;
            StartMonth = startMonth;
            StartDay = startDay;
        }

        public CreatureType(string type, WorldEvent worldEvent) : this(type, worldEvent.Year, worldEvent.Month, worldEvent.Day)
        {
        }
    }

    public string CasteNoun(bool owner = false)
    {
        if (string.Equals(Caste, "male", StringComparison.OrdinalIgnoreCase))
        {
            return owner ? "his" : "he";
        }

        if (string.Equals(Caste, "female", StringComparison.OrdinalIgnoreCase))
        {
            return owner ? "her" : "she";
        }

        return owner ? "its" : "it";
    }

    public string GetRaceString()
    {
        if (Race == null)
        {
            Race = CreatureInfo.Unknown;
        }
        if (Deity)
        {
            return Race.NameSingular.ToLower() + " deity";
        }

        if (Force)
        {
            return "force";
        }

        string raceString = "";
        if (!string.IsNullOrWhiteSpace(PreviousRace))
        {
            raceString += PreviousRace.ToLower() + " turned ";
        }
        else if (!string.IsNullOrWhiteSpace(AnimatedType) && !Name.Contains("Corpse"))
        {
            raceString += AnimatedType.ToLower();
        }
        else
        {
            raceString += Race.NameSingular.ToLower();
        }

        foreach (var creatureType in CreatureTypes)
        {
            raceString += " " + creatureType.Type;
        }

        return raceString;
    }

    private string GetRaceStringByWorldEvent(WorldEvent worldEvent)
    {
        return GetRaceStringForTimeStamp(worldEvent.Year, worldEvent.Month, worldEvent.Day);
    }

    private string GetRaceStringForTimeStamp(int year, int month, int day)
    {
        if (CreatureTypes.Count == 0)
        {
            return RaceString;
        }

        List<CreatureType> relevantCreatureTypes = GetRelevantCreatureTypesByTimeStamp(year, month, day);
        string raceString = "";
        if (!string.IsNullOrWhiteSpace(PreviousRace))
        {
            raceString += PreviousRace.ToLower();
        }
        else if (!string.IsNullOrWhiteSpace(AnimatedType))
        {
            raceString += AnimatedType.ToLower();
        }
        else
        {
            raceString += Race.NameSingular.ToLower();
        }

        foreach (var creatureType in relevantCreatureTypes)
        {
            raceString += " " + creatureType.Type;
        }

        return raceString;
    }

    private List<CreatureType> GetRelevantCreatureTypesByTimeStamp(int year, int month, int day)
    {
        List<CreatureType> relevantCreatureTypes = [];
        foreach (var creatureType in CreatureTypes)
        {
            if (creatureType.StartYear < year)
            {
                relevantCreatureTypes.Add(creatureType);
            }
            else if (creatureType.StartYear == year)
            {
                if (creatureType.StartMonth < month)
                {
                    relevantCreatureTypes.Add(creatureType);
                }
                else if (creatureType.StartMonth == month && creatureType.StartDay < day)
                {
                    relevantCreatureTypes.Add(creatureType);
                }
            }
        }
        return relevantCreatureTypes;
    }
}