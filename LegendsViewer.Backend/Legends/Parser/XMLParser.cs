using LegendsViewer.Backend.Legends.Interfaces;
using System.Data;
using System.Xml;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Events.PlusEvents;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Parser;

public class XmlParser : IDisposable
{
    protected readonly XmlReader XmlReader;
    protected IWorld World;
    protected Section CurrentSection;

    private string _currentItemName = "";
    private readonly XmlPlusParser? _xmlPlusParser;
    protected readonly Stream _xmlStream;

    protected XmlParser(IWorld world, string xmlFile)
    {
        World = world;
        _xmlStream = new FilteredStream(new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read));
        XmlReader = XmlReader.Create(
            _xmlStream,
            new XmlReaderSettings { Async = true, IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true });
    }

    public XmlParser(IWorld world, string xmlFile, string? xmlPlusFile) : this(world, xmlFile)
    {
        if (xmlPlusFile != xmlFile && File.Exists(xmlPlusFile))
        {
            _xmlPlusParser = new XmlPlusParser(world, xmlPlusFile);
            World.Log.AppendLine("Found LEGENDS_PLUS.XML!");
            World.Log.AppendLine("Parsed additional data...\n");
        }
        else
        {
            World.Log.AppendLine("Missing LEGENDS_PLUS.XML!");
            World.Log.AppendLine("Use DFHacks' \"exportlegends info\" if available...\n");
        }
    }

    public virtual async Task ParseAsync()
    {
        if (_xmlPlusParser != null)
        {
            await _xmlPlusParser.ParseAsync();
        }
        while (!XmlReader.EOF)
        {
            CurrentSection = GetSectionType(XmlReader.Name);
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
        XmlReader.Close();
    }

    protected Section GetSectionType(string sectionName)
    {
        switch (sectionName)
        {
            case "artifacts":
                return Section.Artifacts;
            case "entities":
                return Section.Entities;
            case "entity_populations":
                return Section.EntityPopulations;
            case "historical_eras":
                return Section.Eras;
            case "historical_event_collections":
                return Section.EventCollections;
            case "historical_events":
                return Section.Events;
            case "historical_figures":
                return Section.HistoricalFigures;
            case "regions":
                return Section.Regions;
            case "sites":
                return Section.Sites;
            case "underground_regions":
                return Section.UndergroundRegions;
            case "world_constructions":
                return Section.WorldConstructions;
            case "poetic_forms":
                return Section.PoeticForms;
            case "musical_forms":
                return Section.MusicalForms;
            case "dance_forms":
                return Section.DanceForms;
            case "written_contents":
                return Section.WrittenContent;
            case "landmasses":
                return Section.Landmasses;
            case "mountain_peaks":
                return Section.MountainPeaks;
            case "creature_raw":
                return Section.CreatureRaw;
            case "identities":
                return Section.Identities;
            case "rivers":
                return Section.Rivers;
            case "historical_event_relationships":
                return Section.HistoricalEventRelationships;
            case "historical_event_relationship_supplements":
                return Section.HistoricalEventRelationshipSupplement;
            case "name":
                return Section.Name;
            case "altname":
                return Section.AlternativeName;
            case "xml":
            case "":
            case "df_world":
                return Section.Junk;
            default:
                World.ParsingErrors.Report("Unknown XML Section: " + sectionName);
                return Section.Unknown;
        }
    }

    protected virtual async Task ParseSectionAsync()
    {
        string currentSectionName = XmlReader.Name;

        // Move past the start element
        XmlReader.ReadStartElement();

        // Parse items until we reach the end element for the current section
        while (XmlReader.NodeType != XmlNodeType.EndElement || XmlReader.Name != currentSectionName)
        {
            // Ensure we're not at the end of the section before parsing items
            if (XmlReader.NodeType == XmlNodeType.Element)
            {
                List<Property>? itemProperties = await ParseItemPropertiesAsync();
                if (itemProperties != null)
                {
                    if (_xmlPlusParser != null)
                    {
                        await _xmlPlusParser.AddNewPropertiesAsync(itemProperties, CurrentSection);
                    }

                    AddItemToWorld(itemProperties);
                }
            }
            else
            {
                await XmlReader.ReadAsync(); // Move to next node if not an element
            }
        }

        ProcessXmlSection(CurrentSection); // Done with section, do post processing

        // Move past the end element for the section
        XmlReader.ReadEndElement();
    }

    protected async Task SkipSectionAsync()
    {
        // Move past the start element of the section
        XmlReader.ReadStartElement();

        // Skip all nodes until we find the end element of the current section
        int depth = 1;  // Depth starts at 1 because we've entered a section
        while (depth > 0)
        {
            await XmlReader.ReadAsync(); // Read the next node

            if (XmlReader.NodeType == XmlNodeType.Element && !XmlReader.IsEmptyElement)
            {
                depth++;  // Entering a new nested element, increase depth
            }
            else if (XmlReader.NodeType == XmlNodeType.EndElement)
            {
                depth--;  // Exiting an element, decrease depth
            }
        }
        // Now we're at the correct end element, no need to call ReadEndElement() again
    }

    protected async Task<List<Property>?> ParseItemPropertiesAsync()
    {
        _currentItemName = XmlReader.Name;

        if (XmlReader.NodeType == XmlNodeType.EndElement)
        {
            return null;
        }
        List<Property> properties = [];
        if (XmlReader.NodeType == XmlNodeType.Text)
        {
            properties.Add(new Property()
            {
                Name = nameof(XmlNodeType.Text),
                Value = XmlReader.Value
            });
            AddItemToWorld(properties);
            await XmlReader.ReadAsync();  // Move past the text
            return null;
        }

        // Advance to the start element if necessary
        XmlReader.ReadStartElement();


        // Read properties while not at the end of the element or a sibling element
        while (XmlReader.NodeType != XmlNodeType.EndElement && XmlReader.Name != _currentItemName)
        {
            Property? property = await ParsePropertyAsync();
            if (property != null)
            {
                properties.Add(property);
            }
        }

        // Move past the end element
        XmlReader.ReadEndElement();

        return properties;
    }

    private async Task<Property?> ParsePropertyAsync()
    {
        // Handle invalid or missing property names
        if (string.IsNullOrEmpty(XmlReader.Name))
        {
            return null;
        }

        Property property = new();

        // Handle empty elements
        if (XmlReader.IsEmptyElement)
        {
            property.Name = XmlReader.Name;
            XmlReader.ReadStartElement();  // Move past the empty element
            return property;
        }

        // Read the start of the element
        property.Name = XmlReader.Name;
        XmlReader.ReadStartElement();

        // If it's a text node, just grab the value
        if (XmlReader.NodeType == XmlNodeType.Text)
        {
            property.Value = XmlReader.Value;
            await XmlReader.ReadAsync();  // Move past the text
        }
        // If it's a nested element, read sub-properties
        else if (XmlReader.NodeType == XmlNodeType.Element)
        {
            property.SubProperties = [];

            while (XmlReader.NodeType != XmlNodeType.EndElement)
            {
                Property? subProperty = await ParsePropertyAsync();
                if (subProperty != null)
                {
                    property.SubProperties.Add(subProperty);
                }
            }
        }

        // Move past the end of the element
        XmlReader.ReadEndElement();

        return property;
    }

    protected void AddItemToWorld(List<Property> properties)
    {
        string eventType = "";
        if (CurrentSection == Section.Events || CurrentSection == Section.EventCollections)
        {
            eventType = properties.Find(property => property.Name == "type")?.Value ?? "undefined";
        }

        if (CurrentSection != Section.Events && CurrentSection != Section.EventCollections)
        {
            AddFromXmlSection(CurrentSection, properties);
        }
        else if (CurrentSection == Section.EventCollections)
        {
            AddEventCollection(eventType, properties);
        }
        else if (CurrentSection == Section.Events)
        {
            AddEvent(eventType, properties);
        }

#if DEBUG
        string path = CurrentSection.ToString();
        if (CurrentSection == Section.Events || CurrentSection == Section.EventCollections)
        {
            path += " '" + eventType + "'/";
        }

        CheckKnownStateOfProperties(path, properties);
#endif
    }

#if DEBUG
    public void CheckKnownStateOfProperties(string path, List<Property> properties)
    {
        foreach (Property property in properties)
        {
            if (!property.Known)
            {
                World.ParsingErrors.Report("|==> " + path + " --- Unknown Property: " + property.Name, property.Value);
            }
            if (property.SubProperties != null)
            {
                CheckKnownStateOfProperties(path + "/" + property.Name, property.SubProperties);
            }
        }
    }
#endif

    public void AddFromXmlSection(Section section, List<Property> properties)
    {
        switch (section)
        {
            case Section.Name:
                World.Name = properties.Count > 0 ? properties[0].Value : "";
                break;
            case Section.AlternativeName:
                World.AlternativeName = properties.Count > 0 ? properties[0].Value : "";
                break;
            case Section.Regions:
                World.Regions.Add(new WorldRegion(properties, World));
                break;
            case Section.UndergroundRegions:
                World.UndergroundRegions.Add(new UndergroundRegion(properties, World));
                break;
            case Section.Sites:
                World.Sites.Add(new Site(properties, World));
                break;
            case Section.HistoricalFigures:
                World.HistoricalFigures.Add(new HistoricalFigure(properties, World));
                break;
            case Section.EntityPopulations:
                World.EntityPopulations.Add(new EntityPopulation(properties, World));
                break;
            case Section.Entities:
                World.Entities.Add(new Entity(properties, World));
                break;
            case Section.Eras:
                World.Eras.Add(new Era(properties, World));
                break;
            case Section.Artifacts:
                World.Artifacts.Add(new Artifact(properties, World));
                break;
            case Section.WorldConstructions:
                World.WorldConstructions.Add(new WorldConstruction(properties, World));
                break;
            case Section.PoeticForms:
                World.PoeticForms.Add(new PoeticForm(properties, World));
                break;
            case Section.MusicalForms:
                World.MusicalForms.Add(new MusicalForm(properties, World));
                break;
            case Section.DanceForms:
                World.DanceForms.Add(new DanceForm(properties, World));
                break;
            case Section.WrittenContent:
                World.WrittenContents.Add(new WrittenContent(properties, World));
                break;
            case Section.Landmasses:
                World.Landmasses.Add(new Landmass(properties, World));
                break;
            case Section.MountainPeaks:
                World.MountainPeaks.Add(new MountainPeak(properties, World));
                break;
            case Section.CreatureRaw:
                World.AddCreatureInfo(new CreatureInfo(properties, World));
                break;
            case Section.Identities:
                World.Identities.Add(new Identity(properties, World));
                break;
            case Section.Rivers:
                World.Rivers.Add(new River(properties, World));
                break;
            case Section.HistoricalEventRelationships:
                WorldEvent historicalEventRelationShip = new HistoricalEventRelationShip(properties, World);
                World.Events.Add(historicalEventRelationShip);
                World.SpecialEventsById.Add(historicalEventRelationShip.Id, historicalEventRelationShip);
                break;
            case Section.HistoricalEventRelationshipSupplement:
                HistoricalEventRelationShip.ResolveSupplements(properties, World);
                break;
            default:
                World.ParsingErrors.Report($"Unknown XML Section: {section}");
                break;
        }
    }

    private void AddEvent(string type, List<Property> properties)
    {
        switch (type)
        {
            case "add hf entity link":
                World.Events.Add(new AddHfEntityLink(properties, World));
                break;
            case "add hf hf link":
                World.Events.Add(new AddHfhfLink(properties, World));
                break;
            case "attacked site":
                World.Events.Add(new AttackedSite(properties, World));
                break;
            case "body abused":
                World.Events.Add(new BodyAbused(properties, World));
                break;
            case "change hf job":
                World.Events.Add(new ChangeHfJob(properties, World));
                break;
            case "change hf state":
                World.Events.Add(new ChangeHfState(properties, World));
                break;
            case "changed creature type":
                World.Events.Add(new ChangedCreatureType(properties, World));
                break;
            case "create entity position":
                World.Events.Add(new CreateEntityPosition(properties, World));
                break;
            case "created site":
                World.Events.Add(new CreatedSite(properties, World));
                break;
            case "created world construction":
                World.Events.Add(new CreatedWorldConstruction(properties, World));
                break;
            case "creature devoured":
                World.Events.Add(new CreatureDevoured(properties, World));
                break;
            case "destroyed site":
                World.Events.Add(new DestroyedSite(properties, World));
                break;
            case "field battle":
                World.Events.Add(new FieldBattle(properties, World));
                break;
            case "hf abducted":
                World.Events.Add(new HfAbducted(properties, World));
                break;
            case "hf died":
                World.Events.Add(new HfDied(properties, World));
                break;
            case "hf new pet":
                World.Events.Add(new HfNewPet(properties, World));
                break;
            case "hf reunion":
                World.Events.Add(new HfReunion(properties, World));
                break;
            case "hf simple battle event":
                World.Events.Add(new HfSimpleBattleEvent(properties, World));
                break;
            case "hf travel":
                World.Events.Add(new HfTravel(properties, World));
                break;
            case "hf wounded":
                World.Events.Add(new HfWounded(properties, World));
                break;
            case "impersonate hf":
                World.Events.Add(new ImpersonateHf(properties, World));
                break;
            case "item stolen":
                World.Events.Add(new ItemStolen(properties, World));
                break;
            case "new site leader":
                World.Events.Add(new NewSiteLeader(properties, World));
                break;
            case "peace accepted":
                World.Events.Add(new PeaceAccepted(properties, World));
                break;
            case "peace rejected":
                World.Events.Add(new PeaceRejected(properties, World));
                break;
            case "plundered site":
                World.Events.Add(new PlunderedSite(properties, World));
                break;
            case "reclaim site":
                World.Events.Add(new ReclaimSite(properties, World));
                break;
            case "remove hf entity link":
                World.Events.Add(new RemoveHfEntityLink(properties, World));
                break;
            case "artifact created":
                World.Events.Add(new ArtifactCreated(properties, World));
                break;
            case "diplomat lost":
                World.Events.Add(new DiplomatLost(properties, World));
                break;
            case "entity created":
                World.Events.Add(new EntityCreated(properties, World));
                break;
            case "hf revived":
                World.Events.Add(new HfRevived(properties, World));
                break;
            case "masterpiece arch design":
                World.Events.Add(new MasterpieceArchDesign(properties, World));
                break;
            case "masterpiece arch constructed":
                World.Events.Add(new MasterpieceArchConstructed(properties, World));
                break;
            case "masterpiece engraving":
                World.Events.Add(new MasterpieceEngraving(properties, World));
                break;
            case "masterpiece food":
                World.Events.Add(new MasterpieceFood(properties, World));
                break;
            case "masterpiece lost":
                World.Events.Add(new MasterpieceLost(properties, World));
                break;
            case "masterpiece item":
                World.Events.Add(new MasterpieceItem(properties, World));
                break;
            case "masterpiece item improvement":
                World.Events.Add(new MasterpieceItemImprovement(properties, World));
                break;
            case "merchant":
                World.Events.Add(new Merchant(properties, World));
                break;
            case "site abandoned":
                World.Events.Add(new SiteAbandoned(properties, World));
                break;
            case "site died":
                World.Events.Add(new SiteDied(properties, World));
                break;
            case "add hf site link":
                World.Events.Add(new AddHfSiteLink(properties, World));
                break;
            case "created structure":
                World.Events.Add(new CreatedStructure(properties, World));
                break;
            case "hf razed structure":
                World.Events.Add(new HfRazedStructure(properties, World));
                break;
            case "remove hf site link":
                World.Events.Add(new RemoveHfSiteLink(properties, World));
                break;
            case "replaced structure":
                World.Events.Add(new ReplacedStructure(properties, World));
                break;
            case "site taken over":
                World.Events.Add(new SiteTakenOver(properties, World));
                break;
            case "entity relocate":
                World.Events.Add(new EntityRelocate(properties, World));
                break;
            case "hf gains secret goal":
                World.Events.Add(new HfGainsSecretGoal(properties, World));
                break;
            case "hf profaned structure":
                World.Events.Add(new HfProfanedStructure(properties, World));
                break;
            case "hf does interaction":
                World.Events.Add(new HfDoesInteraction(properties, World));
                break;
            case "entity primary criminals":
                World.Events.Add(new EntityPrimaryCriminals(properties, World));
                break;
            case "hf confronted":
                World.Events.Add(new HfConfronted(properties, World));
                break;
            case "assume identity":
                World.Events.Add(new AssumeIdentity(properties, World));
                break;
            case "entity law":
                World.Events.Add(new EntityLaw(properties, World));
                break;
            case "change hf body state":
                World.Events.Add(new ChangeHfBodyState(properties, World));
                break;
            case "razed structure":
                World.Events.Add(new RazedStructure(properties, World));
                break;
            case "hf learns secret":
                World.Events.Add(new HfLearnsSecret(properties, World));
                break;
            case "artifact stored":
                World.Events.Add(new ArtifactStored(properties, World));
                break;
            case "artifact possessed":
                World.Events.Add(new ArtifactPossessed(properties, World));
                break;
            case "agreement made":
                World.Events.Add(new AgreementMade(properties, World));
                break;
            case "agreement rejected":
                World.Events.Add(new AgreementRejected(properties, World));
                break;
            case "artifact lost":
                World.Events.Add(new ArtifactLost(properties, World));
                break;
            case "site dispute":
                World.Events.Add(new SiteDispute(properties, World));
                break;
            case "hf attacked site":
                World.Events.Add(new HfAttackedSite(properties, World));
                break;
            case "hf destroyed site":
                World.Events.Add(new HfDestroyedSite(properties, World));
                break;
            case "agreement formed":
                World.Events.Add(new AgreementFormed(properties, World));
                break;
            case "site tribute forced":
                World.Events.Add(new SiteTributeForced(properties, World));
                break;
            case "insurrection started":
                World.Events.Add(new InsurrectionStarted(properties, World));
                break;
            case "procession":
                World.Events.Add(new Procession(properties, World));
                break;
            case "ceremony":
                World.Events.Add(new Ceremony(properties, World));
                break;
            case "performance":
                World.Events.Add(new Performance(properties, World));
                break;
            case "competition":
                World.Events.Add(new Competition(properties, World));
                break;
            case "written content composed":
                World.Events.Add(new WrittenContentComposed(properties, World));
                break;
            case "poetic form created":
                World.Events.Add(new PoeticFormCreated(properties, World));
                break;
            case "musical form created":
                World.Events.Add(new MusicalFormCreated(properties, World));
                break;
            case "dance form created":
                World.Events.Add(new DanceFormCreated(properties, World));
                break;
            case "knowledge discovered":
                World.Events.Add(new KnowledgeDiscovered(properties, World));
                break;
            case "hf relationship denied":
                World.Events.Add(new HfRelationShipDenied(properties, World));
                break;
            case "regionpop incorporated into entity":
                World.Events.Add(new RegionpopIncorporatedIntoEntity(properties, World));
                break;
            case "artifact destroyed":
                World.Events.Add(new ArtifactDestroyed(properties, World));
                break;
            case "first contact":
                World.Events.Add(new FirstContact(properties, World));
                break;
            case "site retired":
                World.Events.Add(new SiteRetired(properties, World));
                break;
            case "agreement concluded":
                World.Events.Add(new AgreementConcluded(properties, World));
                break;
            case "hf reach summit":
                World.Events.Add(new HfReachSummit(properties, World));
                break;
            case "artifact transformed":
                World.Events.Add(new ArtifactTransformed(properties, World));
                break;
            case "masterpiece dye":
                World.Events.Add(new MasterpieceDye(properties, World));
                break;
            case "hf disturbed structure":
                World.Events.Add(new HfDisturbedStructure(properties, World));
                break;
            case "hfs formed reputation relationship":
                World.Events.Add(new HfsFormedReputationRelationship(properties, World));
                break;
            case "artifact given":
                World.Events.Add(new ArtifactGiven(properties, World));
                break;
            case "artifact claim formed":
                World.Events.Add(new ArtifactClaimFormed(properties, World));
                break;
            case "hf recruited unit type for entity":
                World.Events.Add(new HfRecruitedUnitTypeForEntity(properties, World));
                break;
            case "hf prayed inside structure":
                World.Events.Add(new HfPrayedInsideStructure(properties, World));
                break;
            case "artifact copied":
                World.Events.Add(new ArtifactCopied(properties, World));
                break;
            case "artifact recovered":
                World.Events.Add(new ArtifactRecovered(properties, World));
                break;
            case "artifact found":
                World.Events.Add(new ArtifactFound(properties, World));
                break;
            case "hf viewed artifact":
                World.Events.Add(new HfViewedArtifact(properties, World));
                break;
            case "sneak into site":
                World.Events.Add(new SneakIntoSite(properties, World));
                break;
            case "spotted leaving site":
                World.Events.Add(new SpottedLeavingSite(properties, World));
                break;
            case "entity searched site":
                World.Events.Add(new EntitySearchedSite(properties, World));
                break;
            case "hf freed":
                World.Events.Add(new HfFreed(properties, World));
                break;
            case "tactical situation":
                World.Events.Add(new TacticalSituation(properties, World));
                break;
            case "squad vs squad":
                World.Events.Add(new SquadVsSquad(properties, World));
                break;
            case "agreement void":
                World.Events.Add(new AgreementVoid(properties, World));
                break;
            case "entity rampaged in site":
                World.Events.Add(new EntityRampagedInSite(properties, World));
                break;
            case "entity fled site":
                World.Events.Add(new EntityFledSite(properties, World));
                break;
            case "entity expels hf":
                World.Events.Add(new EntityExpelsHf(properties, World));
                break;
            case "site surrendered":
                World.Events.Add(new SiteSurrendered(properties, World));
                break;
            case "remove hf hf link":
                World.Events.Add(new RemoveHfHfLink(properties, World));
                break;
            case "holy city declaration":
                World.Events.Add(new HolyCityDeclaration(properties, World));
                break;
            case "hf performed horrible experiments":
                World.Events.Add(new HfPerformedHorribleExperiments(properties, World));
                break;
            case "entity incorporated":
                World.Events.Add(new EntityIncorporated(properties, World));
                break;
            case "gamble":
                World.Events.Add(new Gamble(properties, World));
                break;
            case "trade":
                World.Events.Add(new Trade(properties, World));
                break;
            case "hf equipment purchase":
                World.Events.Add(new HfEquipmentPurchase(properties, World));
                break;
            case "entity overthrown":
                World.Events.Add(new EntityOverthrown(properties, World));
                break;
            case "failed frame attempt":
                World.Events.Add(new FailedFrameAttempt(properties, World));
                break;
            case "hf convicted":
                World.Events.Add(new HfConvicted(properties, World));
                break;
            case "failed intrigue corruption":
                World.Events.Add(new FailedIntrigueCorruption(properties, World));
                break;
            case "hfs formed intrigue relationship":
                World.Events.Add(new HfsFormedIntrigueRelationship(properties, World));
                break;
            case "entity alliance formed":
                World.Events.Add(new EntityAllianceFormed(properties, World));
                break;
            case "entity dissolved":
                World.Events.Add(new EntityDissolved(properties, World));
                break;
            case "add hf entity honor":
                World.Events.Add(new AddHfEntityHonor(properties, World));
                break;
            case "entity breach feature layer":
                World.Events.Add(new EntityBreachFeatureLayer(properties, World));
                break;
            case "entity equipment purchase":
                World.Events.Add(new EntityEquipmentPurchase(properties, World));
                break;
            case "hf ransomed":
                World.Events.Add(new HfRansomed(properties, World));
                break;
            case "hf preach":
                World.Events.Add(new HfPreach(properties, World));
                break;
            case "modified building":
                World.Events.Add(new ModifiedBuilding(properties, World));
                break;
            case "hf interrogated":
                World.Events.Add(new HfInterrogated(properties, World));
                break;
            case "entity persecuted":
                World.Events.Add(new EntityPersecuted(properties, World));
                break;
            case "building profile acquired":
                World.Events.Add(new BuildingProfileAcquired(properties, World));
                break;
            case "hf enslaved":
                World.Events.Add(new HfEnslaved(properties, World));
                break;
            case "hf asked about artifact":
                World.Events.Add(new HfAskedAboutArtifact(properties, World));
                break;
            case "hf carouse":
                World.Events.Add(new HfCarouse(properties, World));
                break;
            case "sabotage":
                World.Events.Add(new Sabotage(properties, World));
                break;
            default:
                World.ParsingErrors.Report("\nUnknown Event: " + type);
                break;
        }
    }

    private void AddEventCollection(string type, List<Property> properties)
    {
        switch (type)
        {
            case "abduction":
                World.EventCollections.Add(new Abduction(properties, World));
                break;
            case "battle":
                World.EventCollections.Add(new Battle(properties, World));
                break;
            case "beast attack":
                World.EventCollections.Add(new BeastAttack(properties, World));
                break;
            case "duel":
                World.EventCollections.Add(new Duel(properties, World));
                break;
            case "journey":
                World.EventCollections.Add(new Journey(properties, World));
                break;
            case "site conquered":
                World.EventCollections.Add(new SiteConquered(properties, World));
                break;
            case "theft":
                World.EventCollections.Add(new Theft(properties, World));
                break;
            case "war":
                World.EventCollections.Add(new War(properties, World));
                break;
            case "insurrection":
                World.EventCollections.Add(new Insurrection(properties, World));
                break;
            case "occasion":
                World.EventCollections.Add(new Occasion(properties, World));
                break;
            case "procession":
                World.EventCollections.Add(new ProcessionCollection(properties, World));
                break;
            case "ceremony":
                World.EventCollections.Add(new CeremonyCollection(properties, World));
                break;
            case "performance":
                World.EventCollections.Add(new PerformanceCollection(properties, World));
                break;
            case "competition":
                World.EventCollections.Add(new CompetitionCollection(properties, World));
                break;
            case "purge":
                World.EventCollections.Add(new Purge(properties, World));
                break;
            case "raid":
                World.EventCollections.Add(new Raid(properties, World));
                break;
            case "persecution":
                World.EventCollections.Add(new Persecution(properties, World));
                break;
            case "entity overthrown":
                World.EventCollections.Add(new EntityOverthrownCollection(properties, World));
                break;
            default:
                World.ParsingErrors.Report("\nUnknown Event Collection: " + type);
                break;
        }
    }

    private void ProcessXmlSection(Section section)
    {
        if (section == Section.Events)
        {
            //Calculate Historical Figure Ages.
            int lastYear = World.Events.Last().Year;
            foreach (HistoricalFigure hf in World.HistoricalFigures)
            {
                if (hf.BirthYear != -1)
                {
                    hf.Age = hf.DeathYear > -1
                        ? hf.DeathYear - hf.BirthYear
                        : lastYear - hf.BirthYear;
                }
            }
        }

        //Create sorted Historical Figures so they can be binary searched by name, needed for parsing History file
        if (section == Section.HistoricalFigures)
        {
            World.ProcessHFtoHfLinks();
        }

        //Create sorted entities so they can be binary searched by name, needed for History/sites files
        if (section == Section.Entities)
        {
            World.ProcessReputations();
            World.ProcessHFtoSiteLinks();
            World.ProcessEntityEntityLinks();
        }

        //Calculate end years for eras and add list of wars during era.
        if (section == Section.Eras)
        {
            int lastRecordedYear = World.Events[^1].Year;
            World.Eras[^1].EndYear = lastRecordedYear;
            for (int i = 0; i < World.Eras.Count - 1; i++)
            {
                World.Eras[i].EndYear = World.Eras[i + 1].StartYear - 1;
            }

            foreach (Era era in World.Eras)
            {
                era.Events.AddRange(World.Events
                        .Where(worldEvent => worldEvent.Year >= era.StartYear && worldEvent.Year <= era.EndYear)
                        .OrderBy(worldEvent => worldEvent.Year));
                era.EventCollections.AddRange(World.EventCollections
                        .Where(eventCollection =>
                                eventCollection.StartYear >= era.StartYear && eventCollection.EndYear <= era.EndYear && eventCollection.EndYear != -1
                                // collection started between & ended between
                                || eventCollection.StartYear >= era.StartYear && eventCollection.StartYear <= era.EndYear
                                // collection started before & ended between
                                || eventCollection.EndYear >= era.StartYear && eventCollection.EndYear <= era.EndYear && eventCollection.EndYear != -1
                                // collection started during & ended after
                                || eventCollection.StartYear <= era.StartYear && eventCollection.EndYear >= era.EndYear
                                // collection started before & ended after
                                || eventCollection.StartYear <= era.StartYear && eventCollection.EndYear == -1)
                        .OrderBy(eventCollection => eventCollection.StartYear));

                string duration;
                if (era.StartYear == -1 && era.EndYear > 0)
                {
                    duration = $"{era.EndYear} years";
                }
                else
                {
                    duration = $"{era.EndYear - era.StartYear} years";
                }

                era.Type = duration;
                era.Subtype = $"{(era.StartYear == -1 ? ".." : era.StartYear.ToString())} - {(era.EndYear == lastRecordedYear ? ".." : era.EndYear.ToString())}";
            }
        }

        if (section == Section.EventCollections)
        {
            ProcessCollections();
        }
    }

    private void ProcessCollections()
    {
        foreach (EventCollection eventCollection in World.EventCollections)
        {
            //Sub Event Collections aren't created until after the main collection
            //So only IDs are stored in the main collection until here now that all collections have been created
            //and can now be added to their Parent collection
            foreach (int collectionId in eventCollection.CollectionIDs)
            {
                EventCollection? subEventCollection = World.GetEventCollection(collectionId);
                if (subEventCollection != null)
                {
                    eventCollection.EventCollections.Add(subEventCollection);
                }
            }
        }

        //Attempt at calculating beast historical figure for beast attacks.
        //Find beast by looking at eventsList and fill in some event properties from the beast attacks's properties
        //Calculated here so it can look in Duel collections contained in beast attacks
        foreach (BeastAttack beastAttack in World.EventCollections.OfType<BeastAttack>())
        {
            if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HfAttackedSite>().Any())
            {
                beastAttack.Beast = beastAttack.GetSubEvents().OfType<HfAttackedSite>().First().Attacker;
            }
            if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HfDestroyedSite>().Any())
            {
                beastAttack.Beast = beastAttack.GetSubEvents().OfType<HfDestroyedSite>().First().Attacker;
            }

            //Find Beast by looking at fights, Beast always engages the first fight in a Beast Attack?
            if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HfSimpleBattleEvent>().Any())
            {
                var hfSimpleBattleEvent = beastAttack.GetSubEvents().OfType<HfSimpleBattleEvent>().First();
                beastAttack.Beast = hfSimpleBattleEvent.HistoricalFigure1;
            }
            if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<AddHfEntityLink>().Any())
            {
                var addHfEntityLink = beastAttack.GetSubEvents().OfType<AddHfEntityLink>().FirstOrDefault(link => link.LinkType == HfEntityLinkType.Enemy);
                beastAttack.Beast = addHfEntityLink?.HistoricalFigure;
            }
            if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HfDied>().Any())
            {
                var hfDied = beastAttack.GetSubEvents().OfType<HfDied>().FirstOrDefault();
                if (hfDied?.HistoricalFigure != null && hfDied.HistoricalFigure.RelatedSites.Any(siteLink => siteLink.Type == SiteLinkType.Lair))
                {
                    beastAttack.Beast = hfDied.HistoricalFigure;
                }
                else if (hfDied?.Slayer != null && hfDied.Slayer.RelatedSites.Any(siteLink => siteLink.Type == SiteLinkType.Lair))
                {
                    beastAttack.Beast = hfDied.Slayer;
                }
                else
                {
                    var slayers =
                        beastAttack.GetSubEvents()
                            .OfType<HfDied>()
                            .GroupBy(death => death.Slayer)
                            .Select(hf => new { HF = hf.Key, Count = hf.Count() });
                    if (slayers.Count(slayer => slayer.Count > 1) == 1)
                    {
                        beastAttack.Beast = slayers.Single(slayer => slayer.Count > 1).HF;
                    }
                }
            }

            //Fill in some various event info from collections.

            int insertIndex;
            foreach (ItemStolen theft in beastAttack.Events.OfType<ItemStolen>())
            {
                if (theft.Site == null)
                {
                    theft.Site = beastAttack.Site;
                }
                else
                {
                    beastAttack.Site = theft.Site;
                }
                if (theft.Thief == null)
                {
                    theft.Thief = beastAttack.Beast;
                }
                else if (beastAttack.Beast == null)
                {
                    beastAttack.Beast = theft.Thief;
                }

                if (beastAttack.Site != null)
                {
                    insertIndex = beastAttack.Site.Events.BinarySearch(theft);
                    if (insertIndex < 0)
                    {
                        beastAttack.Site.Events.Add(theft);
                    }
                }
                if (beastAttack.Beast != null)
                {
                    insertIndex = beastAttack.Beast.Events.BinarySearch(theft);
                    if (insertIndex < 0)
                    {
                        beastAttack.Beast.Events.Add(theft);
                    }
                }
            }
            foreach (CreatureDevoured devoured in beastAttack.Events.OfType<CreatureDevoured>())
            {
                if (devoured.Eater == null)
                {
                    devoured.Eater = beastAttack.Beast;
                }
                else if (beastAttack.Beast == null)
                {
                    beastAttack.Beast = devoured.Eater;
                }
                if (beastAttack.Beast != null)
                {
                    insertIndex = beastAttack.Beast.Events.BinarySearch(devoured);
                    if (insertIndex < 0)
                    {
                        beastAttack.Beast.Events.Add(devoured);
                    }
                }
            }
            foreach (HfAbducted abducted in beastAttack.Events.OfType<HfAbducted>())
            {
                if (abducted.Snatcher == null)
                {
                    abducted.Snatcher = beastAttack.Beast;
                }
                else if (beastAttack.Beast == null)
                {
                    beastAttack.Beast = abducted.Snatcher;
                }
                if (beastAttack.Beast != null)
                {
                    insertIndex = beastAttack.Beast.Events.BinarySearch(abducted);
                    if (insertIndex < 0)
                    {
                        beastAttack.Beast.Events.Add(abducted);
                    }
                }
            }
            if (beastAttack.Beast != null)
            {
                if (beastAttack.Beast.BeastAttacks == null)
                {
                    beastAttack.Beast.BeastAttacks = [];
                }
                beastAttack.Beast.BeastAttacks.Add(beastAttack);
            }

            if (beastAttack.Defender != null && beastAttack.Site?.OwnerHistory.Count == 0)
            {
                beastAttack.Site?.OwnerHistory.Add(new OwnerPeriod(beastAttack.Site, beastAttack.Defender, -1, "ancestral claim"));
                var parent = beastAttack.Defender.Parent;
                while (parent != null)
                {
                    if (beastAttack.Site?.OwnerHistory.Count == 0)
                    {
                        beastAttack.Site?.OwnerHistory.Add(new OwnerPeriod(beastAttack.Site, parent, -1, "ancestral claim"));
                    }
                    parent = parent.Parent;
                }
            }
        }

        AssignBattleToSiteConquer();
        AssignSiteToItemStolen();
    }

    private void AssignSiteToItemStolen()
    {
        foreach (var raid in World.EventCollections.OfType<Raid>().Where(r => r.Site != null))
        {
            foreach (var itemStolenEvent in raid.GetSubEvents().OfType<ItemStolen>().Where(i => i.Site == null))
            {
                itemStolenEvent.Site = raid.Site;
            }
        }
    }

    private void AssignBattleToSiteConquer()
    {
        //Assign a Conquering Event its corresponding battle
        //Battle = first Battle prior to the conquering?
        foreach (SiteConquered conquer in World.EventCollections.OfType<SiteConquered>())
        {
            for (int i = conquer.Id - 1; i >= 0; i--)
            {
                EventCollection? collection = World.GetEventCollection(i);
                if (collection == null)
                {
                    continue;
                }

                if (collection.GetType() == typeof(Battle))
                {
                    conquer.Battle = collection as Battle;
                    if (conquer.Battle != null)
                    {
                        conquer.Battle.Conquering = conquer;
                        if (conquer.Battle.Defender == null && conquer.Defender != null)
                        {
                            conquer.Battle.Defender = conquer.Defender;
                        }
                    }

                    break;
                }
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            XmlReader.Close();
            _xmlPlusParser?.Dispose();
            _xmlStream?.Dispose();
        }
    }
}
