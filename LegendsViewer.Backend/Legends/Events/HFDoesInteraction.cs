using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class HfDoesInteraction : WorldEvent
{
    public HistoricalFigure Doer { get; set; }
    public HistoricalFigure Target { get; set; }
    public string Interaction { get; set; }
    public string InteractionAction { get; set; }
    public string InteractionString { get; set; }
    public string Source { get; set; }
    public WorldRegion Region { get; set; }
    public Site Site { get; set; }

    public HfDoesInteraction(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "doer_hfid": Doer = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "target_hfid": Target = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "interaction": Interaction = property.Value; break;
                case "doer": if (Doer == null) { Doer = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "target": if (Target == null) { Target = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "interaction_action": InteractionAction = property.Value.Replace("[IS_HIST_STRING_1:", "").Replace("[IS_HIST_STRING_2:", "").Replace("]", ""); break;
                case "interaction_string": InteractionString = property.Value.Replace("[IS_HIST_STRING_2:", "").Replace("[I_TARGET:A:CREATURE", "").Replace("]", ""); break;
                case "source": Source = property.Value; break;
                case "region": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                case "site": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
            }
        }

        if (Target != null)
        {
            string creatureType = "";
            if (!string.IsNullOrWhiteSpace(Interaction))
            {
                if (!Target.ActiveInteractions.Contains(Interaction))
                {
                    Target.ActiveInteractions.Add(Interaction);
                }
                if (Doer != null)
                {
                    Doer.LineageCurseChilds.Add(Target);
                    Target.LineageCurseParent = Doer;
                }

                if (Interaction.Contains("VAMPIRE"))
                {
                    creatureType = "vampire";
                }
                if (Interaction.Contains("WEREBEAST"))
                {
                    creatureType = "werebeast";
                }
                if (Interaction.Contains("SECRET"))
                {
                    creatureType = "necromancer";
                }
                if (Interaction.Contains("ANIMATE"))
                {
                    creatureType = "animated corpse";
                }
                if (Interaction.Contains("UNDEAD_RES"))
                {
                    creatureType = "resurrected undead";
                }
            }
            if (!string.IsNullOrWhiteSpace(InteractionAction) && InteractionAction.Contains(", passing on the "))
            {
                Target.Interaction = InteractionAction.Replace(", passing on the ", "");
                if (!string.IsNullOrEmpty(Target.Interaction))
                {
                    creatureType = "were" + Target.Interaction.Replace(" monster curse", " ");
                }
            }
            else if (!string.IsNullOrWhiteSpace(InteractionString) && InteractionString.Contains(" to assume the form of a "))
            {
                Target.Interaction = InteractionString.Replace(" to assume the form of a ", "").Replace("-like", "").Replace(" every full moon", " curse");
                if (!string.IsNullOrEmpty(Target.Interaction))
                {
                    creatureType = "were" + Target.Interaction.Replace(" monster curse", " ");
                }
            }

            if (!string.IsNullOrEmpty(creatureType))
            {
                Target.CreatureTypes.Add(new HistoricalFigure.CreatureType(creatureType, this));
            }
        }
        Doer.AddEvent(this);
        Target.AddEvent(this);
        Region.AddEvent(this);
        Site.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject pov = null)
    {
        string eventString = GetYearTime();
        eventString += Doer.ToLink(link, pov, this);
        if (InteractionString?.Length == 0)
        {
            eventString += " bit ";
            eventString += Target.ToLink(link, pov, this);
            eventString += !string.IsNullOrWhiteSpace(InteractionAction) ? InteractionAction : ", passing on the " + Interaction + " ";
        }
        else
        {
            eventString += !string.IsNullOrWhiteSpace(InteractionAction) ? InteractionAction : " put " + Interaction + " on ";
            eventString += Target.ToLink(link, pov, this);
            eventString += !string.IsNullOrWhiteSpace(InteractionString) ? InteractionString : "";
        }
        eventString += " in ";
        eventString += Site != null ? Site.ToLink(link, pov, this) : "UNKNOWN SITE";
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}