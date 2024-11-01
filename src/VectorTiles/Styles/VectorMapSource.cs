namespace VectorTiles.Styles;

public class VectorMapSource
{
    public string Type { get; init; } = "vector";
    public uint MinZoom { get; init; } = 0;
    public uint MaxZoom { get; init; } = 22;
    public string? Url { get; init; }
    public string? Attribution { get; init; }
}