using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Tests.Legends;

[TestClass]
public class EntityMonarchTimelineTests
{
    [TestMethod]
    public void Test_GetLeaderTimelines_WithLeadersList_ReturnsTimeline()
    {
        // Arrange
        var entity = new Entity([], null!)
        {
            Id = 1,
            Name = "The Steel Citadel"
        };
        entity.LeaderTypes.Add("Monarch");

        var king1 = new HistoricalFigure([], null!)
        {
            Id = 10,
            Name = "King Urist I",
            Caste = "Male",
            BirthYear = 10,
            DeathYear = 50
        };
        king1.StartPositionAssignment(entity, 20, 0, "Monarch");

        var queen2 = new HistoricalFigure([], null!)
        {
            Id = 11,
            Name = "Queen Vira I",
            Caste = "Female",
            BirthYear = 30,
            DeathYear = 90
        };
        queen2.StartPositionAssignment(entity, 50, 0, "Monarch");

        entity.Leaders.Add([king1, queen2]);

        // Act
        List<LeaderTimelineDto> timelines = entity.GetLeaderTimelines();

        // Assert
        Assert.AreEqual(1, timelines.Count);
        Assert.AreEqual("Monarch", timelines[0].LeaderType);

        var leaders = timelines[0].Leaders;
        Assert.AreEqual(2, leaders.Count);

        Assert.AreEqual(11, leaders[0].Id);
        Assert.AreEqual("Queen Vira I", leaders[0].Name);
        Assert.AreEqual(50, leaders[0].StartYear);
        Assert.AreEqual(90, leaders[0].EndYear);
        Assert.AreEqual("Successor to King Urist I", leaders[0].PredecessorRelation);

        Assert.AreEqual(10, leaders[1].Id);
        Assert.AreEqual("King Urist I", leaders[1].Name);
        Assert.AreEqual(20, leaders[1].StartYear);
        Assert.AreEqual(50, leaders[1].EndYear);
        Assert.AreEqual("Founder", leaders[1].PredecessorRelation);
    }
}
