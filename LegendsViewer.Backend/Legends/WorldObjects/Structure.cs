using System.Text;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class Structure : WorldObject, IHasCoordinates
{
    public string AltName { get; set; } = "";
    public StructureType TypeEnum { get; set; } // legends_plus.xml

    public List<Location> Coordinates { get; set; } = [];

    [JsonIgnore]
    public List<int> InhabitantIDs { get; set; } = [];

    [JsonIgnore]
    public List<HistoricalFigure> Inhabitants { get; set; } = [];
    public List<string> InhabitantLinks => Inhabitants.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public int DeityId { get; set; } = -1;

    [JsonIgnore]
    public HistoricalFigure? Deity { get; set; } // legends_plus.xml
    public string? DeityToLink => Deity?.ToLink(true);

    [JsonIgnore]
    public int ReligionId { get; set; } = -1;

    [JsonIgnore]
    public Entity? Religion { get; set; } // legends_plus.xml
    public string? ReligionToLink => Religion?.ToLink(true);

    public StructureSubType StructureSubType { get; set; } // legends_plus.xml

    [JsonIgnore]
    public int DeityType { get; set; } = -1;

    [JsonIgnore]
    public HistoricalFigure? Owner { get; set; } // resolved from site properties
    public string? OwnerToLink => Owner?.ToLink(true);

    [JsonIgnore]
    public int EntityId { get; set; } = -1;

    [JsonIgnore]
    public Entity? Entity { get; set; }
    public string? EntityToLink => Entity?.ToLink(true);

    [JsonIgnore]
    public Site? Site { get; set; }
    public string? SiteToLink => Site?.ToLink(true);

    public int LocalId { get; set; }

    [JsonIgnore]
    public List<int> CopiedArtifactIds { get; set; } = [];

    [JsonIgnore]
    public List<Artifact> CopiedArtifacts { get; set; } = [];
    public List<string> CopiedArtifactLinks => CopiedArtifacts.ConvertAll(x => x.ToLink(true, this));

    public Structure(List<Property> properties, IWorld world, Site site)
        : base(properties, world)
    {
        Name = "Structure";

        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "id":
                case "local_id": 
                    LocalId = Convert.ToInt32(property.Value);
                    break;
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "name2": AltName = Formatting.InitCaps(property.Value); break;
                case "inhabitant":
                    InhabitantIDs.Add(Convert.ToInt32(property.Value));
                    break;
                case "deity":
                case "worship_hfid":
                    DeityId = Convert.ToInt32(property.Value);
                    break;
                case "deity_type":
                    DeityType = Convert.ToInt32(property.Value);
                    break;
                case "entity_id": EntityId = Convert.ToInt32(property.Value); break;
                case "religion": ReligionId = Convert.ToInt32(property.Value); break;
                case "copied_artifact_id": CopiedArtifactIds.Add(Convert.ToInt32(property.Value)); break;
                case "dungeon_type":
                    switch (property.Value)
                    {
                        case "0": StructureSubType = StructureSubType.Dungeon; break;
                        case "1": StructureSubType = StructureSubType.Sewers; break;
                        case "2": StructureSubType = StructureSubType.Catacombs; break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "subtype":
                    switch (property.Value)
                    {
                        case "standard": break;
                        case "catacombs": StructureSubType = StructureSubType.Catacombs; break;
                        case "sewers": StructureSubType = StructureSubType.Sewers; break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "type":
                    switch (property.Value)
                    {
                        case "mead_hall":
                        case "mead hall":
                            TypeEnum = StructureType.MeadHall; break;
                        case "market": TypeEnum = StructureType.Market; break;
                        case "keep": TypeEnum = StructureType.Keep; break;
                        case "temple": TypeEnum = StructureType.Temple; break;
                        case "dungeon": TypeEnum = StructureType.Dungeon; break;
                        case "tomb": TypeEnum = StructureType.Tomb; break;
                        case "inn_tavern":
                        case "inn tavern":
                            TypeEnum = StructureType.InnTavern; break;
                        case "underworld_spire":
                        case "underworld spire":
                            TypeEnum = StructureType.UnderworldSpire; break;
                        case "library": TypeEnum = StructureType.Library; break;
                        case "tower": TypeEnum = StructureType.Tower; break;
                        case "counting_house":
                        case "counting house":
                            TypeEnum = StructureType.CountingHouse; break;
                        case "guildhall": TypeEnum = StructureType.Guildhall; break;
                        case "hospital": TypeEnum = StructureType.Hospital; break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
            }
        }
        string icon = "";
        switch (TypeEnum)
        {
            case StructureType.MeadHall:
                icon = HtmlStyleUtil.GetIconString("glass-mug-variant");
                break;
            case StructureType.Market:
                icon = HtmlStyleUtil.GetIconString("scale-balance");
                break;
            case StructureType.Keep:
                icon = HtmlStyleUtil.GetIconString("castle");
                break;
            case StructureType.Temple:
                icon = HtmlStyleUtil.GetIconString("temple-buddhist");
                break;
            case StructureType.Dungeon:
                icon = HtmlStyleUtil.GetIconString("tunnel");
                break;
            case StructureType.InnTavern:
                icon = HtmlStyleUtil.GetIconString("food-turkey");
                break;
            case StructureType.Tomb:
                icon = HtmlStyleUtil.GetIconString("grave-stone");
                break;
            case StructureType.UnderworldSpire:
                icon = HtmlStyleUtil.GetIconString("star-three-points");
                break;
            case StructureType.Library:
                icon = HtmlStyleUtil.GetIconString("library");
                break;
            case StructureType.Tower:
                icon = HtmlStyleUtil.GetIconString("chess-rook");
                break;
            case StructureType.CountingHouse:
                icon = HtmlStyleUtil.GetIconString("bank-outline");
                break;
            case StructureType.Guildhall:
                icon = HtmlStyleUtil.GetIconString("town-hall");
                break;
            case StructureType.Hospital:
                icon = HtmlStyleUtil.GetIconString("medication-outline");
                break;
            default:
                icon = HtmlStyleUtil.GetIconString("home-silo");
                break;
        }
        Icon = icon;
        Site = site;
        Coordinates = site.Coordinates;

        Id = world.Structures.Count;
        world.Structures.Add(this);

        Type = TypeEnum.GetDescription();
        Subtype = StructureSubType != StructureSubType.Unknown ? StructureSubType.GetDescription() : "";
    }

    public void Resolve(IWorld world)
    {
        if (InhabitantIDs.Count > 0)
        {
            foreach (int inhabitantId in InhabitantIDs)
            {
                if (world.GetHistoricalFigure(inhabitantId) is HistoricalFigure inhabitant)
                {
                    Inhabitants.Add(inhabitant);
                }
            }
        }
        if (DeityId != -1)
        {
            Deity = world.GetHistoricalFigure(DeityId);
        }
        if (ReligionId != -1)
        {
            Religion = world.GetEntity(ReligionId);
        }
        if (EntityId != -1)
        {
            Entity = world.GetEntity(EntityId);
        }
        if (Deity != null && Religion != null)
        {
            if (!Religion.Worshipped.Contains(Deity))
            {
                Religion.Worshipped.Add(Deity);
                Deity.DedicatedStructures.Add(this);
            }
        }
        if (CopiedArtifactIds.Count > 0)
        {
            foreach (int copiedArtifactId in CopiedArtifactIds)
            {
                if(world.GetArtifact(copiedArtifactId) is Artifact artifact)
                {
                    CopiedArtifacts.Add(artifact);
                }
            }
        }
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            var sb = new StringBuilder();
            if (StructureSubType != StructureSubType.Unknown)
            {
                sb.Append(StructureSubType.GetDescription());
            }
            else
            {
                sb.Append(TypeEnum.GetDescription());
            }
            sb.Append("&#13");
            sb.Append("Events: ");
            sb.Append(Events.Count);

            string linkedString = pov != this
                ? $"{HtmlStyleUtil.GetAnchorString(Icon, "structure", Id, sb.ToString(), Name)}"
                : $"{HtmlStyleUtil.GetAnchorString(Icon, "structure", Id, sb.ToString(), HtmlStyleUtil.CurrentDwarfObject(Name))}";
            return linkedString;
        }
        return Icon + Name;
    }

    public override string ToString() { return Name; }

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
            if (rule.PropertyName.Equals("HasInhabitants", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Inhabitants.Count > 0 || InhabitantIDs.Count > 0))
            {
                return false;
            }

            if (rule.PropertyName.Equals("HasDeity", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Deity != null || DeityId != -1))
            {
                return false;
            }

            if (rule.PropertyName.Equals("HasReligion", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Religion != null || ReligionId != -1))
            {
                return false;
            }

            if (rule.PropertyName.Equals("HasEntity", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Entity != null || EntityId != -1))
            {
                return false;
            }

            if (rule.PropertyName.Equals("HasCopiedArtifacts", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(CopiedArtifacts.Count > 0 || CopiedArtifactIds.Count > 0))
            {
                return false;
            }

            if ((rule.PropertyName.Equals("StructureType", StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals(nameof(Type), StringComparison.InvariantCultureIgnoreCase)) &&
                !string.IsNullOrWhiteSpace(rule.Value))
            {
                string typeStr = TypeEnum.ToString();
                string typeDesc = TypeEnum.GetDescription();
                if (rule.Operator == FilterOperator.Equals &&
                    !typeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !typeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (rule.Operator == FilterOperator.NotEquals &&
                    (typeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     typeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            if ((rule.PropertyName.Equals("StructureSubType", StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals(nameof(Subtype), StringComparison.InvariantCultureIgnoreCase)) &&
                !string.IsNullOrWhiteSpace(rule.Value))
            {
                string subtypeStr = StructureSubType.ToString();
                string subtypeDesc = StructureSubType.GetDescription();
                if (rule.Operator == FilterOperator.Equals &&
                    !subtypeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !subtypeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !Subtype.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (rule.Operator == FilterOperator.NotEquals &&
                    (subtypeStr.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     subtypeDesc.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     Subtype.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            if ((rule.PropertyName.Equals("InhabitantCount", StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals("InhabitantsCount", StringComparison.InvariantCultureIgnoreCase)) &&
                int.TryParse(rule.Value, out int inhabitantCount) &&
                rule.ViolatesIntegerCriteria(Math.Max(Inhabitants.Count, InhabitantIDs.Count), inhabitantCount))
            {
                return false;
            }

            if ((rule.PropertyName.Equals("CopiedArtifactCount", StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals("CopiedArtifactsCount", StringComparison.InvariantCultureIgnoreCase)) &&
                int.TryParse(rule.Value, out int artifactCount) &&
                rule.ViolatesIntegerCriteria(Math.Max(CopiedArtifacts.Count, CopiedArtifactIds.Count), artifactCount))
            {
                return false;
            }
        }

        return true;
    }
}


