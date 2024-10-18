using System.Drawing;
using VectorTiles.Styles.Values;

namespace VectorTiles.Styles;

public delegate bool VectorMapFilter(Dictionary<string, object> values);

/// <summary>
/// Layer for drawing vector map
/// </summary>
public abstract class VectorMapStyleLayer
{
    public string? Source { get; }
    public int MinZoom { get; init; } = 0;
    public int MaxZoom { get; init; } = 22;
    public string? Id { get; init; }
    private VectorMapFilter? _filter;

    public static readonly Color DefaultColor = Color.White;
    
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
    public InterpolateSegments<Color>? BackgroundColor { get; init; }

    public Color GetBackgroundColor(float zoom) => BackgroundColor?.Interpolate(zoom) ?? DefaultColor;
}

public class VectorFillStyleLayer : VectorMapStyleLayer
{
    public VectorFillStyleLayer(string? source = null, VectorMapFilter? filter = null) : base(source, filter)
    {
    }
    
    public InterpolateSegments<Color>? FillColor { get; init; }
    
    public Color GetFillColor(float zoom) => FillColor?.Interpolate(zoom) ?? DefaultColor;
}

/// <summary>
/// Layer for drawing lines
/// </summary>
public class VectorLineStyleLayer : VectorMapStyleLayer
{
    public VectorLineStyleLayer(string? source = null, VectorMapFilter? filter = null) : base(source, filter)
    {
    }
    
    public InterpolateSegments<float>? LineWidth { get; init; }
    
    public float GetLineWidth(float zoom) => LineWidth?.Interpolate(zoom) ?? 1;
    
    /// <summary>
    /// Pattern of dashes and gaps to be used when drawing lines.
    /// </summary>
    /// <remarks>
    /// Set it as a virtual method to implement compatibility with other libraries such as SkiaSharp.
    /// When override this property, you can return null for getter because it is not used in the library.
    /// </remarks>
    public virtual float[]? DashArray { get; init; }
    
    public InterpolateSegments<Color>? LineColor { get; init; }
    
    public Color GetLineColor(float zoom) => LineColor?.Interpolate(zoom) ?? DefaultColor;
}

/// <summary>
/// Layer for drawing symbols and text
/// </summary>
public class VectorSymbolLayer : VectorMapStyleLayer
{
    public VectorSymbolLayer(string? source = null, VectorMapFilter? filter = null) : base(source, filter)
    {
    }

    public InterpolateSegments<float>? IconSize { get; init; }

    public float GetIconSize(float zoom) => IconSize?.Interpolate(zoom) ?? 1;

    public InterpolateSegments<Color>? IconColor { get; init; }

    public Color GetIconColor(float zoom) => IconColor?.Interpolate(zoom) ?? DefaultColor;

    public InterpolateSegments<float>? IconOpacity { get; init; }

    public float GetIconOpacity(float zoom) => IconOpacity?.Interpolate(zoom) ?? 1;

    public InterpolateSegments<float>? TextSize { get; init; }

    public float GetTextSize(float zoom) => TextSize?.Interpolate(zoom) ?? 1;

    public InterpolateSegments<Color>? TextColor { get; init; }

    public Color GetTextColor(float zoom) => TextColor?.Interpolate(zoom) ?? DefaultColor;

    public InterpolateSegments<float>? TextOpacity { get; init; }
    
    public float GetTextOpacity(float zoom) => TextOpacity?.Interpolate(zoom) ?? 1;
    
    public string? TextField { get; init; }
    
    public string? TextFont { get; init; }
    
    public string? TextAnchor { get; init; }

}