namespace VectorTiles.Styles.Filters;

public class LesserFilter<T> : IStyleValueFilter<T>
    where T : IComparable<T>
{
    public LesserFilter(string key, T value)
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
            return o.CompareTo(Value) < 0;
        }

        return false;
    }

    public string Key { get; init; }
    public T Value { get; init; }
}

public class LesserOrEqualFilter<T> : IStyleValueFilter<T>
    where T : IComparable<T>
{
    public LesserOrEqualFilter(string key, T value)
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
            return o.CompareTo(Value) <= 0;
        }

        return false;
    }

    public string Key { get; init; }
    public T Value { get; init; }
}