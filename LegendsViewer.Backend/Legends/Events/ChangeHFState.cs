using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;
using System.Text;

namespace LegendsViewer.Backend.Legends.Events;

public class ChangeHfState : WorldEvent
{
    public HistoricalFigure? HistoricalFigure { get; set; }
    public Site? Site { get; set; }
    public WorldRegion? Region { get; set; }
    public UndergroundRegion? UndergroundRegion { get; set; }
    public Location? Coordinates { get; set; }
    public HfState State { get; set; }
    public int SubState { get; set; }
    public Mood Mood { get; set; }
    public ChangeHfStateReason Reason { get; set; }

    public ChangeHfState(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "state":
                    switch (property.Value)
                    {
                        case "settled": State = HfState.Settled; break;
                        case "wandering": State = HfState.Wandering; break;
                        case "scouting": State = HfState.Scouting; break;
                        case "snatcher": State = HfState.Snatcher; break;
                        case "refugee": State = HfState.Refugee; break;
                        case "thief": State = HfState.Thief; break;
                        case "hunting": State = HfState.Hunting; break;
                        case "visiting": State = HfState.Visiting; break;
                        default: State = HfState.Unknown; property.Known = false; break;
                    }
                    break;
                case "substate": SubState = Convert.ToInt32(property.Value); break;
                case "coords": Coordinates = Formatting.ConvertToLocation(property.Value, world); break;
                case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                case "site": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "mood":
                    switch (property.Value)
                    {
                        case "macabre":
                            Mood = Mood.Macabre;
                            break;
                        case "secretive":
                            Mood = Mood.Secretive;
                            break;
                        case "insane":
                            Mood = Mood.Insane;
                            break;
                        case "possessed":
                            Mood = Mood.Possessed;
                            break;
                        case "berserk":
                            Mood = Mood.Berserk;
                            break;
                        case "fey":
                            Mood = Mood.Fey;
                            break;
                        case "melancholy":
                            Mood = Mood.Melancholy;
                            break;
                        case "fell":
                            Mood = Mood.Fell;
                            break;
                        case "catatonic":
                            Mood = Mood.Catatonic;
                            break;
                        default:
                            Mood = Mood.Unknown;
                            property.Known = false;
                            break;
                    }
                    break;
                case "reason":
                    switch (property.Value)
                    {
                        case "failed mood":
                            Reason = ChangeHfStateReason.FailedMood;
                            break;
                        case "gather information":
                            Reason = ChangeHfStateReason.GatherInformation;
                            break;
                        case "be with master":
                            Reason = ChangeHfStateReason.BeWithMaster;
                            break;
                        case "flight":
                            Reason = ChangeHfStateReason.Flight;
                            break;
                        case "scholarship":
                            Reason = ChangeHfStateReason.Scholarship;
                            break;
                        case "on a pilgrimage":
                            Reason = ChangeHfStateReason.Pilgrimage;
                            break;
                        case "lack of sleep":
                            Reason = ChangeHfStateReason.LackOfSleep;
                            break;
                        case "great deal of stress":
                            Reason = ChangeHfStateReason.GreatDealOfStress;
                            break;
                        case "exiled after conviction":
                            Reason = ChangeHfStateReason.ExiledAfterConviction;
                            break;
                        default:
                            if (property.Value != "-1" && property.Value != "none")
                            {
                                property.Known = false;
                            }
                            break;
                    }
                    break;
            }
        }
        if (HistoricalFigure != null)
        {
            HistoricalFigure.AddEvent(this);
            HistoricalFigure.State? lastState = HistoricalFigure.States.LastOrDefault();
            if (lastState != null)
            {
                lastState.EndYear = Year;
            }
            HistoricalFigure.States.Add(new HistoricalFigure.State(State, Year));
            HistoricalFigure.CurrentState = State;
        }
        Site?.AddEvent(this);
        Region?.AddEvent(this);
        UndergroundRegion?.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        StringBuilder eventString = new StringBuilder(GetYearTime());

        string figure = HistoricalFigure?.ToLink(link, pov, this) ?? "UNKNOWN HISTORICAL FIGURE";
        eventString.Append(figure).Append(' ');

        // Determine the main action based on State and Mood
        if (State == HfState.Visiting)
        {
            eventString.Append("visited ");
        }
        else if (State == HfState.Settled)
        {
            eventString.Append(SubState switch
            {
                45 => "fled to ",
                46 or 47 => "moved to study in ",
                _ => "settled in "
            });
        }
        else if (State == HfState.Wandering)
        {
            eventString.Append("began wandering ");
        }
        else if (State is HfState.Refugee or HfState.Snatcher or HfState.Thief)
        {
            eventString.Append($"became a {State.ToString().ToLower()} in ");
        }
        else if (State == HfState.Scouting)
        {
            eventString.Append("began scouting the area around ");
        }
        else if (State == HfState.Hunting)
        {
            eventString.Append("began hunting great beasts in ");
        }
        else if (Mood != Mood.Unknown)
        {
            eventString.Append(Mood switch
            {
                Mood.Macabre => "began to skulk and brood in ",
                Mood.Secretive => "withdrew from society in ",
                Mood.Insane => "became crazed in ",
                Mood.Possessed => "was possessed in ",
                Mood.Berserk => "went berserk in ",
                Mood.Fey => "was taken by a fey mood in ",
                Mood.Melancholy => "was stricken by melancholy in ",
                Mood.Fell => "was taken by a fell mood in ",
                Mood.Catatonic => "stopped responding to the outside world in ",
                _ => "changed state in "
            });
        }
        else
        {
            eventString.Append("changed state in ");
        }

        // Determine location
        eventString.Append(Site?.ToLink(link, pov, this) ??
                            Region?.ToLink(link, pov, this) ??
                            UndergroundRegion?.ToLink(link, pov, this) ??
                            "the wilds");

        // Determine reason
        if (Reason != ChangeHfStateReason.Unknown)
        {
            eventString.Append(Reason switch
            {
                ChangeHfStateReason.FailedMood => " after failing to create an artifact",
                ChangeHfStateReason.GatherInformation => " to gather information",
                ChangeHfStateReason.BeWithMaster => " in order to be with the master",
                ChangeHfStateReason.Flight => " in order to flee",
                ChangeHfStateReason.Scholarship => " in order to pursue scholarship",
                ChangeHfStateReason.Pilgrimage => " on a pilgrimage",
                ChangeHfStateReason.LackOfSleep => " due to lack of sleep",
                ChangeHfStateReason.GreatDealOfStress => " after a great deal of stress",
                ChangeHfStateReason.ExiledAfterConviction => " after being exiled following a criminal conviction",
                _ => ""
            });
        }

        eventString.Append(PrintParentCollection(link, pov)).Append('.');
        return eventString.ToString();
    }
}