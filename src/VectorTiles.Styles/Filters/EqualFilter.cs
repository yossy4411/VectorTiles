using VectorTiles.Styles.Values;
using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

/// <summary>
///     Check if a value is equal to a constant value
/// </summary>
public class EqualFilter : IStyleFilter
{
    public EqualFilter(IStyleProperty key, IConstValue value)
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
        return value?.Equals(Value) ?? false;
    }

    public override string ToString()
    {
        return $"( {Key} == {Value} )";
    }
}

/// <summary>
///     Check if a value is not equal to a constant value
/// </summary>
public class NotEqualFilter : IStyleFilter
{
    public NotEqualFilter(IStyleProperty key, IConstValue value)
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
        return !value?.Equals(Value) ?? false;
    }

    public override string ToString()
    {
        return $"( {Key} != {Value} )";
    }
}