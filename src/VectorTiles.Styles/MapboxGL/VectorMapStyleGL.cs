using System.Diagnostics;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VectorTiles.Styles.Values;

namespace VectorTiles.Styles.MapboxGL;

public static class VectorMapStyleGL
{
    public static VectorMapStyle LoadGLJson(string text)
    {
        var jObject = JsonConvert.DeserializeObject<JObject>(text);
        if (jObject?["layers"] is not JArray layers) return new VectorMapStyle();
        var layersList = layers.Select(NewLayer).OfType<VectorMapStyleLayer>().ToList();
        var name = jObject["name"]?.ToObject<string>();
        if (jObject["sources"] is not JObject sources)
            return new VectorMapStyle
            {
                Name = name,
                Layers = layersList
            };
        List<VectorMapSource> sourceList = new();
        
        foreach (var (_, value) in sources)
        {
            if (value is not JObject) continue;
            var source = new VectorMapSource
            {
                Type = value["type"]?.ToObject<string>() ?? "vector",
                MinZoom = value["minzoom"]?.ToObject<uint>() ?? 0,
                MaxZoom = value["maxzoom"]?.ToObject<uint>() ?? 22,
                Url = value["tiles"]?.FirstOrDefault()?.ToObject<string>(),
                Attribution = value["attribution"]?.ToObject<string>()
            };
            sourceList.Add(source);
        }

        return new VectorMapStyle
        {
            Name = name,
            Layers = layersList,
            Sources = sourceList
        };
    }
    
