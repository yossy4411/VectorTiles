using System.Text;
using VectorTiles.Styles.MapboxGL;

namespace VectorTiles.Test;

public class Tests
{
    private HttpClient _client;
    [SetUp]
    public void Setup()
    {
        _client = new HttpClient();
    }

    [Test]
    public async ValueTask Test1()
    {
        var text = await File.ReadAllTextAsync("basic.json"); // Read the JSON file
        var styles = VectorMapStyleGL.LoadGLJson(text);
        Assert.That(styles, Is.Not.Null);
        Dictionary<string, object> values = new();
        
        // Equal filter check
        // ["==", ["get", "vt_code"], 5322]
        var layer = styles.Layers.First(x => x.Id == "河川中心線人工水路地下");
        values["vt_code"] = 5322;
        Assert.That(layer.IsVisible(values), Is.True);
        values["vt_code"] = 5323;
        Assert.That(layer.IsVisible(values), Is.False);
        
        // In filter check
        // ["in", ["get", "vt_code"], ["literal", [5101, 5103]]]
        layer = styles.Layers.First(x => x.Id == "海岸線");
        values["vt_code"] = 5101;
        Assert.That(layer.IsVisible(values), Is.True);
        values["vt_code"] = 5102;
        Assert.That(layer.IsVisible(values), Is.False);
        values["vt_code"] = 5103;
        Assert.That(layer.IsVisible(values), Is.True);
        
        // Not in filter check
        // ["!", ["in", ["get", "vt_code"], ["literal", [5302, 5322]]]]
        layer = styles.Layers.First(x => x.Id == "河川中心線");
        values["vt_code"] = 5301;
        Assert.That(layer.IsVisible(values), Is.True);
        values["vt_code"] = 5302;
        Assert.That(layer.IsVisible(values), Is.False);
        values["vt_code"] = 5322;
        Assert.That(layer.IsVisible(values), Is.False);
        
        // Comparator filter check
        // ["=>", ["get", "vt_code"], 5302]
        // But it is not found in the JSON file, skipping this test.
        
        // All filter check
        // ["all", ["!", ["in", ["get", "vt_railstate"], ["literal", ["トンネル", "雪覆い", "地下", "橋・高架"]]]], ["==", ["get", "vt_lvorder"], 0]]
        layer = styles.Layers.First(x => x.Id == "鉄道中心線0");
        values["vt_railstate"] = "地上";
        values["vt_lvorder"] = 0;
        Assert.That(layer.IsVisible(values), Is.True);
        values["vt_railstate"] = "トンネル";
        Assert.That(layer.IsVisible(values), Is.False);
        values["vt_railstate"] = "地上";
        values["vt_lvorder"] = 1;
        Assert.That(layer.IsVisible(values), Is.False);
        
        // Any filter check
        
    }
    
    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }
}