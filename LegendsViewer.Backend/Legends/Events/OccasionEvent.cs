using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class OccasionEvent : WorldEvent
{
    public Entity? Civ { get; set; }
    public Site? Site { get; set; }
    public WorldRegion? Region { get; set; }
    public UndergroundRegion? UndergroundRegion { get; set; }
    public int OccasionId { get; set; }
    public int ScheduleId { get; set; }
    public OccasionType OccasionType { get; set; }
    public EntityOccasion? EntityOccasion { get; set; }
    public EntityOccasionSchedule? Schedule { get; set; }
    public ArtForm? ReferencedArtForm { get; set; }

    public OccasionEvent(List<Property> properties, World world) : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "civ_id":
                    Civ = world.GetEntity(Convert.ToInt32(property.Value));
                    break;
                case "site_id":
                    Site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "subregion_id":
                    Region = world.GetRegion(Convert.ToInt32(property.Value));
                    break;
                case "feature_layer_id":
                    UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value));
                    break;
                case "occasion_id":
                    OccasionId = Convert.ToInt32(property.Value);
                    break;
                case "schedule_id":
                    ScheduleId = Convert.ToInt32(property.Value);
                    break;
            }
        }

        if (Civ != null)
        {
            Civ.EntityType = EntityType.Civilization;
            Civ.IsCiv = true;
        }
        if (Civ?.Occassions.Count > 0 == true)
        {
            EntityOccasion = Civ.Occassions.ElementAt(OccasionId);
            if (EntityOccasion != null)
            {
                Schedule = EntityOccasion.Schedules.ElementAt(ScheduleId);

                // DEBUG

                //if (Schedule.Reference != -1 && Schedule.Type == ScheduleType.Storytelling)
                //{
                //    WorldEvent worldEvent = World.GetEvent(Schedule.Reference) as WorldEvent;
                //    if (!(worldEvent is IFeatured))
                //    {
                //        world.ParsingErrors.Report("Unknown Occasion Feature - worldEvent.Type: " + worldEvent.Type);
                //    }
                //}
            }
        }

        Civ?.AddEvent(this);
        Site?.AddEvent(this);
        Region?.AddEvent(this);
        UndergroundRegion?.AddEvent(this);
    }

    public void ResolveArtForm()
    {
        if (Schedule != null)
        {
            switch (Schedule.ScheduleType)
            {
                case ScheduleType.PoetryRecital:
                    if (Schedule.Reference != -1)
                    {
                        ReferencedArtForm = World?.GetPoeticForm(Schedule.Reference);
                    }
                    break;
                case ScheduleType.MusicalPerformance:
                    if (Schedule.Reference != -1)
                    {
                        ReferencedArtForm = World?.GetMusicalForm(Schedule.Reference);
                    }
                    break;
                case ScheduleType.DancePerformance:
                    if (Schedule.Reference != -1)
                    {
                        ReferencedArtForm = World?.GetDanceForm(Schedule.Reference);
                    }
                    break;
            }
            ReferencedArtForm?.AddEvent(this);
        }
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        eventString += Civ != null ? Civ.ToLink(link, pov, this) : "UNKNOWN CIV";
        eventString += " held a ";
        if (Schedule != null)
        {
            if (!string.IsNullOrWhiteSpace(Schedule.ItemType) || !string.IsNullOrWhiteSpace(Schedule.ItemSubType))
            {
                eventString += !string.IsNullOrWhiteSpace(Schedule.ItemSubType) ? Schedule.ItemSubType : Schedule.ItemType;
                eventString += " ";
            }
        }
        eventString += Schedule?.ScheduleType.GetDescription().ToLowerInvariant() ?? OccasionType.GetDescription().ToLowerInvariant();
        if (ReferencedArtForm != null)
        {
            eventString += " of ";
            eventString += ReferencedArtForm.ToLink(link, pov, this);
        }
        else if (Schedule?.ScheduleType == ScheduleType.Storytelling && Schedule.Reference != -1)
        {
            WorldEvent? worldEvent = World?.GetEvent(Schedule.Reference);
            if (worldEvent is IFeatured featured)
            {
                eventString += " of ";
                eventString += featured.PrintFeature();
            }
        }
        eventString += " in ";
        eventString += Site != null ? Site.ToLink(link, pov, this) : "UNKNOWN SITE";
        eventString += " as part of ";
        eventString += EntityOccasion != null ? EntityOccasion.Name : "UNKNOWN OCCASION";
        eventString += ".";
        if (Schedule != null)
        {
            switch (Schedule.ScheduleType)
            {
                case ScheduleType.Procession:
                    Structure? startStructure = Site?.Structures.Find(s => s.LocalId == Schedule.Reference);
                    Structure? endStructure = Site?.Structures.Find(s => s.LocalId == Schedule.Reference2);
                    if (startStructure != null || endStructure != null)
                    {
                        eventString += " It started at ";
                        eventString += startStructure != null ? startStructure.ToLink(link, pov, this) : "UNKNOWN STRUCTURE";
                        eventString += " and ended at ";
                        eventString += endStructure != null ? endStructure.ToLink(link, pov, this) : "UNKNOWN STRUCTURE";
                        eventString += ".";
                    }
                    break;
            }
        }
        return eventString;
    }
}