    private static Color ParseColor(string? value)
    {
        if (value is null) return Color.Empty;
        if (value.StartsWith('#')) return Color.FromArgb(int.Parse(value[1..], System.Globalization.NumberStyles.HexNumber));

        if (value.StartsWith("rgba"))
        {
            // ex. rgba(40,20,100,0.8)
            var values = value[5..^1].Split(',');
            return Color.FromArgb((byte)(float.Parse(values[3]) * 255), byte.Parse(values[0]), byte.Parse(values[1]),
                byte.Parse(values[2]));
        }

        if (value.StartsWith("rgb"))
        {
            // ex. rgb(40,20,100)
            var values = value[4..^1].Split(',');
            return Color.FromArgb(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]));
        }

        if (value.StartsWith("hsla"))
        {
            // ex. hsla(120,100%,50%,0.8)
            var values = value[5..^1].Split(',');
            var a = (byte)(float.Parse(values[3]) * 255);
            return FromHsl(float.Parse(values[0]), float.Parse(values[1].TrimEnd('%')) / 100,
                float.Parse(values[2].TrimEnd('%')) / 100, a);
        }
        
        if (value.StartsWith("hsl"))
        {
            // ex. hsl(120,100%,50%)
            var values = value[4..^1].Split(',');
            return FromHsl(float.Parse(values[0]), float.Parse(values[1].TrimEnd('%')) / 100,
                float.Parse(values[2].TrimEnd('%')) / 100);
        }
        
        Debug.WriteLine("Unknown color format: " + value);
        return Color.Empty;
    }

    private static Color FromHsl(float hue, float saturation, float lightness, byte alpha = 255)
    {
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        var m = lightness - c / 2;

        var (r, g, b) = hue switch
        {
            >= 0 and < 60 => new ValueTuple<double, double, double>(c, x, 0),
            >= 60 and < 120 => (x, c, 0),
            >= 120 and < 180 => (0, c, x),
            >= 180 and < 240 => (0, x, c),
            >= 240 and < 300 => (x, 0, c),
            _ => (c, 0, x),
        };

        // RGBを0-255の範囲に変換
        var r1 = (byte)((r + m) * 255);
        var r2 = (byte)((g + m) * 255);
        var r3 = (byte)((b + m) * 255);

        return Color.FromArgb(alpha, r1, r2, r3);
    }
    
    private static VectorMapStyleLayer? NewLayer(JToken jToken)
    {
        var vMapFilter = GetFilter(jToken);
        var source = jToken["source-layer"]?.ToObject<string>()!;
        var paintToken = jToken["paint"];
        var minZoom = jToken["minzoom"]?.ToObject<int>() ?? 0;
        var maxZoom = jToken["maxzoom"]?.ToObject<int>() ?? 22;
        var id = jToken["id"]?.ToObject<string>()!;
        switch (jToken["type"]?.ToObject<string>())
        {
            case "fill":
            {
                var fillColorToken = paintToken!["fill-color"];
                var fillColor = ParseInterpolation(fillColorToken, new InterpolateSegmentColor(0, Color.White));
                return new VectorFillStyleLayer(source, vMapFilter)
                {
                    MinZoom = minZoom,
                    MaxZoom = maxZoom,
                    Id = id,
                    FillColor = fillColor
                };
            }
            case "line":
            {
                var lineColorToken = paintToken!["line-color"];
                var lineColor = ParseInterpolation(lineColorToken, new InterpolateSegmentColor(0, Color.White));
                var lineWidthToken = paintToken["line-width"];
                var lineWidth = ParseInterpolation(lineWidthToken, new InterpolateSegmentFloat(0, 1));
                var dashToken = paintToken["line-dasharray"];
                var dashToken1 = dashToken?[0];
                // {"line-dasharray": ["literal", [1, 2]]} or {"line-dasharray": [1, 2]} <- What's the difference?
                var dashArray = dashToken1 is null ? 
                    null : 
                    dashToken1.Type == JTokenType.String ? 
                        dashToken![1]?.ToObject<float[]>() : 
                        dashToken!.ToObject<float[]>();
                var dashArrayActual = dashArray is null || dashArray.Length % 2 != 0
                    ? null
                    : dashArray;
                return new VectorLineStyleLayer(source, vMapFilter)
                {
                    LineColor = lineColor,
                    LineWidth = lineWidth,
                    DashArray = dashArrayActual,
                    MinZoom = minZoom,
                    MaxZoom = maxZoom,
                    Id = id
                };
            }
            case "symbol":
            {
                var layoutToken = jToken["layout"];
                var textSize = ParseInterpolation(layoutToken?["text-size"], new InterpolateSegmentFloat(0, 12));
                var textColor = ParseInterpolation(layoutToken?["text-color"], new InterpolateSegmentColor(0, Color.Black));
                var fieldToken = layoutToken?["text-field"];
                var field = fieldToken is JArray
                    ? fieldToken[1]?.ToObject<string>()
                    : fieldToken?.ToObject<string>()?.Replace("{", "").Replace("}", "");
                var imageToken = layoutToken?["icon-image"];
                var iconImage = imageToken?.Type == JTokenType.String ? imageToken.ToObject<string>() : null;
                var iconSize = ParseInterpolation(layoutToken?["icon-size"], new InterpolateSegmentFloat(0, 1));
                return new VectorSymbolStyleLayer(source, vMapFilter)
                {
                    TextField = field,
                    MinZoom = minZoom,
                    MaxZoom = maxZoom,
                    TextSize = textSize,
                    TextColor = textColor,
                    IconImage = iconImage,
                    IconSize = iconSize,
                    Id = id
                };
            }
            case "background":
            {
                var backgroundColorToken = paintToken!["background-color"];
                var backgroundColor = ParseInterpolation(backgroundColorToken, new InterpolateSegmentColor(0, Color.White));
                return new VectorBackgroundStyleLayer
                {
                    MinZoom = minZoom,
                    MaxZoom = maxZoom,
                    Id = id,
                    BackgroundColor = backgroundColor
                };
            }
            default:
                return null;
        }
    }

    private static IStyleValues<T> ParseInterpolation<T>(JToken? tokenA, InterpolateSegment<T> defaultValue)
    {
        if (tokenA is not JObject li)
        {
            if (tokenA is not JArray)
                return new InterpolateSegments<T>(InterpolateType.Linear)
                {
                    defaultValue
                };
            // ["get", "<key>"]
            if (tokenA[0]?.ToObject<string>() != "get") return new InterpolateSegments<T>(InterpolateType.Linear)
            {
                defaultValue
            };
            var key = tokenA[1]?.ToObject<string>();
            var property = new KeyProperty()
            {
                Key = key
            };
            if (property is IStyleValues<T> values)
            {
                return values;
            }

            return new InterpolateSegments<T>(InterpolateType.Linear)
            {
                defaultValue
            };
        }

        var segments = new InterpolateSegments<T>(InterpolateType.Linear);
        if (li["stops"] is not JArray stops) return segments;
        foreach (var stop in stops)
        {
            if (stop is not JArray token) continue;
            if (token.Count < 2) continue;
            var zoom = token[0].ToObject<float>();
            var value = token[1];
            switch (value.Type)
            {
                case JTokenType.String:
                {
                    var seg = new InterpolateSegmentColor(zoom, ParseColor(value.ToObject<string>()));
                    if (seg is InterpolateSegment<T> seg1)
                    {
                        segments.Add(seg1);
                    }

                    break;
                }
                case JTokenType.Float:
                {
                    var seg = new InterpolateSegmentFloat(zoom, value.ToObject<float>());
                    if (seg is InterpolateSegment<T> seg1)
                    {
                        segments.Add(seg1);
                    }

                    break;
                }
                default:
                {
                    Debug.WriteLine("Unknown interpolation type: " + value.Type);
                    break;
                }
            }
        }
        return segments;

    }

    private static VectorMapFilter? GetFilter(JToken jToken)
    {
        if (jToken["filter"] is not JArray token) return null;
        if (token.Count < 2) return null;
        
        // new filter
        if (token[1] is not JArray)
        {
            return GetOneFilter(token);
        }
        
        // old filter
        var first = token[0].ToObject<string>();
        if (first == "step") return null; // not supported
        if ( first != "all" && first != "any" && first != "step")
            return GetOneFilter(token);

        return null; // implement later
        // 複数のフィルタ
        List<VectorMapFilter> filterList = new();
        for (var i = 1; i < token.Count; i++)
        {
            var token1 = (JArray)token[i];
            var filter = GetOneFilter(token1);
            if (filter is not null)
            {
                filterList.Add(filter);
            }
        }

        if (filterList.Count == 0) return _ => false;
        return first switch
        {
            "all" => dictionary => filterList.All(x => x(dictionary)),
            "any" => dictionary => filterList.Any(x => x(dictionary)),
            _ => null
        };
    }

    private static VectorMapFilter? GetOneFilter(JArray token)
    {
        var not = (token[0].Type == JTokenType.String ? token[0].ToObject<string>() : null) == "!";
        if (not)
            token = (JArray)token[1];
        var jToken = token[1];
        var key = jToken is JArray keyToken ?
            keyToken.Count > 1 ? 
                keyToken[1].ToObject<string>()
                : keyToken[0].ToObject<string>() switch
                {
                    "geometry-type" => "$type",
                    _ => null
                }
            : jToken.ToObject<string>();
        if (key is null) return null;
        var type = token[0].ToObject<string>();
        if (not)
        {
            key = key switch
            {
                "has" => "!has",
                "!has" => "has",
                "in" => "!in",
                "!in" => "in",
                "==" => "!=",
                "!=" => "==",
                _ => key
            }; // invert
        }
        
        switch (type)
        {
            case "==":
            {
                var value = ParseValue(token[2]);
                return dictionary => dictionary is not null && dictionary.TryGetValue(key, out var v) && Equal(value, v);
            }
            case "!=":
            {
                var value = ParseValue(token[2]);
                return dictionary => dictionary is not null && dictionary.TryGetValue(key, out var v) && !Equal(value, v);
            }
            case "in":
            {
                var value = token[2];
                var value0 = value[0];
                var values = value0?.Type is JTokenType.String && value0.ToObject<string>() == "literal" ? token[2][1]!.Select(ParseValue) : token.Skip(2).Select(ParseValue);
                return dictionary => dictionary is not null && dictionary.TryGetValue(key, out var v) && values.Any(x => Equal(x, v));
            }
            case "!in":
            {
                var values = token[2][0]?.Type is JTokenType.String ? token[2].Skip(1).Select(ParseValue) : token.Skip(2).Select(ParseValue);
                return dictionary => dictionary is null || dictionary.TryGetValue(key, out var v) && values.All(x => !Equal(x, v));
            }
            case "has":
            {
                return dictionary => dictionary is not null && dictionary.ContainsKey(key);
            }
            case "!has":
            {
                return dictionary => dictionary is null || !dictionary.ContainsKey(key);
            }
            case ">":
            {
                var value = ParseValue(token[2]);
                return dictionary => dictionary is not null && dictionary.TryGetValue(key, out var v) && Compare(v, value.Item1!) > 0;
            }
            case ">=":
            {
                var value = ParseValue(token[2]);
                return dictionary => dictionary is not null && dictionary.TryGetValue(key, out var v) && Compare(v, value.Item1!) >= 0;
            }
        }

        return null;
    }

    /// <summary>
    /// 値を展開します
    /// </summary>
    /// <param name="token">展開するトークン</param>
    /// <returns>(値, 型)</returns>
    private static (object?, int) ParseValue(JToken token)
    {
        return token.Type switch
        {
            JTokenType.String => (token.ToObject<string>(), 1), // 文字列
            JTokenType.Integer => (token.ToObject<int>(), 2), // 整数
            JTokenType.Float => (token.ToObject<float>(), 3), // 浮動小数点数
            JTokenType.Boolean => (token.ToObject<bool>(), 4), // 真偽値
            _ => (null, 0)
        };
    }
    
    private static bool Equal((object?, int) value, object target)
    {
        return value.Item2 switch
        {
            1 => target is string s && s == (string?)value.Item1,
            2 => target is long l && l == (int)value.Item1! || target is int i && i == (int)value.Item1!,
            3 => target is float f && Math.Abs(f - (float)value.Item1!) < 0.001f ||
                 target is double d && Math.Abs(d - (float)value.Item1!) < 0.001f,
            4 => target is bool b && b == (bool)value.Item1!,
            _ => false
        };
    }
    
    private static int Compare(object target, object value)
    {
        return target switch
        {
            string s => string.CompareOrdinal(s, (string)value),
            long l => l.CompareTo(value),
            int i => i.CompareTo(value),
            float f => f.CompareTo(value),
            double d => d.CompareTo(value),
            bool b => b.CompareTo(value),
            _ => 0
        };
    }
    
}