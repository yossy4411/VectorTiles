namespace VectorTiles.Styles.Values;

/// <summary>
/// A property that gets a static value
/// </summary>
/// <typeparam name="T"></typeparam>
public class StaticValueProperty<T> : IStyleProperty<T?>
{
    private readonly T? _value;
    
    public StaticValueProperty(T? value)
    {
        _value = value;
    }

    public T? GetValue(float zoom, Dictionary<string, object?>? values = null)
    {
        return _value;
    }
}