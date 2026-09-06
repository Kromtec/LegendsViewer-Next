using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Parser;

namespace LegendsViewer.Backend.Tests.Legends.Events;

[TestClass]
public class EventFilterTests
{
    [TestMethod]
    public void MatchesFilterCriteria_NullOrEmptyFilter_ReturnsTrue()
    {
        var evt = new WorldEvent([new Property { Name = "type", Value = "masterpiece item" }], null);
        Assert.IsTrue(evt.MatchesFilterCriteria(null));
        Assert.IsTrue(evt.MatchesFilterCriteria(new EventFilterDto()));
    }

    [TestMethod]
    public void MatchesFilterCriteria_ExcludedEventType_ReturnsFalse()
    {
        var evt = new WorldEvent([new Property { Name = "type", Value = "masterpiece item" }], null);
        var filter = new EventFilterDto
        {
            ExcludedEventTypes = ["masterpiece item"]
        };
        Assert.IsFalse(evt.MatchesFilterCriteria(filter));
    }

    [TestMethod]
    public void MatchesFilterCriteria_NonExcludedEventType_ReturnsTrue()
    {
        var evt = new WorldEvent([new Property { Name = "type", Value = "hf died" }], null);
        var filter = new EventFilterDto
        {
            ExcludedEventTypes = ["masterpiece item"]
        };
        Assert.IsTrue(evt.MatchesFilterCriteria(filter));
    }
}
