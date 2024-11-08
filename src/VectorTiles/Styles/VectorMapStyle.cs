namespace VectorTiles.Styles;

public class VectorMapStyle
{
    public string? Name { get; init; }
    public List<VectorMapStyleLayer> Layers { get; init; } = new();
    public List<VectorMapSource> Sources { get; init; } = new();
}