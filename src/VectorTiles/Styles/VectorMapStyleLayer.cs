using System.Drawing;
using VectorTiles.Styles.Values;

namespace VectorTiles.Styles;

public delegate bool VectorMapFilter(Dictionary<string, object>? values);

/// <summary>
/// Layer for drawing vector map
/// </summary>
public abstract class VectorMapStyleLayer
{
    public string? Source { get; }
    public int MinZoom { get; init; } = 0;
    public int MaxZoom { get; init; } = 22;
    public string? Id { get; init; }
    private readonly VectorMapFilter? _filter;

    protected static readonly Color DefaultColor = Color.White;
    
    protected VectorMapStyleLayer(string? source = null, VectorMapFilter? filter = null)
    {
        Source = source;
        _filter = filter;
    }
    
    public bool IsVisible(Dictionary<string, object> values)
    {
        return _filter?.Invoke(values) ?? true;
    }
}

/// <summary>
/// Layer for drawing background
/// </summary>
public class VectorBackgroundStyleLayer
    : VectorMapStyleLayer
{
    public IStyleProperty<Color>? BackgroundColor { get; init; }
}

public class VectorFillStyleLayer : VectorMapStyleLayer
{
    public VectorFillStyleLayer(string? source = null, VectorMapFilter? filter = null) : base(source, filter)
    {
    }
    
    public IStyleProperty<Color>? FillColor { get; init; }
}

/// <summary>
/// Layer for drawing lines
/// </summary>
public class VectorLineStyleLayer : VectorMapStyleLayer
{
    public VectorLineStyleLayer(string? source = null, VectorMapFilter? filter = null) : base(source, filter)
    {
    }
    
    public IStyleProperty<float>? LineWidth { get; init; }
    
    /// <summary>
    /// Pattern of dashes and gaps to be used when drawing lines.
    /// </summary>
    /// <remarks>
    /// We set it as a virtual method to implement compatibility with other libraries such as SkiaSharp.
    /// If you override this property, you can return null for getter because it is not used in the library.
    /// </remarks>
    public virtual float[]? DashArray { get; init; }
    
    public IStyleProperty<Color>? LineColor { get; init; }
}

/// <summary>
/// Layer for drawing symbols and text
/// </summary>
public class VectorSymbolStyleLayer : VectorMapStyleLayer
{
    public VectorSymbolStyleLayer(string? source = null, VectorMapFilter? filter = null) : base(source, filter)
    {
    }
    
    public string? IconImage { get; init; } 

    public IStyleProperty<float>? IconSize { get; init; }

    public IStyleProperty<Color>? IconColor { get; init; }

    public IStyleProperty<float>? IconOpacity { get; init; }
    
    public IStyleProperty<float>? IconRotate { get; init; }

    public IStyleProperty<float>? TextSize { get; init; }

    public IStyleProperty<Color>? TextColor { get; init; }

    public IStyleProperty<float>? TextOpacity { get; init; }
    
    public string? TextField { get; init; }
    
    public string? TextFont { get; init; }
    
    public string? TextAnchor { get; init; }

}