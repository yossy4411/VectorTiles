using Google.Protobuf;
using VectorTiles.Styles;
using VectorTiles.Values;

namespace VectorTiles.Mvt;

public static class MapboxTileReader
{
    public static MapboxTile Read(Stream stream, int tileZ, int tileX, int tileY, VectorMapStyle? style = null)
    {
        var tile = new Tile();
        tile.MergeFrom(stream);
        return ToMapboxTile(tile, tileZ, tileX, tileY, style);
    }
    
    public static MapboxTile Read(byte[] data, int tileZ, int tileX, int tileY, VectorMapStyle? style)
    {
        using var stream = new MemoryStream(data);
        return Read(stream, tileZ, tileX, tileY, style);
    }
    
    public static MapboxTile Read(string path, int tileZ, int tileX, int tileY, VectorMapStyle? style)
    {
        using var stream = File.OpenRead(path);
        return Read(stream, tileZ, tileX, tileY, style);
    }
    
    private static MapboxTile ToMapboxTile(Tile tile, int tileZ, int tileX, int tileY, VectorMapStyle? style)
    {
        var layers = new List<MapboxTile.Layer>();
        foreach (var layer in tile.Layers)
        {
            if (style is not null && style.Layers.All(x => x.MaxZoom < tileZ || x.MinZoom > tileZ || x.Source != layer.Name))
            {
                continue;
            }
            var extent = layer.Extent;
            var constValues = layer.Values.Select(GetKeyValues).OfType<IConstValue>().ToList();
            var features = new List<MapboxTile.Layer.Feature>();
            foreach (var feature in layer.Features)
            {
                var values = new Dictionary<string, IConstValue>();
                for (var k = 0; k < feature.Tags.Count; k += 2)
                {
                    var key = layer.Keys[(int)feature.Tags[k]];
                    var value = constValues[(int)feature.Tags[k + 1]];
                    values[key] = value;
                }
                var geometries = new List<MapboxTile.Layer.Feature.Geometry>();
                var newGeometry = new MapboxTile.Layer.Feature.Geometry();
                var type = feature.Type switch
                {
                    Tile.Types.GeomType.Point => MapboxTile.Layer.Feature.FeatureType.Point,
                    Tile.Types.GeomType.Linestring => MapboxTile.Layer.Feature.FeatureType.LineString,
                    Tile.Types.GeomType.Polygon => MapboxTile.Layer.Feature.FeatureType.Polygon,
                    _ => MapboxTile.Layer.Feature.FeatureType.Unknown
                };
                var j = 0;
                while (j < feature.Geometry.Count)
                {
                    var geometry = feature.Geometry[j];
                    var id = geometry & 0b111; // Last 3 bits
                    var count = geometry >> 3; // Shift right 3 bits
                    switch (id)
                    {
                        case 1:
                        {
                            // MoveTo
                            if (j > 0)
                            {
                                geometries.Add(newGeometry);
                                newGeometry = new MapboxTile.Layer.Feature.Geometry();  // Reset
                            }
                            for (var k = 0; k < count; k++)
                            {
                                ExtractPoint(feature, j, extent, tileZ, tileX, tileY, out var x, out var y);
                                newGeometry.Points.Add(new MapboxTile.Layer.Feature.Geometry.Point
                                {
                                    Lon = x,
                                    Lat = y
                                });
                                j += 2;
                            }
                            break;
                        }
                        case 2:
                        {
                            // LineTo
                            for (var k = 0; k < count; k++)
                            {
                                ExtractPoint(feature, j, extent, tileZ, tileX, tileY, out var x, out var y);
                                newGeometry.Points.Add(new MapboxTile.Layer.Feature.Geometry.Point
                                {
                                    Lon = x,
                                    Lat = y
                                });
                                j += 2;
                            }
                            break;
                        }
                        case 7:
                        {
                            // ClosePath
                            break;
                        }
                    }
                    j++;
                }
                geometries.Add(newGeometry);
                features.Add(new MapboxTile.Layer.Feature
                {
                    Geometries = geometries,
                    Tags = values,
                    Type = type
                });
            }
            layers.Add(new MapboxTile.Layer
            {
                Name = layer.Name,
                Features = features,
                Extent = extent
            });
        }
        return new MapboxTile
        {
            Layers = layers
        };
    }

    private static void ExtractPoint(Tile.Types.Feature feature, int j, uint extent, int tileZ, int tileX, int tileY, out float lon, out float lat)
    {
        var x = ZigZagDecode(feature.Geometry[j + 1]);
        var y = ZigZagDecode(feature.Geometry[j + 2]);
        
        // Convert tile coordinates to longitude and latitude
        lon = (float)((tileX + (x / (double)extent)) * 360.0 / (1 << tileZ) - 180.0);
        lat = (float)(Math.Atan(Math.Sinh(Math.PI * (1 - 2 * (tileY + (y / (double)extent)) / (1 << tileZ)))) * 180.0 / Math.PI);
    }

    private static int ZigZagDecode(uint n)
    {
        return (int)(n >> 1) ^ -(int)(n & 1);
    }

    private static IConstValue? GetKeyValues(Tile.Types.Value value)
    {
        if (value.HasBoolValue)
        {
            return new ConstBoolValue(value.BoolValue);
        }

        if (value.HasFloatValue)
        {
            return new ConstFloatValue(value.FloatValue);
        }

        if (value.HasDoubleValue)
        {
            return new ConstFloatValue(value.FloatValue);
        }
        if (value.HasIntValue)
        {
            return new ConstIntValue((int)value.IntValue);
        }
        if (value.HasSintValue)
        {
            return new ConstIntValue((int)value.SintValue);
        }
        if (value.HasUintValue)
        {
            return new ConstIntValue((int)value.UintValue);
        }
        if (value.HasStringValue)
        {
            return new ConstStringValue(value.StringValue);
        }
        return null;
    }
}