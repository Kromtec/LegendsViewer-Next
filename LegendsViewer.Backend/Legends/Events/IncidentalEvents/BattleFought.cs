using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.WorldObjects;
using System.Text;

namespace LegendsViewer.Backend.Legends.Events.IncidentalEvents;

public class BattleFought : WorldEvent
{
    public Site? Site { get; set; }
    public WorldRegion? Region { get; set; }
    public UndergroundRegion? UndergroundRegion { get; set; }
    public HistoricalFigure? HistoricalFigure { get; set; }
    public Battle? Battle { get; }
    public bool WasHired { get; }
    public bool AsScout { get; }

    public BattleFought(HistoricalFigure hf, Battle battle, World? world, bool wasHired = false, bool asScout = false) : base([], world)
    {
        Id = world?.Events.Count ?? -1;
        Type = "battle fought";
        Year = battle.StartYear;
        Seconds72 = battle.StartSeconds72;

        HistoricalFigure = hf;
        Battle = battle;
        WasHired = wasHired;
        AsScout = asScout;
        Site = battle.Site;
        Region = battle.Region;
        UndergroundRegion = battle.UndergroundRegion;
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        StringBuilder eventString = new StringBuilder(GetYearTime());

        string figure = HistoricalFigure?.ToLink(link, pov, this) ?? "UNKNOWN HISTORICAL FIGURE";
        string battle = Battle?.ToLink(link, pov, this) ?? "UNKNOWN BATTLE";

        eventString.Append($"{figure} ");

        if (WasHired)
        {
            eventString.Append("was hired");
            if (AsScout)
            {
                eventString.Append(" as a scout");
            }
            eventString.Append(" to fight in ");
        }
        else
        {
            eventString.Append("fought in ");
        }

        eventString.Append(battle);

        if (Site != null)
        {
            eventString.Append($" an assault on {Site.ToLink(link, pov, this)}");
        }
        else if (Region != null)
        {
            eventString.Append($" in {Region.ToLink(link, pov, this)}");
        }
        else if (UndergroundRegion != null)
        {
            eventString.Append($" in {UndergroundRegion.ToLink(link, pov, this)}");
        }

        eventString.Append('.');
        return eventString.ToString();
    }
}
