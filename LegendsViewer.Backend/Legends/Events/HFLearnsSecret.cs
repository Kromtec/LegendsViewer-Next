using System.Text;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class HfLearnsSecret : WorldEvent
{
    public HistoricalFigure? Student { get; set; }
    public HistoricalFigure? Teacher { get; set; }
    public Artifact? Artifact { get; set; }
    public string? Interaction { get; set; }
    public string? SecretText { get; set; }

    public HfLearnsSecret(List<Property> properties, IWorld world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "student_hfid": Student = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "teacher_hfid": Teacher = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); break;
                case "interaction": Interaction = property.Value; break;
                case "student": if (Student == null) { Student = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "teacher": if (Teacher == null) { Teacher = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "artifact": if (Artifact == null) { Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "secret_text": SecretText = property.Value.Replace("[IS_NAME:", "").Replace("]", ""); break;
                case "unk_1":
                case "interaction_effect":
                case "interaction_source":
                    if (property.Value != "-1")
                    {
                        property.Known = false;
                    }
                    break;
            }
        }

        Student?.AddEvent(this);
        Student?.CreatureTypes.Add(new HistoricalFigure.CreatureType("necromancer", this));
        Teacher?.AddEvent(this);
        Artifact?.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        var sb = new StringBuilder();
        sb.Append(GetYearTime());

        if (Teacher != null)
        {
            sb.Append(Teacher.ToLink(link, pov, this));
            sb.Append(" taught ");
            sb.Append(Student != null ? Student.ToLink(link, pov, this) : "UNKNOWN HISTORICAL FIGURE");
            sb.Append(" ");
            sb.Append(!string.IsNullOrWhiteSpace(SecretText) ? SecretText : "(" + Interaction + ")");
        }
        else
        {
            sb.Append(Student != null ? Student.ToLink(link, pov, this) : "UNKNOWN HISTORICAL FIGURE");
            sb.Append(" learned ");
            sb.Append(!string.IsNullOrWhiteSpace(SecretText) ? SecretText : "(" + Interaction + ")");
            sb.Append(" from ");
            sb.Append(Artifact != null ? Artifact.ToLink(link, pov, this) : "UNKNOWN ARTIFACT");
        }
        sb.Append(PrintParentCollection(link, pov));
        sb.Append(".");
        return sb.ToString();
    }
}
