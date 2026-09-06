using LegendsViewer.Backend.Legends;
using System.Text;

namespace LegendsViewer.Backend.Tests;

[TestClass]
public class LegendsParserTests
{
    [TestMethod]
    public async Task Test_ParseLegendsXmlFile_Success()
    {
        // Arrange
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var world = new World();
        var legendsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "Xah_Atho-00005-01-01-legends.xml");

        // Act
        await world.ParseAsync(legendsPath, null, null, null, null);

        // Assert
        Assert.AreEqual(17, world.Entities.Count, $"Expected 17 entities, but got {world.Entities.Count}");
        Assert.AreEqual(97, world.HistoricalFigures.Count, $"Expected 97 historical figures, but got {world.HistoricalFigures.Count}");
        Assert.AreEqual(20, world.Regions.Count, $"Expected 20 regions, but got {world.Regions.Count}");
        Assert.AreEqual(20, world.UndergroundRegions.Count, $"Expected 20 underground regions, but got {world.UndergroundRegions.Count}");

        Assert.AreEqual(18, world.Sites.Count, $"Expected 18 sites, but got {world.Sites.Count}");
        Assert.AreEqual(2, world.Structures.Count, $"Expected 2 structures, but got {world.Structures.Count}");
        Assert.AreEqual(0, world.WorldConstructions.Count, $"Expected 0 world constructions, but got {world.WorldConstructions.Count}");

        Assert.AreEqual(1, world.Artifacts.Count, $"Expected 1 artifact, but got {world.Artifacts.Count}");
        Assert.AreEqual(10, world.DanceForms.Count, $"Expected 10 dance forms, but got {world.DanceForms.Count}");
        Assert.AreEqual(7, world.MusicalForms.Count, $"Expected 7 musical forms, but got {world.MusicalForms.Count}");
        Assert.AreEqual(7, world.PoeticForms.Count, $"Expected 7 poetic forms, but got {world.PoeticForms.Count}");
        Assert.AreEqual(0, world.WrittenContents.Count, $"Expected 0 written contents, but got {world.WrittenContents.Count}");

        Assert.AreEqual(7, world.BeastAttacks.Count, $"Expected 7 beast attacks, but got {world.BeastAttacks.Count}");
        Assert.AreEqual(3, world.Thefts.Count, $"Expected 3 thefts, but got {world.Thefts.Count}");

        Assert.AreEqual(1, world.Performances.Count, $"Expected 1 performance, but got {world.Performances.Count}");
        Assert.AreEqual(2, world.Journeys.Count, $"Expected 2 journeys, but got {world.Journeys.Count}");
        Assert.AreEqual(2, world.Occasions.Count, $"Expected 2 occasions, but got {world.Occasions.Count}");

        // XML Plus Content
        Assert.AreEqual(0, world.Landmasses.Count, $"Expected 0 landmasses, but got {world.Landmasses.Count}");
        Assert.AreEqual(0, world.Rivers.Count, $"Expected 0 rivers, but got {world.Rivers.Count}");
        Assert.AreEqual(0, world.MountainPeaks.Count, $"Expected 0 mountain peaks, but got {world.MountainPeaks.Count}");
    }

    [TestMethod]
    public async Task Test_ParseLegendsXmlAndXmlPlusFile_Success()
    {
        // Arrange
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var world = new World();
        var legendsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "Xah_Atho-00005-01-01-legends.xml");
        var legendsPlusPath = Path.Combine(AppContext.BaseDirectory, "TestData", "Xah_Atho-00005-01-01-legends_plus.xml");

        // Act
        await world.ParseAsync(legendsPath, legendsPlusPath, null, null, null);

        // Assert
        Assert.AreEqual(17, world.Entities.Count, $"Expected 17 entities, but got {world.Entities.Count}");
        Assert.AreEqual(97, world.HistoricalFigures.Count, $"Expected 97 historical figures, but got {world.HistoricalFigures.Count}");
        Assert.AreEqual(20, world.Regions.Count, $"Expected 20 regions, but got {world.Regions.Count}");
        Assert.AreEqual(20, world.UndergroundRegions.Count, $"Expected 20 underground regions, but got {world.UndergroundRegions.Count}");

        Assert.AreEqual(18, world.Sites.Count, $"Expected 18 sites, but got {world.Sites.Count}");
        Assert.AreEqual(2, world.Structures.Count, $"Expected 2 structures, but got {world.Structures.Count}");
        Assert.AreEqual(0, world.WorldConstructions.Count, $"Expected 0 world constructions, but got {world.WorldConstructions.Count}");

        Assert.AreEqual(1, world.Artifacts.Count, $"Expected 1 artifact, but got {world.Artifacts.Count}");
        Assert.AreEqual(10, world.DanceForms.Count, $"Expected 10 dance forms, but got {world.DanceForms.Count}");
        Assert.AreEqual(7, world.MusicalForms.Count, $"Expected 7 musical forms, but got {world.MusicalForms.Count}");
        Assert.AreEqual(7, world.PoeticForms.Count, $"Expected 7 poetic forms, but got {world.PoeticForms.Count}");
        Assert.AreEqual(0, world.WrittenContents.Count, $"Expected 0 written contents, but got {world.WrittenContents.Count}");

        Assert.AreEqual(7, world.BeastAttacks.Count, $"Expected 7 beast attacks, but got {world.BeastAttacks.Count}");
        Assert.AreEqual(3, world.Thefts.Count, $"Expected 3 thefts, but got {world.Thefts.Count}");

        Assert.AreEqual(1, world.Performances.Count, $"Expected 1 performance, but got {world.Performances.Count}");
        Assert.AreEqual(2, world.Journeys.Count, $"Expected 2 journeys, but got {world.Journeys.Count}");
        Assert.AreEqual(2, world.Occasions.Count, $"Expected 2 occasions, but got {world.Occasions.Count}");

        // XML Plus Content
        Assert.AreEqual(2, world.Landmasses.Count, $"Expected 2 landmasses, but got {world.Landmasses.Count}");
        Assert.AreEqual(50, world.Rivers.Count, $"Expected 50 rivers, but got {world.Rivers.Count}");
        Assert.AreEqual(2, world.MountainPeaks.Count, $"Expected 2 mountain peaks, but got {world.MountainPeaks.Count}");
    }

    [TestMethod]
    public async Task Test_WorldRecordsCalculator_Success()
    {
        // Arrange
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var world = new World();
        var legendsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "Xah_Atho-00005-01-01-legends.xml");
        var legendsPlusPath = Path.Combine(AppContext.BaseDirectory, "TestData", "Xah_Atho-00005-01-01-legends_plus.xml");
        await world.ParseAsync(legendsPath, legendsPlusPath, null, null, null);

        // Act
        var records = world.GetOrComputeWorldRecords();

        // Assert
        Assert.IsNotNull(records, "World records should not be null");
        Assert.AreEqual(6, records.Categories.Count, "Expected 6 record categories");
        Assert.IsNotNull(records.Highlights, "Highlights should not be null");
    }
}