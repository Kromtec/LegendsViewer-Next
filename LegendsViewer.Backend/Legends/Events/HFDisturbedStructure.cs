﻿using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class HfDisturbedStructure : WorldEvent
{
    public string Action { get; set; } // legends_plus.xml TODO not used in Legends Mode
    public HistoricalFigure HistoricalFigure { get; set; }
    public Site Site { get; set; }
    public int StructureId { get; set; }
    public Structure Structure { get; set; }

    public HfDisturbedStructure(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "structure_id":
                case "structure": StructureId = Convert.ToInt32(property.Value); break;
                case "histfig": if (HistoricalFigure == null) { HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "site": if (Site == null) { Site = world.GetSite(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "action":
                    Action = property.Value;
                    break;
            }
        }
        if (Site != null)
        {
            Structure = Site.Structures.Find(structure => structure.Id == StructureId);
        }
        HistoricalFigure.AddEvent(this);
        Site.AddEvent(this);
        Structure.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject pov = null)
    {
        string eventString = GetYearTime() + HistoricalFigure.ToLink(link, pov, this) + " disturbed ";
        eventString += Structure != null ? Structure.ToLink(link, pov, this) : "UNKNOWN STRUCTURE";
        eventString += " in " + Site.ToLink(link, pov, this);
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}