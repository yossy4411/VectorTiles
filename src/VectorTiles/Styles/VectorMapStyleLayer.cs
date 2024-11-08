using System.Drawing;
using VectorTiles.Styles.Filters;
using VectorTiles.Styles.Values;
using VectorTiles.Values;

namespace VectorTiles.Styles;

/// <summary>
///     Layer for drawing vector map
/// </summary>
public abstract class VectorMapStyleLayer
{
    private readonly IStyleFilter? _filter;

    protected VectorMapStyleLayer(string? source = null, IStyleFilter? filter = null)
    {
        Source = source;
        _filter = filter;
    }

    public string? Source { get; }
    public int MinZoom { get; init; } = 0;
    public int MaxZoom { get; init; } = 22;
    public string? Id { get; init; }

    public bool IsVisible(Dictionary<string, IConstValue?> values)
    {
        return _filter?.Filter(values) ?? true;
    }
}

/// <summary>
///     Layer for drawing background
/// </summary>
public class VectorBackgroundStyleLayer
    : VectorMapStyleLayer
{
    public IStyleProperty? BackgroundColor { get; init; }
}

public class VectorFillStyleLayer : VectorMapStyleLayer
{
    public VectorFillStyleLayer(string? source = null, IStyleFilter? filter = null) : base(source, filter)
    {
    }

    public IStyleProperty? FillColor { get; init; }
}

/// <summary>
///     Layer for drawing lines
/// </summary>
public class VectorLineStyleLayer : VectorMapStyleLayer
{
    public VectorLineStyleLayer(string? source = null, IStyleFilter? filter = null) : base(source, filter)
    {
    }

    public IStyleProperty? LineWidth { get; init; }

    /// <summary>
    ///     Pattern of dashes and gaps to be used when drawing lines.
    /// </summary>
    /// <remarks>
    ///     We set it as a virtual method to implement compatibility with other libraries such as SkiaSharp.
    ///     If you override this property, you can return null for getter because it is not used in the library.
    /// </remarks>
    public virtual float[]? DashArray { get; init; }

    public IStyleProperty? LineColor { get; init; }
}

/// <summary>
///     Layer for drawing symbols and text
/// </summary>
public class VectorSymbolStyleLayer : VectorMapStyleLayer
{
    public VectorSymbolStyleLayer(string? source = null, IStyleFilter? filter = null) : base(source, filter)
    {
    }

    public string? IconImage { get; init; }

    public IStyleProperty? IconSize { get; init; }

    public IStyleProperty? IconColor { get; init; }

    public IStyleProperty? IconOpacity { get; init; }

    public IStyleProperty? IconRotate { get; init; }

    public IStyleProperty? TextSize { get; init; }

    public IStyleProperty? TextColor { get; init; }

    public IStyleProperty? TextOpacity { get; init; }

    public string? TextField { get; init; }

    public string? TextFont { get; init; }

    public string? TextAnchor { get; init; }
}