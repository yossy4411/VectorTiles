using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
/// Interface for style property
/// </summary>
/// <typeparam name="T">Type of the return value</typeparam>
public interface IStyleProperty
{
    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null);
}


/// <summary>
/// A wrapper class for a style property
/// </summary>
/// <typeparam name="T"></typeparam>
public class StyleProperty<T>
{
    public IStyleProperty Property { get; init; }
    
    public StyleProperty(IStyleProperty property)
    {
        Property = property;
    }
    
    public T? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value = Property.GetValue(values);
        if (value is T tValue)
        {
            return tValue;
        }
        return default;
    }
}

public static class StylePropertyExtensions
{
    public static StyleProperty<T> Wrap<T>(this IStyleProperty property)
    {
        return new StyleProperty<T>(property);
    }
}