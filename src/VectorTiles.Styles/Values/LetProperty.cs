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
        var newValues = values is null ? new Dictionary<string, IConstValue?>() : new Dictionary<string, IConstValue?>(values);
        foreach (var (key, value) in Variables)
        {
            newValues[key] = value.GetValue(newValues);
        }
        return Value?.GetValue(newValues);
    }
}