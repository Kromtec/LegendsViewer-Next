using LegendsViewer.Backend.Legends.Events;

namespace LegendsViewer.Backend.Extensions;

public static class WorldEventExtensions
{
    private static readonly Dictionary<string, string> eventTypeDictionary = new()
    {
        { "add hf entity link", "Historical Figure linked to Entity" },
        { "add hf hf link", "Historical Figures linked" },
        { "attacked site", "Site Attacked" },
        { "body abused", "Historical Figure Body Abused" },
        { "change hf job", "Historical Figure Change Job" },
        { "change hf state", "Historical Figure Change State" },
        { "changed creature type", "Historical Figure Transformed" },
        { "create entity position", "Entity Position Created" },
        { "created site", "Site Founded" },
        { "created world construction", "Entity Construction Created" },
        { "creature devoured", "Historical Figure Eaten" },
        { "destroyed site", "Site Destroyed" },
        { "field battle", "Entity Battle" },
        { "hf abducted", "Historical Figure Abduction" },
        { "hf died", "Historical Figure Death" },
        { "hf new pet", "Historical Figure Tamed Creatures" },
        { "hf reunion", "Historical Figure Reunion" },
        { "hf simple battle event", "Historical Figure Fight" },
        { "hf travel", "Historical Figure Travel" },
        { "hf wounded", "Historical Figure Wounded" },
        { "impersonate hf", "Historical Figure Impersonation" },
        { "item stolen", "Historical Figure Theft" },
        { "new site leader", "Site Taken Over / New Leader" },
        { "peace accepted", "Entity Accepted Peace" },
        { "peace rejected", "Entity Rejected Peace" },
        { "plundered site", "Site Pillaged" },
        { "reclaim site", "Site Reclaimed" },
        { "remove hf entity link", "Historical Figure detached from Entity" },
        { "artifact created", "Artifact Created" },
        { "artifact destroyed", "Artifact Destroyed" },
        { "diplomat lost", "Diplomat Lost" },
        { "entity created", "Entity Created" },
        { "hf revived", "Historical Figure Became Ghost" },
        { "masterpiece arch design", "Masterpiece Arch. Designed" },
        { "masterpiece arch constructed", "Masterpiece Arch. Constructed" },
        { "masterpiece engraving", "Masterpiece Engraving" },
        { "masterpiece food", "Masterpiece Food Cooked" },
        { "masterpiece dye", "Masterpiece Dye Made" },
        { "masterpiece item", "Masterpiece Item Made" },
        { "masterpiece item improvement", "Masterpiece Item Improvement" },
        { "masterpiece lost", "Masterpiece Item Lost" },
        { "merchant", "Merchants Arrived" },
        { "first contact", "First Contact" },
        { "site abandoned", "Site Abandoned" },
        { "site died", "Site Withered" },
        { "site retired", "Site Retired" },
        { "add hf site link", "Historical Figure linked to Site" },
        { "created structure", "Site Structure Created" },
        { "hf razed structure", "Site Structure Razed" },
        { "remove hf site link", "Historical Figure detached from Site" },
        { "replaced structure", "Site Structure Replaced" },
        { "site taken over", "Site Taken Over" },
        { "entity relocate", "Entity Relocated" },
        { "hf gains secret goal", "Historical Figure Gained Secret Goal" },
        { "hf profaned structure", "Historical Figure Profaned structure" },
        { "hf disturbed structure", "Historical Figure Disturbed structure" },
        { "hf does interaction", "Historical Figure Did Interaction" },
        { "entity primary criminals", "Entity Became Primary Criminals" },
        { "hf confronted", "Historical Figure Confronted" },
        { "assume identity", "Historical Figure Assumed Identity" },
        { "entity law", "Entity Law Change" },
        { "change hf body state", "Historical Figure Body State Changed" },
        { "razed structure", "Entity Razed Structure" },
        { "hf learns secret", "Historical Figure Learned Secret" },
        { "artifact stored", "Artifact Stored" },
        { "artifact possessed", "Artifact Possessed" },
        { "artifact transformed", "Artifact Transformed" },
        { "agreement made", "Entity Agreement Made" },
        { "agreement rejected", "Entity Agreement Rejected" },
        { "artifact lost", "Artifact Lost" },
        { "site dispute", "Site Dispute" },
        { "hf attacked site", "Historical Figure Attacked Site" },
        { "hf destroyed site", "Historical Figure Destroyed Site" },
        { "agreement formed", "Agreement Formed" },
        { "agreement concluded", "Agreement Concluded" },
        { "site tribute forced", "Site Tribute Forced" },
        { "insurrection started", "Insurrection Started" },
        { "hf reach summit", "Historical Figure Reach Summit" },

        // new 0.42.XX events
        { "procession", "Procession" },
        { "ceremony", "Ceremony" },
        { "performance", "Performance" },
        { "competition", "Competition" },
        { "written content composed", "Written Content Composed" },
        { "knowledge discovered", "Knowledge Discovered" },
        { "hf relationship denied", "Historical Figure Relationship Denied" },
        { "poetic form created", "Poetic Form Created" },
        { "musical form created", "Musical Form Created" },
        { "dance form created", "Dance Form Created" },
        { "regionpop incorporated into entity", "Regionpop Incorporated Into Entity" },

        // new 0.44.XX events
        { "hfs formed reputation relationship", "Reputation Relationship Formed" },
        { "hf recruited unit type for entity", "Recruited Unit Type For Entity" },
        { "hf prayed inside structure", "Historical Figure Prayed In Structure" },
        { "hf viewed artifact", "Historical Figure Viewed Artifact" },
        { "artifact given", "Artifact Given" },
        { "artifact claim formed", "Artifact Claim Formed" },
        { "artifact copied", "Artifact Copied" },
        { "artifact recovered", "Artifact Recovered" },
        { "artifact found", "Artifact Found" },
        { "sneak into site", "Sneak Into Site" },
        { "spotted leaving site", "Spotted Leaving Site" },
        { "entity searched site", "Entity Searched Site" },
        { "hf freed", "Historical Figure Freed" },
        { "tactical situation", "Tactical Situation" },
        { "squad vs squad", "Squad vs. Squad" },
        { "agreement void", "Agreement Void" },
        { "entity rampaged in site", "Entity Rampaged In Site" },
        { "entity fled site", "Entity Fled Site" },
        { "entity expels hf", "Entity Expels Historical Figure" },
        { "site surrendered", "Site Surrendered" },

        // new 0.47.XX events
        { "remove hf hf link", "Historical Figures Unlinked" },
        { "holy city declaration", "Holy City Declaration" },
        { "hf performed horrible experiments", "Historical Figure Performed Horrible Experiments" },
        { "entity incorporated", "Entity Incorporated" },
        { "gamble", "Gamble" },
        { "trade", "Trade" },
        { "hf equipment purchase", "Historical Figure Equipment Purchase" },
        { "entity overthrown", "Entity Overthrown" },
        { "failed frame attempt", "Failed Frame Attempt" },
        { "hf convicted", "Historical Figure Convicted" },
        { "failed intrigue corruption", "Failed Intrigue Corruption"},
        { "hfs formed intrigue relationship", "Historical Figures formed Intrigue Relationship"},
        { "entity alliance formed", "Entities formed Alliance" },
        { "entity dissolved", "Entity Dissolved" },
        { "add hf entity honor", "Add Historical Figure Honor" },
        { "entity breach feature layer", "Entity Breach Caverns" },
        { "entity equipment purchase", "Entity Equipment Purchase" },
        { "hf ransomed", "Historical Figure Ransomed" },
        { "hf preach", "Historical Figure Preach" },
        { "modified building", "Modified Building" },
        { "hf interrogated", "Historical Figure Interrogated" },
        { "entity persecuted", "Entity Persecuted" },
        { "building profile acquired", "Building Profile Acquired" },
        { "hf enslaved", "Historical Figure Enslaved" },
        { "hf asked about artifact", "Historical Figure Asked About Artifact" },
        { "hf carouse", "Historical Figure Carouse" },
        { "sabotage", "Sabotage" },

        // incidental events from event collections
        { "battle fought", "Historical Figure Fought In Battle" },

        // xml plus only events
        { "historical event relationship", "Historical Event Relationships" }
    };

    public static string GetEventInfo(this WorldEvent worldEvent)
    {
        if (eventTypeDictionary.TryGetValue(worldEvent.Type, out var eventInfo))
        {
            return eventInfo;
        }
        return $"UNKNOWN EVENT ({worldEvent.Type})";
    }

    public static string GetEventInfo(string eventType)
    {
        if (eventTypeDictionary.TryGetValue(eventType, out var eventInfo))
        {
            return eventInfo;
        }
        return $"UNKNOWN EVENT ({eventType})";
    }
}
