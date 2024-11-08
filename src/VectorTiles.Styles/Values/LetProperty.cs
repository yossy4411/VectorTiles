using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

public class LetProperty : IStyleProperty
{
    public List<(string, IStyleProperty)> Variables { get; }
    
    public IStyleProperty? Value { get; }
    
    public LetProperty(List<(string, IStyleProperty)> variables, IStyleProperty? value)
    {
        Variables = variables;
        Value = value;
    }
    
    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null) return null;
        // I didn't copy the dictionary here because it causes a performance issue
        foreach (var (key, value) in Variables)
        {
            values[key] = value.GetValue(values);
        }
        var result = Value?.GetValue(values);
        foreach (var (key, _) in Variables)
        {
            values.Remove(key);
        }

        return result;
    }
}