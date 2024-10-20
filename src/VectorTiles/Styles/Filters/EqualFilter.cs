using VectorTiles.Styles.Values;

namespace VectorTiles.Styles.Filters;

/// <summary>
/// Filter for checking if two values are equal
/// </summary>
public class EqualFilter<T> : IStyleValueFilter<T, T>
{
    public IStyleProperty<T?> Key { get; init; }
    public T Value { get; init; }

    public EqualFilter(IStyleProperty<T?> key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value?.Equals(Value) ?? false;
    }
}

public class NotEqualFilter<T> : IStyleValueFilter<T, T>
{
    public NotEqualFilter(IStyleProperty<T?> key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return !value?.Equals(Value) ?? false;
    }

    public IStyleProperty<T?> Key { get; init; }
    public T Value { get; init; }
}