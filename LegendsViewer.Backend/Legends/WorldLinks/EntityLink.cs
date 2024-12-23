using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldLinks;

public class EntityLink
{
    [JsonIgnore]
    public Entity? Entity { get; set; }
    public string? EntityToLink => Entity?.ToLink(true);

    public EntityLinkType Type { get; set; }
    public int Strength { get; set; }
    public int PositionId { get; set; } = -1;
    public int StartYear { get; set; } = -1;
    public int EndYear { get; set; } = -1;

    public EntityLink(List<Property> properties, World world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "entity_id":
                    int id = Convert.ToInt32(property.Value);
                    Entity = world.GetEntity(id);
                    break;
                case "position_profile_id": PositionId = Convert.ToInt32(property.Value); break;
                case "start_year":
                    StartYear = Convert.ToInt32(property.Value);
                    Type = EntityLinkType.Position;
                    break;
                case "end_year":
                    EndYear = Convert.ToInt32(property.Value);
                    Type = EntityLinkType.FormerPosition;
                    break;
                case "link_strength": Strength = Convert.ToInt32(property.Value); break;
                case "link_type":
                    EntityLinkType linkType;
                    if (!Enum.TryParse(Formatting.InitCaps(property.Value), out linkType))
                    {
                        switch (property.Value)
                        {
                            case "former member": Type = EntityLinkType.FormerMember; break;
                            case "former prisoner": Type = EntityLinkType.FormerPrisoner; break;
                            case "former slave": Type = EntityLinkType.FormerSlave; break;
                            default:
                                Type = EntityLinkType.Unknown;
                                world.ParsingErrors.Report("Unknown Entity Link Type: " + property.Value);
                                break;
                        }
                    }
                    else
                    {
                        Type = linkType;
                    }
                    break;
            }
        }
    }
}