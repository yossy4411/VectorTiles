using VectorTiles.Styles.Values;
using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class BiggerFilter : IStyleFilter
{
    public BiggerFilter(IStyleProperty key, IConstValue value)
    {
        Key = key;
        Value = value;
    }

    public IStyleProperty Key { get; init; }
    public IConstValue Value { get; init; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value != null && value.CompareTo(Value) > 0;
    }

    public override string ToString()
    {
        return $"( {Key} > {Value} )";
    }
}

public class BiggerOrEqualFilter : IStyleFilter
{
    public BiggerOrEqualFilter(IStyleProperty key, IConstValue value)
    {
        Key = key;
        Value = value;
    }

    public IStyleProperty Key { get; init; }
    public IConstValue Value { get; init; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value != null && value.CompareTo(Value) >= 0;
    }

    public override string ToString()
    {
        return $"( {Key} >= {Value} )";
    }
}