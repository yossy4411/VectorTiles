using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VectorTiles.Styles.Filters;
using VectorTiles.Styles.Values;
using VectorTiles.Values;

namespace VectorTiles.Styles.MapboxGL;

public static class MapboxStyle
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

    public static VectorMapStyle LoadGLJson(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        var jObject = JObject.Load(jsonReader);
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

    /// <summary>
    ///     Parse color
    /// </summary>
    /// <param name="value">Color string</param>
    /// <param name="color">Color value</param>
    /// <returns>The result of parsing</returns>
    private static bool TryParseColor(string? value, out Color color)
    {
        if (value is null)
        {
            color = Color.Empty;
            return false;
        }

        if (value.StartsWith('#'))
        {
            color = Color.FromArgb(int.Parse(value[1..], NumberStyles.HexNumber));
            return true;
        }

        if (value.StartsWith("rgba"))
        {
            // ex. rgba(40,20,100,0.8)
            var values = value[5..^1].Split(',');
            color = Color.FromArgb((byte)(float.Parse(values[3]) * 255), byte.Parse(values[0]), byte.Parse(values[1]),
                byte.Parse(values[2]));
            return true;
        }

        if (value.StartsWith("rgb"))
        {
            // ex. rgb(40,20,100)
            var values = value[4..^1].Split(',');
            color = Color.FromArgb(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]));
            return true;
        }

        if (value.StartsWith("hsla"))
        {
            // ex. hsla(120,100%,50%,0.8)
            var values = value[5..^1].Split(',');
            var a = (byte)(float.Parse(values[3]) * 255);
            color = FromHsl(float.Parse(values[0]), float.Parse(values[1].TrimEnd('%')) / 100,
                float.Parse(values[2].TrimEnd('%')) / 100, a);
            return true;
        }

        if (value.StartsWith("hsl"))
        {
            // ex. hsl(120,100%,50%)
            var values = value[4..^1].Split(',');
            color = FromHsl(float.Parse(values[0]), float.Parse(values[1].TrimEnd('%')) / 100,
                float.Parse(values[2].TrimEnd('%')) / 100);
            return true;
        }

        Debug.WriteLine("Unknown color format: " + value);
        color = Color.Empty;
        return false;
    }

    private static Color FromHsl(float hue, float saturation, float lightness, byte alpha = 255)
    {
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = lightness - c / 2;

        var (r, g, b) = hue switch
        {
            >= 0 and < 60 => new ValueTuple<double, double, double>(c, x, 0),
            >= 60 and < 120 => (x, c, 0),
            >= 120 and < 180 => (0, c, x),
            >= 180 and < 240 => (0, x, c),
            >= 240 and < 300 => (x, 0, c),
            _ => (c, 0, x)
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
                var fillColor = ParseProperty(fillColorToken);
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
                var lineColor = ParseProperty(lineColorToken);
                var lineWidthToken = paintToken["line-width"];
                var lineWidth = ParseProperty(lineWidthToken);
                var dashToken = paintToken["line-dasharray"];
                var dashToken1 = dashToken?[0];
                // {"line-dasharray": ["literal", [1, 2]]} or {"line-dasharray": [1, 2]} <- What's the difference?
                var dashArray = dashToken1 is null ? null :
                    dashToken1.Type == JTokenType.String ? dashToken![1]?.ToObject<float[]>() :
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
                var textSize = ParseProperty(layoutToken?["text-size"]);
                var textColor = ParseProperty(jToken["paint"]?["text-color"]);
                var fieldToken = layoutToken?["text-field"];
                var field = fieldToken is JArray
                    ? fieldToken[1]?.ToObject<string>()
                    : fieldToken?.ToObject<string>()?.Replace("{", "").Replace("}", "");
                var imageToken = layoutToken?["icon-image"];
                var iconImage = imageToken?.Type == JTokenType.String ? imageToken.ToObject<string>() : null;
                var iconSize = ParseProperty(layoutToken?["icon-size"]);
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
                var backgroundColor = ParseProperty(backgroundColorToken);
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

    private static IStyleProperty ParseProperty(JToken? tokenA)
    {
        if (tokenA is JValue jValue) return ParseStaticValue(jValue) ?? new StaticValueProperty(default);
        if (tokenA is not JObject li)
        {
            if (tokenA is not JArray array)
                // I don't know what is the value
                return new StaticValueProperty(default);

            if (array.Count == 1)
                // might: ["zoom"]
                return new ValueGetProperty
                {
                    Key = GetKey(array)
                };

            switch (array[0].ToObject<string>())
            {
                case "get":
                case "var":
                {
                    // ["get", "<key>"]
                    // ["var", "<key>"]  (nearly equal to get)
                    var key = array[1].ToObject<string>();
                    var property = new ValueGetProperty
                    {
                        Key = key
                    };
                    return property;
                }
                case "%":
                {
                    // ["%", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1]);
                    var value = ParseProperty(array[2]);
                    return new ModuloProperty(key, value);
                }
                case "+":
                {
                    // ["+", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1]);
                    var value = ParseProperty(array[2]);
                    return new PlusProperty(key, value);
                }
                case "-":
                {
                    // ["-", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1]);
                    var value = ParseProperty(array[2]);
                    return new MinusProperty(key, value);
                }
                case "*":
                {
                    // ["*", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1]);
                    var value = ParseProperty(array[2]);
                    return new MultiplyProperty(key, value);
                }
                case "/":
                {
                    // ["/", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1]);
                    var value = ParseProperty(array[2]);
                    return new DivideProperty(key, value);
                }
                case "^":
                {
                    // ["^", ["get", "vt_code"], 10]
                    var key = ParseProperty(array[1]);
                    var value = ParseProperty(array[2]);
                    return new PowerProperty(key, value);
                }
                case "interpolate":
                {
                    // ["interpolate", ["linear"], ["zoom"], 0, 0, 22, 1] or ["interpolate", ["linear"], ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                    var type = array[1][0]?.ToObject<string>() switch
                    {
                        "linear" => InterpolateType.Linear,
                        "exponential" => InterpolateType.Exponential,
                        _ => InterpolateType.Linear
                    };
                    // var args = type is not InterpolateType.Exponential ? null : array[1][1]; // factor key
                    var key = ParseProperty(array[2]);
                    var segments = new InterpolateProperty(type, key);

                    for (var i = 3; i < array.Count; i += 2)
                    {
                        var zoom = array[i].ToObject<float>();
                        var value = array[i + 1];
                        var valProp = ParseProperty(value);
                        var seg = new InterpolateSegment(zoom, valProp);
                        segments.Add(seg);
                    }

                    return segments;
                }
                case "case":
                {
                    // ["case", ["==", ["get", "vt_code"], 5322], "red", ["==", ["get", "vt_code"], 5323], "blue", "green"]
                    var cases = new List<(IStyleFilter, IStyleProperty)>();
                    for (var i = 1; i < array.Count - 1; i += 2)
                    {
                        var filter = GetFilter((JArray)array[i]);
                        var value = array[i + 1];
                        var valProp = ParseProperty(value);
                        if (filter is not null) cases.Add((filter, valProp));
                    }

                    var defaultProp = ParseProperty(array[array.Count - 1]);
                    return new CaseProperty(cases, defaultProp);
                }

                case "match":
                {
                    // ["match", ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                    var key = ParseProperty(array[1]);
                    var cases = new List<(IConstValue[], IStyleProperty)>();
                    for (var i = 2; i < array.Count - 1; i += 2)
                    {
                        var value = array[i];
                        IConstValue[] caseProp;
                        if (value is JValue)
                        {
                            var caseValue = ParseValue(value);
                            if (caseValue is null) continue;
                            caseProp = new[] { caseValue };
                        }
                        else
                        {
                            caseProp = value.Select(ParseValue).OfType<IConstValue>().ToArray();
                        }

                        var result = array[i + 1];
                        var resultProp = ParseProperty(result);
                        cases.Add((caseProp, resultProp));
                    }

                    var defaultProp = ParseProperty(array[array.Count - 1]);
                    return new MatchProperty(key, cases, defaultProp);
                }

                case "step":
                {
                    // ["step", ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                    List<IStyleProperty> filterList = new();
                    List<IConstValue> stops = new();
                    filterList.Add(ParseProperty(array[2]));
                    for (var i = 3; i < array.Count; i += 2)
                    {
                        var filter = ParseProperty(array[i + 1]);
                        filterList.Add(filter);
                        var stop = ParseValue(array[i]);
                        if (stop is not null) stops.Add(stop);
                    }

                    var key = ParseProperty(array[1]);
                    return new StepProperty(filterList, stops, key);
                }

                case "let":
                {
                    // ["let", "color", ["get", "vt_code"], "size", 10, ["var", "color"]]
                    var variables = new List<(string, IStyleProperty)>();
                    for (var i = 1; i < array.Count - 1; i += 2)
                    {
                        var key = array[i].ToObject<string>();
                        if (key is null) continue;
                        var value = ParseProperty(array[i + 1]);
                        variables.Add((key, value));
                    }
                    var finalValue = ParseProperty(array[array.Count - 1]);
                    return new LetProperty(variables, finalValue);
                }
                default:
                {
                    return new StaticValueProperty(default);
                }
            }
        }

        {
            // New interpolation format
            // {"stops": [[0, "red"], [10, "blue"], [20, "green"]]}
            var segments = new InterpolateProperty(InterpolateType.Linear, new ValueGetProperty { Key = "$zoom" });
            if (li["stops"] is not JArray stops) return segments;
            foreach (var stop in stops)
            {
                if (stop is not JArray token) continue;
                if (token.Count < 2) continue;
                var zoom = token[0].ToObject<float>();
                var value = token[1];
                var valProp = ParseProperty(value);
                var seg = new InterpolateSegment(zoom, valProp);
                segments.Add(seg);
            }

            return segments;
        }
    }

    private static StaticValueProperty? ParseStaticValue(JToken token)
    {
        var value = ParseValue(token);
        return value is null ? null : new StaticValueProperty(value);
    }

    private static IConstValue? ParseValue(JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.String:
            {
                var str = value.ToObject<string>();
                if (str is null) return null;
                if ((str.StartsWith("rgba") || str.StartsWith("rgb") || str.StartsWith("hsla") ||
                     str.StartsWith("hsl") || str.StartsWith("#")) &&
                    TryParseColor(value.ToObject<string>(), out var color))
                    return new ConstColorValue(color);


                if (str is "true" or "false") return new ConstBoolValue(str == "true");
                if (int.TryParse(str, out var intValue)) return new ConstIntValue(intValue);
                if (float.TryParse(str, out var floatValue)) return new ConstFloatValue(floatValue);
                return new ConstStringValue(str);
            }
            case JTokenType.Float:
            {
                return new ConstFloatValue(value.ToObject<float>());
            }
            case JTokenType.Integer:
            {
                return new ConstIntValue(value.ToObject<int>());
            }
            case JTokenType.None:
            case JTokenType.Object:
            case JTokenType.Array:
            case JTokenType.Constructor:
            case JTokenType.Property:
            case JTokenType.Comment:
            case JTokenType.Boolean:
            case JTokenType.Null:
            case JTokenType.Undefined:
            case JTokenType.Date:
            case JTokenType.Raw:
            case JTokenType.Bytes:
            case JTokenType.Guid:
            case JTokenType.Uri:
            case JTokenType.TimeSpan:
            default:
            {
                Debug.WriteLine("Unknown interpolation type: " + value.Type);
                return null;
            }
        }
    }

    private static IStyleFilter? GetFilter(JArray token)
    {
        // new filter
        if (token[1] is not JArray) return GetOneFilter(token);
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
                    if (filter is not null) filterList.Add(filter);
                }

                if (filterList.Count == 0) return FalseFilter.Instance;

                return first switch
                {
                    "all" => new AllFilter(filterList), // All was true, return true.
                    "any" => new AnyFilter(filterList), // Any was true, return true.
                    "none" => new NoneFilter(filterList), // All was false, return true.
                    _ => null
                };
            }
            case "step":
            {
                // ["step", ["get", "vt_code"], 5322, "red", 5323, "blue", "green"]
                List<IStyleFilter> filterList = new();
                List<IConstValue> stops = new();
                filterList.Add(GetFilter((JArray)token[2])!);
                for (var i = 3; i < token.Count; i += 2)
                {
                    var filter = GetFilter((JArray)token[i + 1]);
                    if (filter is null) continue;
                    filterList.Add(filter);
                    var stop = ParseValue(token[i]);
                    if (stop is not null) stops.Add(stop);
                }

                var key = ParseProperty(token[1]);
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

        switch (filterType)
        {
            case "==":
            {
                var value = ParseValue(token[2]);
                return value is null ? null : new EqualFilter(ParseProperty(token[1]), value);
            }
            case "!=":
            {
                var value = ParseValue(token[2]);
                return value is null ? null : new NotEqualFilter(ParseProperty(token[1]), value);
            }
            case "in":
            {
                var values = ParseInFilter(token);
                return new InFilter(ParseProperty(token[1]), values);
            }
            case "!in":
            {
                var values = ParseInFilter(token);
                return new NotInFilter(ParseProperty(token[1]), values);
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
                var value = ParseValue(token[2]);
                return value is null ? null : new BiggerFilter(ParseProperty(token[1]), value);
            }
            case ">=":
            {
                var value = ParseValue(token[2]);
                return value is null ? null : new BiggerOrEqualFilter(ParseProperty(token[1]), value);
            }
            case "<":
            {
                var value = ParseValue(token[2]);
                return value is null ? null : new LesserFilter(ParseProperty(token[1]), value);
            }
            case "<=":
            {
                var value = ParseValue(token[2]);
                return value is null ? null : new LesserOrEqualFilter(ParseProperty(token[1]), value);
            }
        }

        return null;
    }

    private static string? GetKey(JToken jToken)
    {
        var key = jToken is JArray keyToken
            ? keyToken.Count > 1
                ? keyToken[1].ToObject<string>() // ["get", "<key>"]
                : keyToken[0].ToObject<string>() switch
                {
                    "geometry-type" => "$type",
                    "zoom" => "$zoom",
                    _ => null
                } // example: ["$type"] (new), ["geometry-type"] (old), ["zoom"] (old)
            : jToken.ToObject<string>();
        return key;
    }

    private static List<IConstValue> ParseInFilter(JArray token)
    {
        var value = token[2];
        IEnumerable<IConstValue?> values;
        if (value is JArray array)
        {
            var first = array[0];
            if (first.Type == JTokenType.String && first.ToObject<string>() == "literal")
                // ["in", ["get", "vt_code"], ["literal", [5101, 5103]]]
                values = array[1].Select(ParseValue);
            else
                // ["in", ["get", "vt_code"], [5322, ...]]
                values = array.Select(ParseValue);
        }
        else
        {
            // ["in", ["get", "vt_code"], 5322, ...]
            values = token.Skip(2).Select(ParseValue);
        }

        var valueTuples = values.OfType<IConstValue>().ToArray();
        return valueTuples.ToList();
    }
}