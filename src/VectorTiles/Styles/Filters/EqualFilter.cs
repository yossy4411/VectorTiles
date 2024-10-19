namespace VectorTiles.Styles.Filters;

/// <summary>
/// Filter for checking if two values are equal
/// </summary>
public class EqualFilter<T> : IStyleValueFilter<T>
{
    public string Key { get; init; }
    public T Value { get; init; }

    public EqualFilter(string key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        if (values.TryGetValue(Key, out var value))
        {
            return value?.Equals(Value) ?? false;
        }

        return false;
    }
}

public class NotEqualFilter<T> : IStyleValueFilter<T>
{
    public NotEqualFilter(string key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        if (values.TryGetValue(Key, out var value))
        {
            return !value?.Equals(Value) ?? false;
        }

        return false;
    }

    public string Key { get; init; }
    public T Value { get; init; }
}