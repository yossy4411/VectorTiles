using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
/// A property that matches a value to a key
/// </summary>
public class MatchProperty : IStyleProperty
{
    public IStyleProperty Key { get; init; }
    public List<(IConstValue, IConstValue)> Values { get; init; }

    public IConstValue DefaultValue { get; init; }
    
    public MatchProperty(IStyleProperty key, List<(IConstValue, IConstValue)> values, IConstValue defaultValue)
    {
        Key = key;
        Values = values;
        DefaultValue = defaultValue;
    }
    
    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values)
    {
        if (values is null) return null;
        var value = Key.GetValue(values);
        foreach (var v in Values.Where(v => v.Item1.Equals(value)))
        {
            return v.Item2;
        }

        return DefaultValue;
    }
}