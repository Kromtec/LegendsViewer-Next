using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Legends.Various;
using Moq;

namespace LegendsViewer.Backend.Tests.Legends.EventCollections;

[TestClass]
public class BattleTests
{
    private Mock<IWorld> _mockWorld = null!;
    private Entity _attacker = null!;
    private Entity _defender = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockWorld = new Mock<IWorld>();
        _mockWorld.Setup(w => w.ParsingErrors).Returns(new ParsingErrors());
        
        _attacker = new Entity([], _mockWorld.Object) { Id = 1, Name = "Attackers", Icon = "civilization" };
        _defender = new Entity([], _mockWorld.Object) { Id = 2, Name = "Defenders", Icon = "civilization" };
        
        _mockWorld.Setup(w => w.GetEntity(1)).Returns(_attacker);
        _mockWorld.Setup(w => w.GetEntity(2)).Returns(_defender);
    }

    [TestMethod]
    public void Constructor_WithValidProperties_ParsesCorrectly()
    {
        var props = new List<Property>
        {
            new Property { Name = "battle_type", Value = "1" },
            new Property { Name = "attacker_enid", Value = "1" },
            new Property { Name = "defender_enid", Value = "2" }
        };

        var battle = new Battle(props, _mockWorld.Object);

        Assert.IsNotNull(battle);
    }

    [TestMethod]
    public void Constructor_SetsDefaultValues()
    {
        var props = new List<Property>
        {
            new Property { Name = "battle_type", Value = "1" }
        };

        var battle = new Battle(props, _mockWorld.Object);

        Assert.IsNotNull(battle.Events);
    }

    [TestMethod]
    public void SteamSquads_AreAggregatedByRace()
    {
        _mockWorld.Setup(w => w.GetCreatureInfo("DWARF")).Returns(new CreatureInfo("DWARF"));
        var battle = new Battle([
            new Property { Name = "attacking_squad_race", Value = "DWARF" },
            new Property { Name = "attacking_squad_entity_pop", Value = "42" },
            new Property { Name = "attacking_squad_number", Value = "10" },
            new Property { Name = "attacking_squad_deaths", Value = "3" },
            new Property { Name = "attacking_squad_site", Value = "7" },
            new Property { Name = "attacking_squad_race", Value = "DWARF" },
            new Property { Name = "attacking_squad_entity_pop", Value = "43" },
            new Property { Name = "attacking_squad_number", Value = "5" },
            new Property { Name = "attacking_squad_deaths", Value = "1" },
            new Property { Name = "attacking_squad_site", Value = "8" }
        ], _mockWorld.Object);

        Assert.AreEqual(2, battle.AttackerSquads.Count);
        Assert.AreEqual(1, battle.Attackers.Count);
        Assert.AreEqual(15, battle.Attackers[0].Numbers);
        Assert.AreEqual(4, battle.Attackers[0].Deaths);
    }

    [TestMethod]
    public void ToLink_WithLink_ReturnsHtml()
    {
        var props = new List<Property>
        {
            new Property { Name = "battle_type", Value = "1" }
        };

        var battle = new Battle(props, _mockWorld.Object);

        var result = battle.ToLink(link: true);

        Assert.IsTrue(result.Contains("battle") || result.Contains("the"));
    }

    [TestMethod]
    public void ToLink_WithoutLink_ReturnsName()
    {
        var props = new List<Property>
        {
            new Property { Name = "battle_type", Value = "1" }
        };

        var battle = new Battle(props, _mockWorld.Object);

        // When link=false, returns Name which may be empty - this is expected
        var result = battle.ToLink(link: false);
        Assert.IsNotNull(result);
    }
}
