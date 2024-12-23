using VectorTiles.Styles.Values;
using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class InFilter : IStyleFilter
{
    public InFilter(IStyleProperty key, List<IConstValue> values)
    {
        Key = key;
        Value = values;
    }

    public IStyleProperty Key { get; init; }
    public List<IConstValue> Value { get; init; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value is not null && Value.Contains(value);
    }

    public override string ToString()
    {
        return $"( {Key} IN {string.Join(", ", Value)} )";
    }
}

public class NotInFilter : IStyleFilter
{
    public NotInFilter(IStyleProperty key, List<IConstValue> values)
    {
        Key = key;
        Value = values;
    }

    public IStyleProperty Key { get; init; }
    public List<IConstValue> Value { get; init; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        if (values is null) return false;
        var value = Key.GetValue(values);
        return value is null || !Value.Contains(value);
    }

    public override string ToString()
    {
        return $"( {Key} NOT IN {string.Join(", ", Value)} )";
    }
}