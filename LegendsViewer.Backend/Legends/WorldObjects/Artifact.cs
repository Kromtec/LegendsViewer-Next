using System.Text;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class Artifact : WorldObject, IHasCoordinates
{
    private const string DefaultName = "Untitled";

    [JsonIgnore]
    public HistoricalFigure? Creator { get; set; }
    public string? CreatorLink => Creator?.ToLink(true, this);

    [JsonIgnore]
    public int HolderId { get; set; } = -1;
    [JsonIgnore]
    public HistoricalFigure? Holder { get; set; }
    public string? HolderLink => Holder?.ToLink(true, this);

    public string? Item { get; set; }
    public string? Description { get; set; } // legends_plus.xml
    public string? Material { get; set; } // legends_plus.xml
    public int PageCount { get; set; } // legends_plus.xml

    [JsonIgnore]
    public int WrittenContentId { get; set; } = -1;

    [JsonIgnore]
    public WrittenContent? WrittenContent { get; set; }
    public string? WrittenContentLink => WrittenContent?.ToLink(true, this);

    public int AbsTileX { get; set; }
    public int AbsTileY { get; set; }
    public int AbsTileZ { get; set; }
    public List<Location> Coordinates { get; set; } = [];

    [JsonIgnore]
    public Structure? Structure { get; set; }
    public string? StructureLink => Structure?.ToLink(true, this);

    [JsonIgnore]
    public Site? Site { get; set; }
    public string? SiteLink => Site?.ToLink(true, this);

    [JsonIgnore]
    public WorldRegion? Region { get; set; }
    public string? RegionLink => Region?.ToLink(true, this);

    public Artifact(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        Icon = HtmlStyleUtil.GetIconString("diamond-stone");
        Name = DefaultName;
        Type = "Unknown";
        Subtype = "";

        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(CheckArtifactName(property.Value)); break;
                case "item":
                    if (property.SubProperties != null)
                    {
                        property.Known = true;
                        foreach (Property subProperty in property.SubProperties)
                        {
                            switch (subProperty.Name)
                            {
                                case "name_string":
                                    Item = Formatting.InitCaps(CheckArtifactName(subProperty.Value));
                                    break;
                                case "page_number":
                                    PageCount = Convert.ToInt32(subProperty.Value);
                                    break;
                                case "page_written_content_id":
                                case "writing_written_content_id":
                                    WrittenContentId = Convert.ToInt32(subProperty.Value);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Item = Formatting.InitCaps(property.Value);
                    }
                    break;
                case "item_type": Type = Formatting.InitCaps(property.Value); break;
                case "item_subtype": Subtype = Formatting.InitCaps(property.Value); break;
                case "item_description": Description = Formatting.InitCaps(property.Value); break;
                case "mat": Material = string.Intern(property.Value); break;
                case "page_count": PageCount = Convert.ToInt32(property.Value); break;
                case "abs_tile_x": AbsTileX = Convert.ToInt32(property.Value); break;
                case "abs_tile_y": AbsTileY = Convert.ToInt32(property.Value); break;
                case "abs_tile_z": AbsTileZ = Convert.ToInt32(property.Value); break;
                case "writing": WrittenContentId = Convert.ToInt32(property.Value); break;
                case "site_id":
                    Site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "subregion_id":
                    Region = world.GetRegion(Convert.ToInt32(property.Value));
                    break;
                case "holder_hfid":
                    HolderId = Convert.ToInt32(property.Value);
                    break;
                case "structure_local_id":
                    Structure = Site?.Structures.Find(structure => structure.LocalId == Convert.ToInt32(property.Value));
                    break;
            }
        }
        if (AbsTileX > 0 && AbsTileY > 0)
        {
            Coordinates.Add(new Location(AbsTileX / 816, AbsTileY / 816));
        }
        else if (Site != null)
        {
            Coordinates.AddRange(Site.Coordinates);
        }
        if (Name == DefaultName && !string.IsNullOrEmpty(Item))
        {
            Name = Item;
        }
    }

    private static string CheckArtifactName(ReadOnlySpan<char> text)
    {
        // Determine the start and end characters to replace if needed
        char firstChar = text.Length > 0 ? text[0] : '\0';
        char lastChar = text.Length > 1 ? text[^1] : '\0';

        // If no replacements are necessary, return the original string
        if (firstChar != ' ' && lastChar != ' ')
        {
            return text.ToString();
        }

        // Allocate a new array to modify the content if changes are needed
        Span<char> result = stackalloc char[text.Length];
        text.CopyTo(result);

        // Replace the first character if it's a space
        if (firstChar == ' ')
        {
            result[0] = '‹';
        }

        // Replace the last character if it's a space
        if (lastChar == ' ')
        {
            result[^1] = '›';
        }

        // Convert the modified span back to a string
        return new string(result);
    }

    public void Resolve(IWorld world)
    {
        if (HolderId > -1)
        {
            Holder = world.GetHistoricalFigure(HolderId);
            if (Holder?.HoldingArtifacts.Contains(this) == false)
            {
                Holder.HoldingArtifacts.Add(this);
            }
        }
        if (WrittenContentId > -1)
        {
            WrittenContent = world.GetWrittenContent(WrittenContentId);
            if (WrittenContent != null)
            {
                WrittenContent.Artifact = this;
            }
        }
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            var sb = new StringBuilder();
            sb.Append("Artifact");
            if (!string.IsNullOrEmpty(Type))
            {
                sb.Append(", ");
                sb.Append(Type);
            }
            sb.Append("&#13");
            sb.Append("Events: ");
            sb.Append(Events.Count);
            string title = sb.ToString();
            sb.Clear();
            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "artifact", Id, title, Name)
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
            if (rule.PropertyName.Equals("IsHeld", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Holder != null || HolderId > -1))
            {
                return false;
            }

            if (rule.PropertyName.Equals("IsWrittenContent", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(WrittenContent != null || WrittenContentId > -1))
            {
                return false;
            }

            if (rule.PropertyName.Equals("IsLocatedInSite", StringComparison.InvariantCultureIgnoreCase) &&
                rule.ViolatesBooleanCriteria(Site != null))
            {
                return false;
            }

            if ((rule.PropertyName.Equals(nameof(Type), StringComparison.InvariantCultureIgnoreCase) ||
                 rule.PropertyName.Equals("ArtifactType", StringComparison.InvariantCultureIgnoreCase)) &&
                !string.IsNullOrWhiteSpace(rule.Value))
            {
                if (rule.Operator == FilterOperator.Equals &&
                    !Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    !Subtype.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (rule.Operator == FilterOperator.NotEquals &&
                    (Type.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase) ||
                     Subtype.Equals(rule.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            if (rule.PropertyName.Equals(nameof(PageCount), StringComparison.InvariantCultureIgnoreCase) &&
                int.TryParse(rule.Value, out int pageCount) &&
                rule.ViolatesIntegerCriteria(PageCount, pageCount))
            {
                return false;
            }
        }

        return true;
    }
}

