using VectorTiles.Styles.Values;

namespace VectorTiles.Styles.Filters;

public class BiggerFilter<T> : IStyleValueFilter<T, T>
    where T : IComparable<T>
{
    public BiggerFilter(IStyleProperty<T> key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value.CompareTo(Value) > 0;

    }

    public IStyleProperty<T> Key { get; init; }
    public T Value { get; init; }
}

public class BiggerOrEqualFilter<T> : IStyleValueFilter<T, T>
    where T : IComparable<T>
{
    public BiggerOrEqualFilter(IStyleProperty<T> key, T value)
    {
        Key = key;
        Value = value;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value.CompareTo(Value) >= 0;
    }

    public IStyleProperty<T> Key { get; init; }
    public T Value { get; init; }
}