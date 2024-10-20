using VectorTiles.Styles.Values;

namespace VectorTiles.Styles.Filters;

public class InFilter<T> : IStyleValueFilter<T, List<T>>
{
    public InFilter(IStyleProperty<T?> key, List<T> values)
    {
        Key = key;
        Value = values;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value is not null && Value.Contains(value);
    }

    public IStyleProperty<T?> Key { get; init; }
    public List<T> Value { get; init; }
}

public class NotInFilter<T> : IStyleValueFilter<T, List<T>>
{
    public NotInFilter(IStyleProperty<T?> key, List<T> values)
    {
        Key = key;
        Value = values;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value is null || !Value.Contains(value);
    }

    public IStyleProperty<T?> Key { get; init; }
    public List<T> Value { get; init; }
}