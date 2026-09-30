using LegendsViewer.Backend.Legends.Maps;

namespace LegendsViewer.Backend.Tests;

[TestClass]
public class PremiumWorldMapTests
{
    [TestMethod]
    public void SelectsBiomeAndAlignmentSprite()
    {
        Assert.AreEqual("GRASSLAND_TEMP", WorldMapImageGenerator.GetPremiumSpriteToken(18, 50, 80, 100, 0, 0, 0));
        Assert.AreEqual("GRASSLAND_TEMP_EVILSAV", WorldMapImageGenerator.GetPremiumSpriteToken(18, 80, 80, 100, 0, 0, 0));
        Assert.AreEqual("FOREST_TAIGA_GOODSAV", WorldMapImageGenerator.GetPremiumSpriteToken(12, 10, 80, 120, 0, 0, 0));
        Assert.AreEqual("FOREST_BROADLEAF_TROP_MOIST", WorldMapImageGenerator.GetPremiumSpriteToken(17, 50, 50, 120, 0, 0, 0));
        Assert.AreEqual("MOUNTAIN_LOW", WorldMapImageGenerator.GetPremiumSpriteToken(0, 50, 50, 199, 0, 0, 0));
        Assert.AreEqual("MOUNTAIN_MID", WorldMapImageGenerator.GetPremiumSpriteToken(0, 50, 50, 200, 0, 0, 0));
        Assert.AreEqual("MOUNTAIN_HIGH", WorldMapImageGenerator.GetPremiumSpriteToken(0, 50, 50, 250, 0, 0, 0));
        Assert.AreEqual("MOUNTAIN_PEAK_EVIL", WorldMapImageGenerator.GetPremiumSpriteToken(0, 80, 10, 220, 0, 0, 1));
        Assert.AreEqual("VOLCANO", WorldMapImageGenerator.GetPremiumSpriteToken(0, 50, 50, 220, 0, 100, 0));
        Assert.AreEqual("VOLCANO", WorldMapImageGenerator.GetPremiumSpriteToken(18, 50, 50, 120, 0, 0, 2));
        Assert.AreEqual("LAKE_GOOD", WorldMapImageGenerator.GetPremiumSpriteToken(18, 10, 10, 100, 1, 0, 0));
    }

    [TestMethod]
    public void MapsEveryDirectionMaskToGraphicsToken()
    {
        string[] expected = ["0", "N", "S", "NS", "W", "NW", "SW", "NSW",
            "E", "NE", "SE", "NSE", "WE", "NWE", "SWE", "NSWE"];
        for (int mask = 0; mask < expected.Length; mask++)
            Assert.AreEqual(expected[mask], WorldMapImageGenerator.GetDirectionSuffix(mask));
    }

    [TestMethod]
    public void ConnectivityDoesNotReadPastMapBoundaries()
    {
        var visited = new List<(int X, int Y)>();
        int directions = WorldMapImageGenerator.GetConnectedDirections(2, 2, 0, 0, (x, y) =>
        {
            visited.Add((x, y));
            return true;
        });
        Assert.AreEqual(10, directions);
        CollectionAssert.AreEquivalent(new[] { (0, 1), (1, 0) }, visited);
    }
}
