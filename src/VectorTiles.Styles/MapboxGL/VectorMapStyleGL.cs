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
        var filterToken = jToken["filter"];
        var vMapFilter = filterToken is JArray filterObj ? GetFilter(filterObj) : null;


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
                var fillColor = ParseInterpolation(fillColorToken, Color.White);
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
                var lineColor = ParseInterpolation(lineColorToken, Color.White);
                var lineWidthToken = paintToken["line-width"];
                var lineWidth = ParseInterpolation(lineWidthToken, 1f);
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
                var textSize = ParseInterpolation(layoutToken?["text-size"], 12f);
                var textColor = ParseInterpolation(layoutToken?["text-color"], Color.Black);
                var fieldToken = layoutToken?["text-field"];
                var field = fieldToken is JArray
                    ? fieldToken[1]?.ToObject<string>()
                    : fieldToken?.ToObject<string>()?.Replace("{", "").Replace("}", "");
                var imageToken = layoutToken?["icon-image"];
                var iconImage = imageToken?.Type == JTokenType.String ? imageToken.ToObject<string>() : null;
                var iconSize = ParseInterpolation(layoutToken?["icon-size"], 1f);
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
                var backgroundColor = ParseInterpolation(backgroundColorToken, Color.White);
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

    private static IStyleProperty<T?> ParseInterpolation<T>(JToken? tokenA, T defaultValue)
    {
        if (tokenA is not JObject li)
        {
            if (tokenA is not JArray)
                return new StaticValueProperty<T>(defaultValue);
            
            switch (tokenA[0]?.ToObject<string>())
            {
                case "get":
                {
                    // ["get", "<key>"]
                    var key = tokenA[1]?.ToObject<string>();
                    var property = new ValueGetProperty<T>
                    {
                        Key = key
                    };
                    return property;
                }
            }
            
            return new StaticValueProperty<T>(defaultValue);
        }
        
        // New interpolation format
        // {"stops": [[0, "red"], [10, "blue"], [20, "green"]]}
        var segments = new InterpolateProperty<T>(InterpolateType.Linear);
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

    private static VectorMapFilter? GetFilter(JArray token)
    {
        // new filter
        if (token[1] is not JArray)
        {
            return GetOneFilter(token);
        }
        var not = token[0].ToObject<string>() == "!";
        
        // old filter
        var first = not ? token[1][0]!.ToObject<string>() : token[0].ToObject<string>();
        
        if (first is not ("all" or "any" or "none" or "step"))
            return GetOneFilter(token);
        
        // Handling !all
        
        if (not)
        {
            token = (JArray)token[1];
            if (first == "all") first = "none";
        }
        
        switch (first)
        {
            case "all" or "any" or "none":
            {
                // all, any
                List<VectorMapFilter> filterList = new();
                for (var i = 1; i < token.Count; i++)
                {
                    var token1 = (JArray)token[i];
                    var filter = GetFilter(token1);
                    if (filter is not null)
                    {
                        filterList.Add(filter);
                    }
                }

                if (filterList.Count == 0) return _ => false;
                
                return first switch
                {
                    "all" => dictionary => filterList.All(x => x(dictionary)),  // All was true, return true.
                    "any" => dictionary => filterList.Any(x => x(dictionary)),  // Any was true, return true.
                    "none" => dictionary => filterList.All(x => !x(dictionary)), // All was false, return true.
                    _ => null
                };
            }
            case "step":
            {
                // ["step", ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                List<VectorMapFilter> filterList = new();
                List<int> stops = new();
                var defaultFilter = GetFilter((JArray)token[2])!;
                for (var i = 3; i < token.Count; i+=2)
                {
                    var filter = GetFilter((JArray)token[i + 1]);
                    if (filter is null) continue;
                    filterList.Add(filter);
                    var stop = token[i].ToObject<int>();
                    stops.Add(stop);
                }
                
                var key = GetKey(token[1]);
                if (key is null) return null;

                return dictionary =>
                {
                    // ["step", ["get", "zoom"], <filter1>, 5, <filter2>, 10, <filter3>, 15, <filter4>]
                    // 0-5: filter1, 5-10: filter2, 10-15: filter3, 15-: filter4
                    if (dictionary is null || !dictionary.TryGetValue(key, out var v)) return false;
                    if (v is not int intValue) return false;
                    if (intValue < stops[0]) return defaultFilter(dictionary);
                    
                    if (stops.Count == 1) return filterList[0](dictionary);
                    // Range check
                    for (var i = 0; i < stops.Count - 1; i++)
                    {
                        if (intValue >= stops[i] && intValue < stops[i + 1])
                        {
                            return filterList[i](dictionary);
                        }
                    }
                    // exceeded the last stop, return the last filter
                    return filterList[^1](dictionary);
                };
            }
        }

        return null;
    }

    private static VectorMapFilter? GetOneFilter(JArray token)
    {
        var not = (token[0].Type == JTokenType.String ? token[0].ToObject<string>() : null) == "!";
        if (not)
            token = (JArray)token[1];
        var keyToken = token[1];
        var key = GetKey(keyToken);
        if (key is null) return null;
        var type = token[0].ToObject<string>();
        if (not)
        {
            type = type switch
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
                var values = InFilter(token);
                return dictionary => dictionary is not null && dictionary.TryGetValue(key, out var v) && values.Any(x => Equal(x, v));
            }
            case "!in":
            {
                var values = InFilter(token);
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

    private static string? GetKey(JToken jToken)
    {
        var key = jToken is JArray keyToken ?
            keyToken.Count > 1 ? 
                keyToken[1].ToObject<string>() // ["get", "<key>"]
                : keyToken[0].ToObject<string>() switch
                {
                    "geometry-type" => "$type",
                    "zoom" => "$zoom",
                    _ => null
                } // example: ["$type"] (new), ["geometry-type"] (old), ["zoom"] (old)
            : jToken.ToObject<string>();
        return key;
    }

    private static IEnumerable<(object?, int)> InFilter(JArray token)
    {
        var value = token[2];
                
        IEnumerable<(object?, int)> values;
        if (value is JArray array)
        {
            var first = array[0];
            if (first.Type == JTokenType.String && first.ToObject<string>() == "literal")
            {
                // ["in", ["get", "vt_code"], ["literal", [5101, 5103]]]
                values = array[1].Select(ParseValue);
            }
            else
            {
                // ["in", ["get", "vt_code"], [5322, ...]]
                values = array.Select(ParseValue);
            }
        }
        else
        {
            // ["in", ["get", "vt_code"], 5322, ...]
            values = token.Skip(2).Select(ParseValue);
        }

        return values;
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