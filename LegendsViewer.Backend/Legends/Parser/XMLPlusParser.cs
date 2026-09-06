using LegendsViewer.Backend.Legends.Interfaces;
using System.Xml;

namespace LegendsViewer.Backend.Legends.Parser;

public class XmlPlusParser : XmlParser
{
    private bool _inMiddleOfSection;
    private List<Property>? _currentItem;

    public XmlPlusParser(IWorld world, string xmlFile) : base(world, xmlFile)
    {
    }

    public override async Task ParseAsync()
    {
        if (XmlReader.ReadState == ReadState.Closed)
        {
            return;
        }

        while (!XmlReader.EOF && _currentItem == null)
        {
            if (!_inMiddleOfSection)
            {
                CurrentSection = GetSectionType(XmlReader.Name);
            }

            if (CurrentSection == Section.Junk)
            {
                await XmlReader.ReadAsync();
            }
            else if (CurrentSection == Section.Unknown)
            {
                await SkipSectionAsync();
            }
            else
            {
                await ParseSectionAsync();
            }
        }

        if (XmlReader.EOF)
        {
            XmlReader.Close();
        }
    }

    protected override async Task ParseSectionAsync()
    {
        while (XmlReader.NodeType == XmlNodeType.EndElement || XmlReader.NodeType == XmlNodeType.None)
        {
            if (XmlReader.NodeType == XmlNodeType.None)
            {
                return;
            }

            XmlReader.ReadEndElement();
        }

        if (!_inMiddleOfSection)
        {
            XmlReader.ReadStartElement();
            _inMiddleOfSection = true;
        }

        _currentItem = await ParseItemPropertiesAsync();

        if (XmlReader.NodeType == XmlNodeType.EndElement)
        {
            XmlReader.ReadEndElement();
            _inMiddleOfSection = false;
        }
    }

    public async Task AddNewPropertiesAsync(List<Property> existingProperties, Section xmlParserSection)
    {
        if (_currentItem == null)
        {
            return;
        }

        if (xmlParserSection > CurrentSection)
        {
            while (xmlParserSection > CurrentSection &&
                   (_currentItem != null || ReadState.Closed != XmlReader.ReadState))
            {
                if (_currentItem != null)
                {
                    AddItemToWorld(_currentItem);
                    _currentItem = null;
                }

                await ParseAsync();
            }
        }

        if (xmlParserSection < CurrentSection)
        {
            return;
        }

        if (_currentItem != null)
        {
            Property? id = GetPropertyByName(existingProperties, "id");
            Property? currentId = GetPropertyByName(_currentItem, "id");
            while (currentId?.ValueAsInt() < 0)
            {
                _currentItem = await ParseItemPropertiesAsync();
                if (_currentItem != null)
                {
                    currentId = GetPropertyByName(_currentItem, "id");
                }
            }
            if (id != null && currentId != null && id.ValueAsInt().Equals(currentId.ValueAsInt()))
            {
                if (_currentItem != null)
                {
                    foreach (var property in _currentItem)
                    {
                        if (CurrentSection == Section.Entities &&
                            (property.Name == "entity_link" || property.Name == "child" ||
                             property.Name == "entity_position" || property.Name == "entity_position_assignment" ||
                             property.Name == "occasion" || property.Name == "weapon" || property.Name == "histfig_id"))
                        {
                            existingProperties.Add(property);
                            continue;
                        }
                        if (CurrentSection == Section.Artifacts && property.Name == "writing")
                        {
                            existingProperties.Add(property);
                            continue;
                        }
                        if (CurrentSection == Section.WrittenContent && property.Name == "style")
                        {
                            existingProperties.Add(property);
                            continue;
                        }
                        if (CurrentSection == Section.Events && property.Name == "bodies")
                        {
                            existingProperties.Add(property);
                            continue;
                        }
                        Property? matchingProperty = GetPropertyByName(existingProperties, property.Name);
                        if (CurrentSection == Section.Events && matchingProperty != null &&
                            (matchingProperty.Name == "type" || matchingProperty.Name == "state" ||
                             matchingProperty.Name == "slayer_race" || matchingProperty.Name == "circumstance" ||
                             matchingProperty.Name == "reason"))
                        {
                            continue;
                        }

                        if (matchingProperty != null)
                        {
                            if (CurrentSection == Section.Sites && property.Name == "structures")
                            {
                                MergeStructures(matchingProperty, property);
                                continue;
                            }
                            matchingProperty.Value = property.Value;
                            matchingProperty.Known = false;
                            if (property.SubProperties != null)
                            {
                                if (matchingProperty.SubProperties == null)
                                {
                                    matchingProperty.SubProperties = property.SubProperties;
                                }
                                else
                                {
                                    matchingProperty.SubProperties.AddRange(property.SubProperties);
                                }
                            }
                        }
                        else
                        {
                            existingProperties.Add(property);
                        }
                    }
                }

                _currentItem = null;
                await ParseAsync();
            }
        }
    }

    private static void MergeStructures(Property matchingProperty, Property property)
    {
        if (matchingProperty.SubProperties == null)
        {
            matchingProperty.SubProperties = property.SubProperties;
            return;
        }
        if (property.SubProperties == null)
        {
            return;
        }

        foreach (Property plusStruct in property.SubProperties)
        {
            if (plusStruct.SubProperties == null)
            {
                continue;
            }

            Property? plusIdProp = GetPropertyByName(plusStruct.SubProperties, "id") ?? GetPropertyByName(plusStruct.SubProperties, "local_id");
            int plusId = -1;
            if (plusIdProp != null)
            {
                int.TryParse(plusIdProp.Value, out plusId);
            }

            Property? existingStruct = matchingProperty.SubProperties.FirstOrDefault(s =>
            {
                if (s.SubProperties == null) return false;
                Property? idProp = GetPropertyByName(s.SubProperties, "id") ?? GetPropertyByName(s.SubProperties, "local_id");
                return idProp != null && int.TryParse(idProp.Value, out int id) && id == plusId;
            });

            if (existingStruct != null && existingStruct.SubProperties != null)
            {
                foreach (Property plusProp in plusStruct.SubProperties)
                {
                    if (plusProp.Name == "inhabitant" || plusProp.Name == "copied_artifact_id")
                    {
                        existingStruct.SubProperties.Add(plusProp);
                    }
                    else
                    {
                        Property? existingProp = GetPropertyByName(existingStruct.SubProperties, plusProp.Name);
                        if (existingProp != null)
                        {
                            existingProp.Value = plusProp.Value;
                            existingProp.Known = false;
                            if (plusProp.SubProperties != null)
                            {
                                if (existingProp.SubProperties == null)
                                {
                                    existingProp.SubProperties = plusProp.SubProperties;
                                }
                                else
                                {
                                    existingProp.SubProperties.AddRange(plusProp.SubProperties);
                                }
                            }
                        }
                        else
                        {
                            existingStruct.SubProperties.Add(plusProp);
                        }
                    }
                }
            }
            else
            {
                matchingProperty.SubProperties.Add(plusStruct);
            }
        }
    }

    private static Property? GetPropertyByName(List<Property> existingProperties, string name)
    {
        for (int i = 0; i < existingProperties.Count; i++)
        {
            if (existingProperties[i].Name == name)
            {
                return existingProperties[i];
            }
        }
        return null;
    }
}
