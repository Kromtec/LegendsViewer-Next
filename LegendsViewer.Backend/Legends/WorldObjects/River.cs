﻿using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class River : WorldObject, IHasCoordinates
{
    public string Name { get; set; } // legends_plus.xml
    public Location EndPos { get; set; } // legends_plus.xml
    public string Path { get; set; } // legends_plus.xml
    public List<Location> Coordinates { get; set; } // legends_plus.xml

    public string Icon = "<i class=\"fa fa-fw fa-tint\"></i>";

    public River(List<Property> properties, World world)
        : base(properties, world)
    {
        Name = "Untitled";
        Coordinates = [];

        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "path":
                    Path = property.Value;
                    string[] coordinateStrings = property.Value.Split(new[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (var coordinateString in coordinateStrings)
                    {
                        string[] xYCoordinates = coordinateString.Split(',');
                        int x = Convert.ToInt32(xYCoordinates[0]);
                        int y = Convert.ToInt32(xYCoordinates[1]);
                        Coordinates.Add(new Location(x, y));
                    }
                    break;
                case "end_pos":
                    string[] endCoordinates = property.Value.Split(',');
                    int endX = Convert.ToInt32(endCoordinates[0]);
                    int endY = Convert.ToInt32(endCoordinates[1]);
                    EndPos = new Location(endX, endY);
                    Coordinates.Add(EndPos);
                    break;
            }
        }
        if (Id == -1)
        {
            Id = world.Rivers.Count;
        }
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject pov = null, WorldEvent worldEvent = null)
    {
        if (link)
        {
            string linkedString = "";
            if (pov != this)
            {
                string title = "";
                title += "River";
                title += "&#13";
                title += "Events: " + Events.Count;

                linkedString = Icon + "<a href = \"river#" + Id + "\" title=\"" + title + "\">" + Name + "</a>";
            }
            else
            {
                linkedString = Icon + HtmlStyleUtil.CurrentDwarfObject(Name);
            }

            return linkedString;
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
