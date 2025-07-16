using LegendsViewer.Backend.Legends.Cytoscape;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.WorldLinks;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;
using System.Net;

namespace LegendsViewer.Backend.Extensions;

public static class HistoricalFigureExtensions
{
    public static CytoscapeData CreateFamilyTreeElements(this HistoricalFigure historicalFigure)
    {
        List<CytoscapeNodeElement> nodes = [CreateFamilyTreeNodeData(historicalFigure, historicalFigure)];
        List<CytoscapeEdgeElement> edges = [];
        int mothertreesize = 0;
        int fathertreesize = 0;
        GetFamilyDataParents(historicalFigure, historicalFigure, nodes, edges, ref mothertreesize, ref fathertreesize);
        GetFamilyDataChildren(historicalFigure, historicalFigure, nodes, edges);
        var familyTreeData = new CytoscapeData();
        familyTreeData.Nodes.AddRange(nodes);
        familyTreeData.Edges.AddRange(edges);
        return familyTreeData;
    }

    private static void GetFamilyDataChildren(HistoricalFigure original, HistoricalFigure current, List<CytoscapeNodeElement> nodes, List<CytoscapeEdgeElement> edges)
    {
        foreach (HistoricalFigure? child in current.RelatedHistoricalFigures.Where(rel => rel.Type == HistoricalFigureLinkType.Child).Select(rel => rel.HistoricalFigure))
        {
            if (child == null)
            {
                continue;
            }

            CytoscapeNodeElement node = CreateFamilyTreeNodeData(original, child);
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
            }
            CytoscapeEdgeElement edge = new(new CytoscapeEdgeData
            {
                Source = current.Id.ToString(),
                Target = child.Id.ToString(),
            });
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }
        }
    }

    private static void GetFamilyDataParents(HistoricalFigure original, HistoricalFigure current, List<CytoscapeNodeElement> nodes, List<CytoscapeEdgeElement> edges, ref int mothertreesize, ref int fathertreesize)
    {
        foreach (HistoricalFigure? mother in current.RelatedHistoricalFigures.Where(rel => rel.Type == HistoricalFigureLinkType.Mother).Select(rel => rel.HistoricalFigure))
        {
            if (mother == null)
            {
                continue;
            }
            mothertreesize++;
            CytoscapeNodeElement node = CreateFamilyTreeNodeData(original, mother);
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
            }
            CytoscapeEdgeElement edge = new(new CytoscapeEdgeData
            {
                Source = mother.Id.ToString(),
                Target = current.Id.ToString(),
            });
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }
            if (mothertreesize < 3)
            {
                GetFamilyDataParents(original, mother, nodes, edges, ref mothertreesize, ref fathertreesize);
            }
            mothertreesize--;
        }
        foreach (HistoricalFigure? father in current.RelatedHistoricalFigures.Where(rel => rel.Type == HistoricalFigureLinkType.Father).Select(rel => rel.HistoricalFigure))
        {
            if (father == null)
            {
                continue;
            }
            fathertreesize++;
            CytoscapeNodeElement node = CreateFamilyTreeNodeData(original, father);
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
            }
            CytoscapeEdgeElement edge = new(new CytoscapeEdgeData
            {
                Source = father.Id.ToString(),
                Target = current.Id.ToString(),
            });
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
            }
            if (fathertreesize < 3)
            {
                GetFamilyDataParents(original, father, nodes, edges, ref mothertreesize, ref fathertreesize);
            }
            fathertreesize--;
        }
    }

    private static CytoscapeNodeElement CreateFamilyTreeNodeData(HistoricalFigure original, HistoricalFigure current)
    {
        List<string> classes = original.Equals(current) ? ["current"] : [];

        string title = "";
        title += current.Race != null && current.Race != original.Race ? current.Race.NameSingular + " " : "";

        string description = "";
        if (current.ActiveInteractions.Any(it => it.Contains("VAMPIRE")))
        {
            description += "\nVampire\n";
            classes.Add("vampire");
        }
        if (current.ActiveInteractions.Any(it => it.Contains("WEREBEAST")))
        {
            description += "\nWerebeast\n";
            classes.Add("werebeast");
        }
        if (current.ActiveInteractions.Any(it => it.Contains("SECRET") && !it.Contains("ANIMATE") && !it.Contains("UNDEAD_RES")))
        {
            description += "\nNecromancer\n";
            classes.Add("necromancer");
        }
        if (current.Ghost)
        {
            description += "\nGhost\n";
            classes.Add("ghost");
        }
        if (current.IsMainCivLeader)
        {
            classes.Add("leader");
        }
        if (current.Caste == "Male")
        {
            classes.Add("male");
        }
        else if (current.Caste == "Female")
        {
            classes.Add("female");
        }
        var hfAssignment = current.GetLastAssignmentString();
        var hfHighestSkill = Formatting.InitCaps(current.GetHighestSkillAsString());
        if (!string.IsNullOrWhiteSpace(hfAssignment) &&
            !string.Equals(hfAssignment, "Standard", StringComparison.OrdinalIgnoreCase) &&
            hfAssignment != title &&
            !hfHighestSkill.Contains(hfAssignment))
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                description += "\n";
            }
            description += hfAssignment;
        }
        if (!string.IsNullOrWhiteSpace(hfHighestSkill))
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                description += "\n──────── ✶ ────────\n\n";
            }
            description += hfHighestSkill;
        }
        if (!string.IsNullOrWhiteSpace(title))
        {
            title += "\n────────────\n";
        }
        if (!string.IsNullOrWhiteSpace(description))
        {
            description += "\n────────────\n";
        }
        title += description;
        title += current.Name;
        if (!current.Alive)
        {
            title += $"\n\n(Age: {current.Age}✝)";
            classes.Add("dead");
        }
        else
        {
            title += $"\n\nAge: {current.Age}";
        }

        CytoscapeNodeData nodaData = new()
        {
            Id = current.Id.ToString(),
            Label = WebUtility.HtmlEncode(title),
            Href = $"/hf/{current.Id}"
        };
        return new CytoscapeNodeElement(nodaData)
        {
            Classes = classes
        };
    }

    public static void AddWorshipper(this HistoricalFigure? worshipped, HistoricalFigure? worshipper, int strength)
    {
        if (worshipped == null || worshipper == null)
        {
            return;
        }
        worshipped.WorshippingFigures ??= [];
        var entry = (worshipper, strength);
        if (worshipped.WorshippingFigures.Contains(entry))
        {
            return;
        }
        worshipped.WorshippingFigures.Add(entry);
    }

    public static void AddWorshipper(this HistoricalFigure? worshipped, Entity? worshipper)
    {
        if (worshipped == null || worshipper == null)
        {
            return;
        }
        worshipped.WorshippingEntities ??= [];
        if (worshipped.WorshippingEntities.Contains(worshipper))
        {
            return;
        }
        worshipped.WorshippingEntities.Add(worshipper);
    }
}
