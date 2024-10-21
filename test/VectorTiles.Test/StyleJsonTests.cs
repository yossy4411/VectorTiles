using System.Drawing;
using NUnit.Framework.Constraints;
using VectorTiles.Styles;
using VectorTiles.Styles.MapboxGL;
using VectorTiles.Values;

namespace VectorTiles.Test;

public class StyleJsonTests
{
    private VectorMapStyle _style;
    
    [SetUp]
    public async Task Setup()
    {
        var text = await File.ReadAllTextAsync("basic.json"); // Read the JSON file
        _style = VectorMapStyleGL.LoadGLJson(text);
    }
    
    [Test]
    public void LoadTest()
    {
        Assert.That(_style, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(_style.Layers, Is.Not.Null);
            Assert.That(_style.Layers, Is.Not.Empty);
            Assert.That(_style.Sources, Is.Not.Null);
        });
        Assert.That(_style.Sources, Is.Not.Empty);
        var source = _style.Sources[0];
        Assert.Multiple(() =>
        {
            Assert.That(source.Url, new StartsWithConstraint("pmtiles://"));
            Assert.That(source.Type, Is.EqualTo("vector"));
            Assert.That(source.Attribution, Is.EqualTo("国土地理院最適化ベクトルタイル"));
        });

    }
    
    [Test]
    public void VisibleTest()
    {
        
        Assert.That(_style, Is.Not.Null);
        Dictionary<string, IConstValue?> values = new();
        Assert.Multiple(() =>
        {
            // Equal filter check
            // ["==", ["get", "vt_code"], 5322]
            Assert.Multiple(() =>
            {
                var layer = _style.Layers.First(x => x.Id == "河川中心線人工水路地下");
                values["vt_code"] = new ConstIntValue(5322);
                Assert.That(layer.IsVisible(values), Is.True);
                values["vt_code"] = new ConstIntValue(5323);
                Assert.That(layer.IsVisible(values), Is.False);
            });
            // In filter check
            // ["in", ["get", "vt_code"], ["literal", [5101, 5103]]]
            Assert.Multiple(() =>
            {
                var layer = _style.Layers.First(x => x.Id == "海岸線");
                values["vt_code"] = new ConstIntValue(5101);
                Assert.That(layer.IsVisible(values), Is.True);
                values["vt_code"] = new ConstIntValue(5102);
                Assert.That(layer.IsVisible(values), Is.False);
                values["vt_code"] = new ConstIntValue(5103);
                Assert.That(layer.IsVisible(values), Is.True);
            });
            // Not in filter check
            // ["!", ["in", ["get", "vt_code"], ["literal", [5302, 5322]]]]
            Assert.Multiple(() =>
            {
                var layer = _style.Layers.First(x => x.Id == "河川中心線");
                values["vt_code"] = new ConstIntValue(5301);
                Assert.That(layer.IsVisible(values), Is.True);
                values["vt_code"] = new ConstIntValue(5302);
                Assert.That(layer.IsVisible(values), Is.False);
                values["vt_code"] = new ConstIntValue(5322);
                Assert.That(layer.IsVisible(values), Is.False);
            });

            // Comparator filter check
            // [">=", ["get", "vt_lvorder"], 4]
            Assert.Multiple(() =>
            {
                var layer = _style.Layers.First(x => x.Id == "建築物の外周線4");
                values["vt_lvorder"] = new ConstIntValue(4);
                Assert.That(layer.IsVisible(values), Is.True);
                values["vt_lvorder"] = new ConstIntValue(3);
                Assert.That(layer.IsVisible(values), Is.False);
                values["vt_lvorder"] = new ConstIntValue(5);
                Assert.That(layer.IsVisible(values), Is.True);
            });

            // All filter check
            // ["all", ["!", ["in", ["get", "vt_railstate"], ["literal", ["トンネル", "雪覆い", "地下", "橋・高架"]]]], ["==", ["get", "vt_lvorder"], 0]]
            Assert.Multiple(() =>
            {
                var layer = _style.Layers.First(x => x.Id == "鉄道中心線0");
                values["vt_railstate"] = new ConstStringValue("地上");
                values["vt_lvorder"] = new ConstIntValue(0);
                Assert.That(layer.IsVisible(values), Is.True);
                values["vt_railstate"] = new ConstStringValue("トンネル");
                Assert.That(layer.IsVisible(values), Is.False);
                values["vt_railstate"] = new ConstStringValue("地上");
                values["vt_lvorder"] = new ConstIntValue(1);
                Assert.That(layer.IsVisible(values), Is.False);
            });

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
            Assert.Multiple(() =>
            {
                var layer = _style.Layers.First(x => x.Id == "道路中心線ククリ2");
                values.Clear();
                values["$zoom"] = new ConstFloatValue(14);
                values["vt_lvorder"] = new ConstIntValue(2);
                values["vt_code"] = new ConstIntValue(2701);
                Assert.That(layer.IsVisible(values), Is.True);
                values["$zoom"] = new ConstFloatValue(11);
                Assert.That(layer.IsVisible(values), Is.True);
                values["vt_rdctg"] = new ConstStringValue("市区町村道等");
                Assert.That(layer.IsVisible(values), Is.False);
                values["vt_rdctg"] = new ConstStringValue("国道等");
                Assert.That(layer.IsVisible(values), Is.True);
            });
        });

    }

    [Test]
    public void PropertyTest()
    {
        Assert.That(_style, Is.Not.Null);
        Dictionary<string, IConstValue?> values = new();
        
        
        
        // Interpolate property check
        // ["interpolate",["linear"],["zoom"],15,["match",["get","vt_code"],5321,0.5,1], 16, ["match",["get","vt_code"],5321,2,1]]
        var layer = _style.Layers.First(x => x.Id == "河川中心線");
        Assert.That(layer, Is.InstanceOf<VectorLineStyleLayer>());
        var fillLayer = (VectorLineStyleLayer) layer;
        values["$zoom"] = new ConstFloatValue(15);
        values["vt_code"] = new ConstIntValue(5321);
        Assert.That(fillLayer.LineWidth, Is.Not.Null);
        Assert.That(fillLayer.LineWidth.GetValue(values), Is.EqualTo(0.5f));
        values["vt_code"] = new ConstIntValue(5322);
        Assert.That(fillLayer.LineWidth.GetValue(values), Is.EqualTo(1f));
        values["$zoom"] = new ConstFloatValue(16);
        values["vt_code"] = new ConstIntValue(5321);
        Assert.That(fillLayer.LineWidth.GetValue(values), Is.EqualTo(2f));
        
        // Match property check
        // ["match", ["get","vt_code"], [5203,5233], "rgba(120, 120, 120, 1)", "rgba(20, 90, 255, 1)"]
        layer = _style.Layers.First(x => x.Id == "水涯線");
        Assert.That(layer, Is.InstanceOf<VectorLineStyleLayer>());
        fillLayer = (VectorLineStyleLayer) layer;
        values["vt_code"] = new ConstIntValue(5203);
        Assert.That(fillLayer.LineColor, Is.Not.Null);
        Assert.That(fillLayer.LineColor.GetValue(values), Is.EqualTo(Color.FromArgb(255, 120, 120, 120)));
        
    }
}