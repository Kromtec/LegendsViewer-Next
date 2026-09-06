using LegendsViewer.Backend.Legends.Interfaces;
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
    public bool AsAttacker { get; }
    public bool WasHired { get; }
    public bool AsScout { get; }


    public BattleFought(HistoricalFigure hf, Battle battle, IWorld? world, bool asAttacker, bool wasHired = false, bool asScout = false) : base([], world)
    {
        // Synthetic events must get an Id strictly greater than any existing event Id so that
        // World.Events stays sorted and collision-free. World.GetEvent relies on a binary search
        // (with a dense-index shortcut) that breaks on duplicate or out-of-order Ids.
        // Using Events.Count fails whenever the XML contained unsupported event types that were
        // skipped while parsing: Count then drops below the highest real Id, so synthetic Ids
        // collide with real ones and lookups resolve to the wrong event. This is what caused late
        // battles to display "battle fought" events belonging to unrelated, much older battles
        // (issue #47). Appending with "last Id + 1" keeps the invariant intact in O(1).
        Id = world == null
            ? -1
            : world.Events.Count > 0 ? world.Events[^1].Id + 1 : 0;
        Type = "battle fought";
        Year = battle.StartYear;
        Seconds72 = battle.StartSeconds72;

        HistoricalFigure = hf;
        Battle = battle;
        AsAttacker = asAttacker;
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
            if (AsAttacker)
            {
                eventString.Append($", an assault on {Site.ToLink(link, pov, this)}");
            }
            else
            {
                eventString.Append($" in defense of {Site.ToLink(link, pov, this)}");
            }
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


