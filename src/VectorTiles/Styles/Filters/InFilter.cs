namespace VectorTiles.Styles.Filters;

public class InFilter<T> : IStyleValueFilter<IEnumerable<T>>
{
    public InFilter(string key, IEnumerable<T> values)
    {
        Key = key;
        Value = values;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        if (!values.TryGetValue(Key, out var value)) return false;
        if (value is T o)
        {
            return Value.Contains(o);
        }

        return false;
    }

    public string Key { get; init; }
    public IEnumerable<T> Value { get; init; }
}

public class NotInFilter<T> : IStyleValueFilter<IEnumerable<T>>
{
    public NotInFilter(string key, IEnumerable<T> values)
    {
        Key = key;
        Value = values;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        if (!values.TryGetValue(Key, out var value)) return false;
        if (value is T o)
        {
            return !Value.Contains(o);
        }

        return false;
    }

    public string Key { get; init; }
    public IEnumerable<T> Value { get; init; }
}