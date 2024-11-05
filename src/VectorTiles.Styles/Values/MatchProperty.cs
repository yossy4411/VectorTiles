using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
///     A property that matches a value to a key
/// </summary>
public class MatchProperty : IStyleProperty
{
    public MatchProperty(IStyleProperty key, List<(IConstValue[], IStyleProperty)> values, IStyleProperty defaultValue)
    {
        Key = key;
        Values = values;
        DefaultValue = defaultValue;
    }

    public IStyleProperty Key { get; init; }
    public List<(IConstValue[], IStyleProperty)> Values { get; init; }

    public IStyleProperty DefaultValue { get; init; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values)
    {
        if (values is null) return null;
        var value = Key.GetValue(values);
        foreach (var v in Values.Where(v => v.Item1.Any(a => a.Equals(value)))) return v.Item2.GetValue(values);

        return DefaultValue.GetValue(values);
    }

    public override string ToString()
    {
        return
            $"( {Key} MATCH {string.Join(", ", Values.Select(v => $"({string.Join<IConstValue>(", ", v.Item1)} -> {v.Item2})"))} )";
    }
}