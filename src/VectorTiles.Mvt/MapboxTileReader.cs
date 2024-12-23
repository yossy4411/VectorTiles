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
            var constValues = layer.Values.Select(GetKeyValues).ToList();
            var features = new List<MapboxTile.Layer.Feature>();
            foreach (var feature in layer.Features)
            {
                var values = new Dictionary<string, IConstValue?>();
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
                int x1 = 0, y1 = 0; 
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
                            AddOval(tileZ, tileX, tileY, count, feature, ref j, extent, newGeometry, ref x1, ref y1);
                            break;
                        }
                        case 2:
                        {
                            // LineTo
                            AddOval(tileZ, tileX, tileY, count, feature, ref j, extent, newGeometry, ref x1, ref y1);
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

    private static void AddOval(int tileZ, int tileX, int tileY, uint count, Tile.Types.Feature feature, ref int j, uint extent,
        MapboxTile.Layer.Feature.Geometry newGeometry, ref int x1, ref int y1)
    {
        
        double tileSize = extent * Math.Pow(2, tileZ); // タイル全体のサイズ
    
        for (var k = 0; k < count; k++)
        {
            x1 += ZigZagDecode(feature.Geometry[j + 1]);
            y1 += ZigZagDecode(feature.Geometry[j + 2]);
        
            double worldX = (tileX * extent + x1) / tileSize;
            double worldY = (tileY * extent + y1) / tileSize;
        
            // タイル座標から緯度経度に変換
            var lon = worldX * 360.0 - 180.0;
            var lat = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * worldY))) * 180.0 / Math.PI;
        
            newGeometry.Points.Add(new MapboxTile.Layer.Feature.Geometry.Point
            {
                Lon = lon,
                Lat = lat
            });
            j += 2;
        }
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