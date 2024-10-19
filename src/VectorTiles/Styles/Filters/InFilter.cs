namespace VectorTiles.Styles.Filters;

public class InFilter<T> : IStyleValueFilter<List<T>>
{
    public InFilter(string key, List<T> values)
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
    public List<T> Value { get; init; }
}

public class NotInFilter<T> : IStyleValueFilter<List<T>>
{
    public NotInFilter(string key, List<T> values)
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
    public List<T> Value { get; init; }
}