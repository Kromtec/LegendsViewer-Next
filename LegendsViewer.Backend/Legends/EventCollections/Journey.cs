﻿using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Parser;

namespace LegendsViewer.Backend.Legends.EventCollections;

public class Journey : EventCollection
{
    public string Ordinal;
    public Journey(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "ordinal": Ordinal = string.Intern(property.Value); break;
            }
        }
    }
    public override string ToLink(bool link = true, DwarfObject pov = null, WorldEvent worldEvent = null)
    {
        return "a journey";
    }
}
