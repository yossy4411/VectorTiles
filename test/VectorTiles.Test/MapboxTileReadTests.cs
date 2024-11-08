using PMTiles;
using VectorTiles.Mvt;
using VectorTiles.Styles;
using VectorTiles.Styles.MapboxGL;

namespace VectorTiles.Test;

public class MapboxTileReadTests
{
    [Test]
    public async ValueTask ReadTest()
    {
        var tiles = await PMTilesReader.FromUrl(
            "https://cyberjapandata.gsi.go.jp/xyz/optimal_bvmap-v1/optimal_bvmap-v1.pmtiles");
        Assert.That(tiles, Is.Not.Null);
        var text = await File.ReadAllTextAsync("basic.json"); // Read the JSON file
        var style = MapboxStyle.LoadGLJson(text);
        await using var stream = await tiles.GetTileZxy(10, 909, 403); // tokyo
        Assert.That(stream, Is.Not.Null);
        var tile = MapboxTileReader.Read(stream, 10, 909, 403, style);
        Assert.That(tile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tile.Layers, Is.Not.Null);
            Assert.That(tile.Layers, Is.Not.Empty);
            Assert.That(tile.Layers[0].Features, Is.Not.Null);
            Assert.That(tile.Layers[0].Features, Is.Not.Empty);
        });

        style = new VectorMapStyle();  // empty style
        await using var stream2 = await tiles.GetTileZxy(10, 909, 403); // tokyo
        tile = MapboxTileReader.Read(stream2, 10, 909, 403, style);
        Assert.That(tile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tile.Layers, Is.Not.Null);
            Assert.That(tile.Layers, Is.Empty);
        });
    }
}