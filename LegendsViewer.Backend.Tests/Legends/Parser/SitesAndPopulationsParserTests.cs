using System.Text;
using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldLinks;
using LegendsViewer.Backend.Legends.WorldObjects;
using Moq;

namespace LegendsViewer.Backend.Tests.Legends.Parser;

[TestClass]
public class SitesAndPopulationsParserTests
{
    [TestMethod]
    public void Parse_ReadsUtf8AndSortsPopulationsWithoutDuplicates()
    {
        var civilized = new List<Population>();
        var sites = new List<Population>();
        var outdoor = new List<Population>();
        var underground = new List<Population>();
        var world = new Mock<IWorld>();
        world.SetupGet(x => x.CivilizedPopulations).Returns(civilized);
        world.SetupGet(x => x.SitePopulations).Returns(sites);
        world.SetupGet(x => x.OutdoorPopulations).Returns(outdoor);
        world.SetupGet(x => x.UndergroundPopulations).Returns(underground);
        world.SetupGet(x => x.Entities).Returns([]);
        world.SetupGet(x => x.Sites).Returns([]);
        world.SetupGet(x => x.HistoricalFigures).Returns([]);
        world.Setup(x => x.GetCreatureInfo(It.IsAny<string>())).Returns((string id) => new CreatureInfo(id));

        var path = Path.GetTempFileName();
        File.WriteAllText(path, "Civilized World Population\nignored\n\t2 élves\n\t10 dwarves\n\nSites\n\nOutdoor Animal Populations (Including Undead)\n\n\t3 wolves\n\t8 ravens\n\nUnderground Animal Populations (Including Undead)\n\t4 bats\n\t9 rats\n\n", Encoding.UTF8);
        try
        {
            using var parser = new SitesAndPopulationsParser(world.Object, path);
            parser.Parse();
        }
        finally
        {
            File.Delete(path);
        }

        CollectionAssert.AreEqual(new[] { 10, 2 }, civilized.Select(x => x.Count).ToArray());
        Assert.AreEqual("Élves", civilized[1].Race.NameSingular);
        CollectionAssert.AreEqual(new[] { 8, 3 }, outdoor.Select(x => x.Count).ToArray());
        CollectionAssert.AreEqual(new[] { 9, 4 }, underground.Select(x => x.Count).ToArray());
    }

    [TestMethod]
    public void Parse_InfersOnlyLivingLairResidentsWithoutExplicitPopulation()
    {
        var world = new Mock<IWorld>();
        var inferredSite = new Site([], world.Object) { Id = 1, SiteType = SiteType.Lair };
        var explicitSite = new Site([], world.Object) { Id = 2, SiteType = SiteType.Lair };
        world.Setup(x => x.GetSite(1)).Returns(inferredSite);
        world.Setup(x => x.GetSite(2)).Returns(explicitSite);
        var race = new CreatureInfo("starved monster");
        var lairLink = new SiteLink(
            [new Property { Name = "link_type", Value = "lair" }, new Property { Name = "site_id", Value = "1" }],
            world.Object);
        var figures = new List<HistoricalFigure>
        {
            new() { Race = race, DeathYear = -1, RelatedSites = [lairLink] },
            new() { Race = race, DeathYear = 1, RelatedSites = [lairLink] }
        };
        world.SetupGet(x => x.Entities).Returns([]);
        world.SetupGet(x => x.Sites).Returns([inferredSite, explicitSite]);
        world.SetupGet(x => x.HistoricalFigures).Returns(figures);
        world.SetupGet(x => x.CivilizedPopulations).Returns([]);
        world.SetupGet(x => x.SitePopulations).Returns([]);
        world.SetupGet(x => x.OutdoorPopulations).Returns([]);
        world.SetupGet(x => x.UndergroundPopulations).Returns([]);
        world.Setup(x => x.GetCreatureInfo(It.IsAny<string>())).Returns((string id) => new CreatureInfo(id));

        var path = Path.GetTempFileName();
        File.WriteAllText(path, "Civilized World Population\nignored\n\nSites\n\n1: Empty, lair\n2: Occupied, lair\n\t7 dwarves\nOutdoor Animal Populations (Including Undead)\n\n\t1 wolves\n\nUnderground Animal Populations (Including Undead)\n\t1 bats\n\n");
        try
        {
            using var parser = new SitesAndPopulationsParser(world.Object, path);
            parser.Parse();
        }
        finally
        {
            File.Delete(path);
        }

        Assert.AreEqual(1, inferredSite.Populations.Single().Count);
        Assert.AreEqual("Starved Monsters", new SiteMarkerDto(inferredSite).Populations.Single().Race.NamePlural);
        Assert.AreEqual(7, explicitSite.Populations.Single().Count);
    }
}
