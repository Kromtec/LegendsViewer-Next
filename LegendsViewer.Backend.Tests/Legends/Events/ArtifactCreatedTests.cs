using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using Moq;

namespace LegendsViewer.Backend.Tests.Legends.Events;

[TestClass]
public class ArtifactCreatedTests
{
    private Mock<IWorld> _mockWorld = null!;
    private Artifact _artifact = null!;
    private HistoricalFigure _historicalFigure = null!;
    private Site _site = null!;
    private Entity _entity = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockWorld = new Mock<IWorld>();
        _mockWorld.Setup(w => w.ParsingErrors).Returns(new ParsingErrors());

        _artifact = new Artifact([], _mockWorld.Object)
        {
            Id = 1,
            Name = "Test Artifact",
            Icon = "artifact"
        };

        _historicalFigure = new HistoricalFigure
        {
            Id = 1,
            Name = "Test Creator",
            Icon = "person"
        };

        _site = new Site([], _mockWorld.Object)
        {
            Id = 1,
            Name = "Test Site",
            Type = "TOWER"
        };
        _site.Structures = [];

        _entity = new Entity([], _mockWorld.Object)
        {
            Id = 1,
            Name = "Test Entity",
            Icon = "civilization"
        };
        _entity.Honors = [];

        _mockWorld.Setup(w => w.GetArtifact(1)).Returns(_artifact);
        _mockWorld.Setup(w => w.GetHistoricalFigure(1)).Returns(_historicalFigure);
        _mockWorld.Setup(w => w.GetSite(1)).Returns(_site);
        _mockWorld.Setup(w => w.GetEntity(1)).Returns(_entity);
    }

    [TestMethod]
    public void Constructor_WithValidProperties_ParsesCorrectly()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.IsNotNull(artifactCreated);
        Assert.AreEqual(_artifact, artifactCreated.Artifact);
        Assert.AreEqual(_historicalFigure, artifactCreated.HistoricalFigure);
    }

    [TestMethod]
    public void Constructor_WithSite_ParsesSite()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" },
            new Property { Name = "site_id", Value = "1" }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(_site, artifactCreated.Site);
    }

    [TestMethod]
    public void Constructor_WithNameOnly_SetsReceivedName()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "name_only", Value = "true" }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.IsTrue(artifactCreated.ReceivedName);
    }

    [TestMethod]
    public void Constructor_WithSanctifyFigure_ParsesSanctifyFigure()
    {
        // Arrange
        var sanctifyFigure = new HistoricalFigure
        {
            Id = 2,
            Name = "Sanctify Figure",
            Icon = "person"
        };
        _mockWorld.Setup(w => w.GetHistoricalFigure(2)).Returns(sanctifyFigure);

        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "sanctify_hf", Value = "2" }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(sanctifyFigure, artifactCreated.SanctifyFigure);
    }

    [TestMethod]
    public void Constructor_WithEntity_ParsesEntity()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "entity_id", Value = "1" }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(_entity, artifactCreated.Entity);
    }

    [TestMethod]
    public void Constructor_WithReasonSanctify_SetsReason()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "reason", Value = "sanctify_hf" }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(ArtifactReason.SanctifyHistoricalFigure, artifactCreated.Reason);
    }

    [TestMethod]
    public void Constructor_AddsEventToArtifact()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" }
        };
        var initialEventCount = _artifact.Events.Count;

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(initialEventCount + 1, _artifact.Events.Count);
    }

    [TestMethod]
    public void Constructor_AddsEventToHistoricalFigure()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" }
        };
        var initialEventCount = _historicalFigure.Events.Count;

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(initialEventCount + 1, _historicalFigure.Events.Count);
    }

    [TestMethod]
    public void Constructor_WithCircumstanceDefeated_ParsesDefeatedFigure()
    {
        // Arrange
        var defeatedFigure = new HistoricalFigure
        {
            Id = 3,
            Name = "Defeated Figure",
            Icon = "person"
        };
        _mockWorld.Setup(w => w.GetHistoricalFigure(3)).Returns(defeatedFigure);

        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "defeated" },
                    new Property { Name = "defeated", Value = "3" }
                }
            }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(Circumstance.DefeatedHf, artifactCreated.Circumstance);
        Assert.AreEqual(defeatedFigure, artifactCreated.DefeatedFigure);
    }

    [TestMethod]
    public void Constructor_WithCircumstanceConflict_ParsesCorrectly()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "conflict" }
                }
            }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(Circumstance.Conflict, artifactCreated.Circumstance);
    }

    [TestMethod]
    public void Constructor_WithCircumstanceTrauma_ParsesCorrectly()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "trauma" }
                }
            }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(Circumstance.Trauma, artifactCreated.Circumstance);
    }

    [TestMethod]
    public void Constructor_WithCircumstanceFavoritePossession_ParsesCorrectly()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "favoritepossession" }
                }
            }
        };

        // Act
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Assert
        Assert.AreEqual(Circumstance.FavoritePossession, artifactCreated.Circumstance);
    }

    [TestMethod]
    public void Print_WithAllProperties_ReturnsCorrectFormat()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" },
            new Property { Name = "site_id", Value = "1" }
        };
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Act
        var result = artifactCreated.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("Test Artifact"));
        Assert.IsTrue(result.Contains("Test Creator"));
        Assert.IsTrue(result.Contains("Test Site"));
        Assert.IsTrue(result.Contains("was created"));
        Assert.IsTrue(result.Contains("in"));
    }

    [TestMethod]
    public void Print_WithReceivedName_ReturnsCorrectFormat()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" },
            new Property { Name = "name_only", Value = "true" }
        };
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Act
        var result = artifactCreated.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("received its name"));
    }

    [TestMethod]
    public void Print_WithSanctifyFigure_ReturnsCorrectFormat()
    {
        // Arrange
        var sanctifyFigure = new HistoricalFigure
        {
            Id = 2,
            Name = "Sanctify Figure",
            Icon = "person"
        };
        _mockWorld.Setup(w => w.GetHistoricalFigure(2)).Returns(sanctifyFigure);

        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" },
            new Property { Name = "reason", Value = "sanctify_hf" },
            new Property { Name = "sanctify_hf", Value = "2" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "preservebody" }
                }
            }
        };
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Act
        var result = artifactCreated.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("sanctify"));
        Assert.IsTrue(result.Contains("Sanctify Figure"));
        Assert.IsTrue(result.Contains("preserving a part of the body"));
    }

    [TestMethod]
    public void Print_WithSanctifyFigure_FavoritePossession_DoesNotContainPreserveBody()
    {
        // Arrange
        var sanctifyFigure = new HistoricalFigure
        {
            Id = 2,
            Name = "Sanctify Figure",
            Icon = "person"
        };
        _mockWorld.Setup(w => w.GetHistoricalFigure(2)).Returns(sanctifyFigure);

        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" },
            new Property { Name = "reason", Value = "sanctify_hf" },
            new Property { Name = "sanctify_hf", Value = "2" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "favoritepossession" }
                }
            }
        };
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Act
        var result = artifactCreated.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("sanctify"));
        Assert.IsTrue(result.Contains("Sanctify Figure"));
        Assert.IsTrue(result.Contains("favorite possession"));
        Assert.IsFalse(result.Contains("preserving a part of the body"));
    }

    [TestMethod]
    public void Print_WithDefeatedFigure_ReturnsCorrectFormat()
    {
        // Arrange
        var defeatedFigure = new HistoricalFigure
        {
            Id = 3,
            Name = "Defeated Figure",
            Icon = "person"
        };
        _mockWorld.Setup(w => w.GetHistoricalFigure(3)).Returns(defeatedFigure);

        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" },
            new Property
            {
                Name = "circumstance",
                SubProperties = new List<Property>
                {
                    new Property { Name = "type", Value = "defeated" },
                    new Property { Name = "defeated", Value = "3" }
                }
            }
        };
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Act
        var result = artifactCreated.Print(link: true);

        // Assert
        Assert.IsTrue(result.Contains("after defeating"));
        Assert.IsTrue(result.Contains("Defeated Figure"));
    }

    [TestMethod]
    public void Print_WithoutLink_ReturnsPlainText()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property { Name = "artifact_id", Value = "1" },
            new Property { Name = "hist_figure_id", Value = "1" }
        };
        var artifactCreated = new ArtifactCreated(properties, _mockWorld.Object);

        // Act
        var result = artifactCreated.Print(link: false);

        // Assert
        Assert.IsTrue(result.Contains("was created"));
        Assert.IsTrue(result.Contains("Test Creator"));
    }
}
