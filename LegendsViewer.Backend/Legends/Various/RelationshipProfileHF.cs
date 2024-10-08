﻿using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Parser;

namespace LegendsViewer.Backend.Legends.Various;

public class RelationshipProfileHf
{
    public int MeetCount { get; set; }
    public int LastMeetYear { get; set; }
    public int LastMeetSeconds72 { get; set; }
    public int Id { get; set; }
    public int HistoricalFigureId { get; set; }
    public int KnownIdentityId { get; set; } // TODO find the purpose of this property
    public List<Reputation> Reputations { get; set; }
    public RelationShipProfileType Type { get; set; }

    public RelationshipProfileHf(List<Property> properties, RelationShipProfileType type)
    {
        Type = type;
        Reputations = [];
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "meet_count": MeetCount = Convert.ToInt32(property.Value); break;
                case "last_meet_year": LastMeetYear = Convert.ToInt32(property.Value); break;
                case "last_meet_seconds72": LastMeetSeconds72 = Convert.ToInt32(property.Value); break;
                case "id": Id = Convert.ToInt32(property.Value); break;
                case "hf_id": HistoricalFigureId = Convert.ToInt32(property.Value); break;
                case "known_identity_id": KnownIdentityId = Convert.ToInt32(property.Value); break;
                default:
                    Reputations.Add(new Reputation(property));
                    break;
            }
        }
    }
}
