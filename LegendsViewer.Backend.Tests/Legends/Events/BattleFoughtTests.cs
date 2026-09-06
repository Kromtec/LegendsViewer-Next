using LegendsViewer.Backend.Legends.Events.IncidentalEvents;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using Moq;

namespace LegendsViewer.Backend.Tests.Legends.Events;

[TestClass]
public class BattleFoughtTests
{
    private Mock<IWorld> _mockWorld = null!;
    private HistoricalFigure _historicalFigure = null!;
    private Battle _battle = null!;
    private Site _site = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockWorld = new Mock<IWorld>();
        _mockWorld.Setup(w => w.Events).Returns(new List<LegendsViewer.Backend.Legends.Events.WorldEvent>());

        _site = new Site([], _mockWorld.Object)
        {
            Id = 1,
            Name = "Test Fortress",
            Icon = "fortress"
        };

        _battle = new Battle([], _mockWorld.Object)
        {
            Id = 1,
            Name = "Battle of the Valley"
        };
        _battle.Site = _site;

        _historicalFigure = new HistoricalFigure
        {
            Id = 1,
            Name = "General Ironfist",
            Icon = "person"
        };
    }

    [TestMethod]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Act
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: true);

        // Assert
        Assert.AreEqual(_historicalFigure, battleFought.HistoricalFigure);
        Assert.AreEqual(_battle, battleFought.Battle);
        Assert.AreEqual(true, battleFought.AsAttacker);
        Assert.AreEqual(false, battleFought.WasHired);
        Assert.AreEqual(false, battleFought.AsScout);
        Assert.AreEqual(_site, battleFought.Site);
    }

    [TestMethod]
    public void Constructor_WithHiredAndScout_SetsFlags()
    {
        // Act
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: false, wasHired: true, asScout: true);

        // Assert
        Assert.AreEqual(true, battleFought.WasHired);
        Assert.AreEqual(true, battleFought.AsScout);
        Assert.AreEqual(false, battleFought.AsAttacker);
    }

    [TestMethod]
    public void Print_WithLink_ReturnsFormattedString()
    {
        // Arrange
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: true);

        // Act
        var result = battleFought.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("General Ironfist"));
        Assert.IsTrue(result.Contains("Battle of the Valley"));
        Assert.IsTrue(result.Contains("an assault on"));
    }

    [TestMethod]
    public void Print_WithDefender_ReturnsDefenseString()
    {
        // Arrange
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: false);

        // Act
        var result = battleFought.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("in defense of"));
    }

    [TestMethod]
    public void Print_WithHiredFighter_ReturnsHiredString()
    {
        // Arrange
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: true, wasHired: true);

        // Act
        var result = battleFought.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("was hired"));
        Assert.IsTrue(result.Contains("to fight in"));
    }

    [TestMethod]
    public void Print_WithHiredScout_ReturnsScoutString()
    {
        // Arrange
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: true, wasHired: true, asScout: true);

        // Act
        var result = battleFought.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("was hired"));
        Assert.IsTrue(result.Contains("as a scout"));
    }

    [TestMethod]
    public void Constructor_WhenEventListHasGaps_AssignsIdAboveHighestExistingId()
    {
        // Arrange: simulate unsupported event types being skipped during parsing, which leaves
        // World.Events with gaps so Count (3) is lower than the highest existing Id (5).
        // Regression test for issue #47 where synthetic Ids collided with real event Ids.
        var existingEvents = new List<LegendsViewer.Backend.Legends.Events.WorldEvent>
        {
            new(new List<LegendsViewer.Backend.Legends.Parser.Property>(), _mockWorld.Object) { Id = 0 },
            new(new List<LegendsViewer.Backend.Legends.Parser.Property>(), _mockWorld.Object) { Id = 1 },
            new(new List<LegendsViewer.Backend.Legends.Parser.Property>(), _mockWorld.Object) { Id = 5 },
        };
        _mockWorld.Setup(w => w.Events).Returns(existingEvents);

        // Act
        var battleFought = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: true);

        // Assert: must be highest Id + 1 (6), NOT the list Count (3), to avoid colliding with Id 5.
        Assert.AreEqual(6, battleFought.Id);
    }

    [TestMethod]
    public void Constructor_MultipleSyntheticEvents_RemainSortedAndCollisionFree()
    {
        // Arrange: a gapped event list (Count = 3, highest Id = 9) plus a caller that appends each
        // synthetic event like Battle does. The resulting list must stay strictly ascending so
        // World.GetEvent's binary search keeps resolving events correctly.
        var events = new List<LegendsViewer.Backend.Legends.Events.WorldEvent>
        {
            new(new List<LegendsViewer.Backend.Legends.Parser.Property>(), _mockWorld.Object) { Id = 0 },
            new(new List<LegendsViewer.Backend.Legends.Parser.Property>(), _mockWorld.Object) { Id = 4 },
            new(new List<LegendsViewer.Backend.Legends.Parser.Property>(), _mockWorld.Object) { Id = 9 },
        };
        _mockWorld.Setup(w => w.Events).Returns(events);

        // Act: create and append three synthetic events, mirroring Battle's construction loop.
        for (int i = 0; i < 3; i++)
        {
            var bf = new BattleFought(_historicalFigure, _battle, _mockWorld.Object, asAttacker: true);
            events.Add(bf);
        }

        // Assert: ids strictly ascending and unique across the whole list.
        for (int i = 1; i < events.Count; i++)
        {
            Assert.IsTrue(events[i].Id > events[i - 1].Id,
                $"Event at index {i} (Id {events[i].Id}) must be greater than previous (Id {events[i - 1].Id}).");
        }
        Assert.AreEqual(events.Count, events.Select(e => e.Id).Distinct().Count(), "Event Ids must be unique.");
    }
}
