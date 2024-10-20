using System.Diagnostics;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VectorTiles.Styles.Filters;
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
                var fillColor = ParseProperty(fillColorToken, Color.White);
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
                var lineColor = ParseProperty(lineColorToken, Color.White);
                var lineWidthToken = paintToken["line-width"];
                var lineWidth = ParseProperty(lineWidthToken, 1f);
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
                var textSize = ParseProperty(layoutToken?["text-size"], 12f);
                var textColor = ParseProperty(layoutToken?["text-color"], Color.Black);
                var fieldToken = layoutToken?["text-field"];
                var field = fieldToken is JArray
                    ? fieldToken[1]?.ToObject<string>()
                    : fieldToken?.ToObject<string>()?.Replace("{", "").Replace("}", "");
                var imageToken = layoutToken?["icon-image"];
                var iconImage = imageToken?.Type == JTokenType.String ? imageToken.ToObject<string>() : null;
                var iconSize = ParseProperty(layoutToken?["icon-size"], 1f);
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
                var backgroundColor = ParseProperty(backgroundColorToken, Color.White);
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

    private static IStyleProperty<T?> ParseProperty<T>(JToken? tokenA, T defaultValue)
    {
        if (tokenA is JValue jValue)
        {
            return ParseStaticValue<T>(jValue) ?? new StaticValueProperty<T>(defaultValue);
        }
        if (tokenA is not JObject li)
        {
            if (tokenA is not JArray array)
            {
                // I don't know what is the value
                return new StaticValueProperty<T>(defaultValue);
            }
            
            switch (array[0].ToObject<string>())
            {
                case "get":
                {
                    // ["get", "<key>"]
                    var key = array[1].ToObject<string>();
                    var property = new ValueGetProperty<T>
                    {
                        Key = key
                    };
                    return property;
                }
                case "%":
                {
                    // ["%", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1], 0f);
                    var value = ParseProperty(array[2], 0f);
                    return new ModuloProperty(key, value) as IStyleProperty<T?> ?? new StaticValueProperty<T>(defaultValue);
                }
                case "interpolate":
                {
                    // ["interpolate", ["linear"], ["zoom"], 0, 0, 22, 1] or ["interpolate", ["linear"], ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                    var type = array[1][0]?.ToObject<string>() switch
                    {
                        "linear" => InterpolateType.Linear,
                        _ => InterpolateType.Linear
                    };
                    var key = ParseProperty(array[2], 0f);
                    var segments = new InterpolateProperty<T?>(type, key);
                    
                    for (var i = 3; i < array.Count; i += 2)
                    {
                        var zoom = array[i].ToObject<float>();
                        var value = array[i + 1];
                        var valProp = ParseProperty(value, defaultValue);
                        var seg = CreateSegment(zoom, valProp);
                        if (seg is not null)
                        {
                            segments.Add(seg);
                        }
                    }
                    
                    return segments;
                }
                case "case":
                {
                    // ["case", ["==", ["get", "vt_code"], 5322], "red", ["==", ["get", "vt_code"], 5323], "blue", "green"]
                    var cases = new List<(IStyleFilter, IStyleProperty<T?>)>();
                    for (var i = 1; i < array.Count - 1; i += 2)
                    {
                        var filter = GetFilter((JArray)array[i]);
                        var value = array[i + 1];
                        var valProp = ParseProperty(value, defaultValue);
                        if (filter is not null)
                        {
                            cases.Add((filter, valProp));
                        }
                    }

                    var defaultProp = ParseProperty(array[array.Count - 1], defaultValue);
                    return new CaseProperty<T?>(cases, defaultProp);
                }

                case "match":
                {
                    // ["match", ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                    var key = ParseProperty(array[1]);
                    var cases = new List<(IConstValue, IConstValue)>();
                    for (var i = 2; i < array.Count - 1; i += 2)
                    {
                        var value = array[i];
                        var valProp = ParseValue(value);
                        var result = array[i + 1];
                        var resultProp = ParseValue(result);
                        if (valProp is not null && resultProp is not null)
                        {
                            cases.Add((valProp, resultProp));
                        }
                    }
                    var defaultProp = ParseValue(array[array.Count - 1]);
                    if (defaultProp is null) return new StaticValueProperty(default);
                    return new MatchProperty(key, cases, defaultProp);
                    
                }
                default:
                {
                    return new StaticValueProperty<T>(defaultValue);
                }
            }
            
            
        }

        {
            // New interpolation format
            // {"stops": [[0, "red"], [10, "blue"], [20, "green"]]}
            var segments = new InterpolateProperty<T?>(InterpolateType.Linear, new ValueGetProperty<float>{Key = "$zoom"});
            if (li["stops"] is not JArray stops) return segments;
            foreach (var stop in stops)
            {
                if (stop is not JArray token) continue;
                if (token.Count < 2) continue;
                var zoom = token[0].ToObject<float>();
                var value = token[1];
                var valProp = ParseProperty(value, defaultValue);
                var seg = CreateSegment(zoom, valProp);
                if (seg is not null)
                {
                    segments.Add(seg);
                }
            }

            return segments;
        }

    }

    private static StaticValueProperty<T>? ParseStaticValue<T>(JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.String:
            {
                var seg = new StaticValueProperty<Color>(ParseColor(value.ToObject<string>()));
                return seg as StaticValueProperty<T>;
            }
            case JTokenType.Float:
            {
                var seg = new StaticValueProperty<float>(value.ToObject<float>());
                return seg as StaticValueProperty<T>;
            }
            case JTokenType.Integer:
            {
                var seg = new StaticValueProperty<float>(value.ToObject<int>());
                return seg as StaticValueProperty<T>;
            }
            default:
            {
                Debug.WriteLine("Unknown interpolation type: " + value.Type);
                return null;
            }
        }
    }
    
    private static InterpolateSegment<T>? CreateSegment<T>(float zoom, IStyleProperty<T> value)
    {
        return value switch
        {
            IStyleProperty<Color> color => new InterpolateSegmentColor(zoom, color) as InterpolateSegment<T>,
            IStyleProperty<float> f => new InterpolateSegmentFloat(zoom, f) as InterpolateSegment<T>,
            _ => null
        };
    }

    private static IStyleFilter? GetFilter(JArray token)
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
                List<IStyleFilter> filterList = new();
                for (var i = 1; i < token.Count; i++)
                {
                    var token1 = (JArray)token[i];
                    var filter = GetFilter(token1);
                    if (filter is not null)
                    {
                        filterList.Add(filter);
                    }
                }

                if (filterList.Count == 0) return FalseFilter.Instance;
                
                return first switch
                {
                    "all" => new AllFilter(filterList),  // All was true, return true.
                    "any" => new AnyFilter(filterList),  // Any was true, return true.
                    "none" => new NoneFilter(filterList), // All was false, return true.
                    _ => null
                };
            }
            case "step":
            {
                // ["step", ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                List<IStyleFilter> filterList = new();
                List<float> stops = new();
                filterList.Add(GetFilter((JArray)token[2])!);
                for (var i = 3; i < token.Count; i+=2)
                {
                    var filter = GetFilter((JArray)token[i + 1]);
                    if (filter is null) continue;
                    filterList.Add(filter);
                    var stop = token[i].ToObject<float>();
                    stops.Add(stop);
                }

                var key = ParseProperty(token[1], 0f);
                return new StepFilter(filterList, stops, key);
            }
        }

        return null;
    }

    private static IStyleFilter? GetOneFilter(JArray token)
    {
        var not = (token[0].Type == JTokenType.String ? token[0].ToObject<string>() : null) == "!";
        if (not)
            token = (JArray)token[1];
        var filterType = token[0].ToObject<string>();
        if (not)
        {
            filterType = filterType switch
            {
                "has" => "!has",
                "!has" => "has",
                "in" => "!in",
                "!in" => "in",
                "==" => "!=",
                "!=" => "==",
                _ => filterType
            }; // invert
        }
        
        switch (filterType)
        {
            case "==":
            {
                var (value, type) = ParseValue(token[2]);
                
                if (value is null) return null;
                return type switch
                {
                    1 => new EqualFilter<string>(ParseProperty(token[1], string.Empty), (string)value),
                    2 => new EqualFilter<bool>(ParseProperty(token[1], false), (bool)value),
                    3 => new EqualFilter<int>(ParseProperty(token[1], 0), (int)value),
                    4 => new EqualFilter<float>(ParseProperty(token[1], 0f), (float)value),
                    _ => null
                };
            }
            case "!=":
            {
                var (value, type) = ParseValue(token[2]);
                
                if (value is null) return null;
                return type switch
                {
                    1 => new NotEqualFilter<string>(ParseProperty(token[1], string.Empty), (string)value),
                    2 => new NotEqualFilter<bool>(ParseProperty(token[1], false), (bool)value),
                    3 => new NotEqualFilter<int>(ParseProperty(token[1], 0), (int)value),
                    4 => new NotEqualFilter<float>(ParseProperty(token[1], 0f), (float)value),
                    _ => null
                };
            }
            case "in":
            {
                var values = InFilter(token, out var type);
                return type switch
                {
                    1 => new InFilter<string>(ParseProperty(token[1], string.Empty), values.Where(o => o.Item1 is not null && o.Item2 == 1).Select(o => (string)o.Item1!).ToList()),
                    2 => new InFilter<bool>(ParseProperty(token[1], false), values.Where(o => o.Item1 is not null && o.Item2 == 2).Select(o => (bool)o.Item1!).ToList()),
                    3 => new InFilter<int>(ParseProperty(token[1], 0), values.Where(o => o.Item1 is not null && o.Item2 == 3).Select(o => (int)o.Item1!).ToList()),
                    4 => new InFilter<float>(ParseProperty(token[1], 0f), values.Where(o => o.Item1 is not null && o.Item2 is 3 or 4).Select(o => o.Item2 == 3 ? 
                            (int)o.Item1! : (float)o.Item1!).ToList()),
                    _ => null
                };
            }
            case "!in":
            {
                var values = InFilter(token, out var type);
                return type switch
                {
                    1 => new NotInFilter<string>(ParseProperty(token[1], string.Empty), values.Where(o => o.Item1 is not null && o.Item2 == 1).Select(o => (string)o.Item1!).ToList()),
                    2 => new NotInFilter<bool>(ParseProperty(token[1], false), values.Where(o => o.Item1 is not null && o.Item2 == 2).Select(o => (bool)o.Item1!).ToList()),
                    3 => new NotInFilter<int>(ParseProperty(token[1], 0), values.Where(o => o.Item1 is not null && o.Item2 == 3).Select(o => (int)o.Item1!).ToList()),
                    4 => new NotInFilter<float>(ParseProperty(token[1], 0f), values.Where(o => o.Item1 is not null && o.Item2 is 3 or 4).Select(o => o.Item2 == 3 ? 
                        (int)o.Item1! : (float)o.Item1!).ToList()),
                    _ => null
                };
            }
            case "has":
            {
                var key = GetKey(token[1]);
                return key is null ? null : new HasFilter(key);
            }
            case "!has":
            {
                var key = GetKey(token[1]);
                return key is null ? null : new NotHasFilter(key);
            }
            case ">":
            {
                var (value, type) = ParseValue(token[2]);
                if (value is null) return null;
                return type switch
                {
                    3 => new BiggerFilter<int>(ParseProperty(token[1], 0), (int)value),
                    4 => new BiggerFilter<float>(ParseProperty(token[1], 0f), (float)value),
                    _ => null
                };
            }
            case ">=":
            {
                var (value, type) = ParseValue(token[2]);
                if (value is null) return null;
                return type switch
                {
                    3 => new BiggerOrEqualFilter<int>(ParseProperty(token[1], 0), (int)value),
                    4 => new BiggerOrEqualFilter<float>(ParseProperty(token[1], 0f), (float)value),
                    _ => null
                };
            }
            case "<":
            {
                var (value, type) = ParseValue(token[2]);
                if (value is null) return null;
                return type switch
                {
                    3 => new LesserFilter<int>(ParseProperty(token[1], 0), (int)value),
                    4 => new LesserFilter<float>(ParseProperty(token[1], 0f), (float)value),
                    _ => null
                };
            }
            case "<=":
            {
                var (value, type) = ParseValue(token[2]);
                if (value is null) return null;
                return type switch
                {
                    3 => new LesserOrEqualFilter<int>(ParseProperty(token[1], 0), (int)value),
                    4 => new LesserOrEqualFilter<float>(ParseProperty(token[1], 0f), (float)value),
                    _ => null
                };
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

    private static (object?, int)[] InFilter(JArray token, out int type)
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

        var valueTuples = values.ToArray();
        type = valueTuples.Max(x => x.Item2);
        return valueTuples;
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
            JTokenType.Boolean => (token.ToObject<bool>(), 2), // 真偽値
            JTokenType.Integer => (token.ToObject<int>(), 3), // 整数
            JTokenType.Float => (token.ToObject<float>(), 4), // 浮動小数点数
            _ => (null, 0)
        };
    }
}