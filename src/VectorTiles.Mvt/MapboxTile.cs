using VectorTiles.Values;

namespace VectorTiles.Mvt;


public class MapboxTile
{
    public List<Layer> Layers { get; set; } = new();
    public class Layer
    {
        public string Name { get; set; } = string.Empty;
        public uint Extent { get; set; }
        public List<Feature> Features { get; set; } = new();
        public class Feature
        {
            public List<Geometry> Geometries { get; set; } = new();
            public Dictionary<string, IConstValue?> Tags { get; set; } = new();
            public enum FeatureType
            {
                Unknown = 0,
                Point = 1,
                LineString = 2,
                Polygon = 3
            }
            public FeatureType Type { get; set; }
            public class Geometry
            {
                public List<Point> Points { get; set; } = new();
                public class Point
                {
                    /// <summary>
                    /// Longitude
                    /// </summary>
                    public double Lon { get; set; }
                    /// <summary>
                    /// Latitude
                    /// </summary>
                    public double Lat { get; set; }
                }
                public bool IsEmpty => Points.Count == 0;
            }
        }
        
        
    }
}