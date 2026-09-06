using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using Moq;

namespace LegendsViewer.Backend.Tests.Legends.Events;

[TestClass]
public class HfDoesInteractionTests
{
    private Mock<IWorld> _mockWorld = null!;
    private HistoricalFigure _doer = null!;
    private HistoricalFigure _target = null!;
    private Site _site = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockWorld = new Mock<IWorld>();

        // Create doer
        _doer = new HistoricalFigure
        {
            Id = 1,
            Name = "Vampire Lord",
            Icon = "person"
        };

        // Create target
        _target = new HistoricalFigure
        {
            Id = 2,
            Name = "Innocent Villager",
            Icon = "person"
        };

        // Create site
        _site = new Site([], _mockWorld.Object)
        {
            Id = 1,
            Name = "Village"
        };

        // Setup mock world
        _mockWorld.Setup(w => w.GetHistoricalFigure(1)).Returns(_doer);
        _mockWorld.Setup(w => w.GetHistoricalFigure(2)).Returns(_target);
        _mockWorld.Setup(w => w.GetSite(1)).Returns(_site);
    }

    [TestMethod]
    public void Constructor_WithValidProperties_ParsesCorrectly()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "doer_hfid", Value = "1" },
            new Property { Name = "target_hfid", Value = "2" },
            new Property { Name = "interaction", Value = "VAMPIRE" },
            new Property { Name = "site", Value = "1" }
        };

        // Act
        var hfDoesInteraction = new HfDoesInteraction(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(_doer, hfDoesInteraction.Doer);
        Assert.AreEqual(_target, hfDoesInteraction.Target);
        Assert.AreEqual("VAMPIRE", hfDoesInteraction.Interaction);
    }

    [TestMethod]
    public void Print_WithLinkTrue_ReturnsFormattedString()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "doer_hfid", Value = "1" },
            new Property { Name = "target_hfid", Value = "2" },
            new Property { Name = "interaction", Value = "VAMPIRE" },
            new Property { Name = "site", Value = "1" }
        };
        var hfDoesInteraction = new HfDoesInteraction(properties, _mockWorld.Object);

        // Act
        var result = hfDoesInteraction.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("Vampire Lord"));
        Assert.IsTrue(result.Contains("Innocent Villager"));
    }

    [TestMethod]
    public void Constructor_AddsEventToDoer()
    {
        // Arrange
        var initialEventCount = _doer.Events.Count;

        var properties = new List<Property>
        {
            new Property { Name = "doer_hfid", Value = "1" },
            new Property { Name = "target_hfid", Value = "2" },
            new Property { Name = "interaction", Value = "VAMPIRE" }
        };

        // Act
        var hfDoesInteraction = new HfDoesInteraction(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(initialEventCount + 1, _doer.Events.Count);
    }

    [TestMethod]
    public void Constructor_WithDeityMajorCurse_SetsVampireCreatureTypeAndIsVampire()
    {
        // Arrange: Deity major curses (e.g. DEITY_MAJOR_CURSE_10) represent vampire curses in DF legends XML
        var properties = new List<Property>
        {
            new Property { Name = "doer_hfid", Value = "1" },
            new Property { Name = "target_hfid", Value = "2" },
            new Property { Name = "interaction", Value = "DEITY_MAJOR_CURSE_10" }
        };

        // Act
        var hfDoesInteraction = new HfDoesInteraction(properties, _mockWorld.Object);

        // Assert
        Assert.IsTrue(_target.ActiveInteractions.Contains("DEITY_MAJOR_CURSE_10"));
        Assert.IsTrue(LegendsViewer.Backend.Extensions.HistoricalFigureExtensions.IsVampire(_target));
        Assert.IsTrue(_target.CreatureTypes.Any(ct => ct.Type == "vampire"));
    }
}
