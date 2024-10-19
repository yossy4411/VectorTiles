namespace VectorTiles.Styles.Filters;

public class BiggerFilter<T> : IStyleValueFilter<T>
    where T : IComparable<T>
{
    public BiggerFilter(string key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        if (!values.TryGetValue(Key, out var value)) return false;
        if (value is T o)
        {
            return o.CompareTo(Value) > 0;
        }

        return false;
    }

    public string Key { get; init; }
    public T Value { get; init; }
}

public class BiggerOrEqualFilter<T> : IStyleValueFilter<T>
    where T : IComparable<T>
{
    public BiggerOrEqualFilter(string key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        if (!values.TryGetValue(Key, out var value)) return false;
        if (value is T o)
        {
            return o.CompareTo(Value) >= 0;
        }

        return false;
    }

    public string Key { get; init; }
    public T Value { get; init; }
}