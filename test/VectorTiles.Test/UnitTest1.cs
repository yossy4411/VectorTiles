using System.Diagnostics;
using VectorTiles.Styles.MapboxGL;

namespace VectorTiles.Test;

public class Tests
{
    [Test]
    public async ValueTask StyleJson()
    {
        var text = await File.ReadAllTextAsync("basic.json"); // Read the JSON file
        var styles = VectorMapStyleGL.LoadGLJson(text);
        
        Assert.That(styles, Is.Not.Null);
        Dictionary<string, object?> values = new();
        
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
        // [">=", ["get", "vt_lvorder"], 4]
        layer = styles.Layers.First(x => x.Id == "建築物の外周線4");
        values["vt_lvorder"] = 4;
        Assert.That(layer.IsVisible(values), Is.True);
        values["vt_lvorder"] = 3;
        Assert.That(layer.IsVisible(values), Is.False);
        values["vt_lvorder"] = 5;
        Assert.That(layer.IsVisible(values), Is.True);
        
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
        // ["any", ["==", ["get", "vt_lvorder"], 0], ["==", ["get", "vt_lvorder"], 1]]
        // But it is not found in the JSON file, skipping this test.
        
        // None filter check
        // ["none", ["==", ["get", "vt_lvorder"], 0], ["==", ["get", "vt_lvorder"], 1]]
        // But it is not found in the JSON file, skipping this test.
        
        // Step filter check
        // ["step", ["zoom"],
        //  ["all", ["==", ["get", "vt_lvorder"], 2], ["!", ["in", ["get", "vt_code"], ["literal", [2703, 2704, ...]], ["!", ["all", ["in", ["get", "vt_rdctg"], ["literal", ["市区町村道等", ...]]], ...]]],
        //  14, ["all", ["==", ["get", "vt_lvorder"], 2], ["!", ["in", ["get", "vt_code"], ["literal", [2703, 2704, ...]]]],
        layer = styles.Layers.First(x => x.Id == "道路中心線ククリ2");
        values.Clear();
        values["$zoom"] = 14f;
        values["vt_lvorder"] = 2;
        values["vt_code"] = 2701;
        Assert.That(layer.IsVisible(values), Is.True);
        values["$zoom"] = 11f;
        Assert.That(layer.IsVisible(values), Is.True);
        values["vt_rdctg"] = "市区町村道等";
        Assert.That(layer.IsVisible(values), Is.False);
        values["vt_rdctg"] = "国道等";
        Assert.That(layer.IsVisible(values), Is.True);
    }
}