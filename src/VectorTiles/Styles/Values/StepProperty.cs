using VectorTiles.Styles.Filters;
using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

public class StepProperty : IStyleProperty
{
    public List<IStyleProperty> Values { get; }
    public List<IConstValue> Steps { get; }
    public IStyleProperty Key { get; }
    
    public StepProperty(List<IStyleProperty> values, List<IConstValue> stops, IStyleProperty key)
    {
        Values = values;
        Steps = stops;
        Key = key;
    }
    
    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values)
    {
        // Very similar to StepFilter, but returns the value instead of filtering
        var intValue = Key.GetValue(values);
        if (intValue is null) return null;
        if (intValue.CompareTo(Steps[0]) <= 0) return Values[0].GetValue(values);
                    
        if (Steps.Count == 1) return Values[1].GetValue(values);
        for (var i = 0; i < Steps.Count - 1; i++)
        {
            if (intValue.CompareTo(Steps[i]) >= 0 && intValue.CompareTo(Steps[i + 1]) < 0)
            {
                return Values[i + 1].GetValue(values);
            }
        }
        return Values[^1].GetValue(values);
    }
}