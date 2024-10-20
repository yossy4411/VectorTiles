using VectorTiles.Styles.Filters;

namespace VectorTiles.Styles.Values;

public class CaseProperty<T> : IStyleProperty<T>
{
    public List<(IStyleFilter Filter, IStyleProperty<T> Property)> Cases { get; }
    public IStyleProperty<T> Default { get; }
    
    public CaseProperty(List<(IStyleFilter Filter, IStyleProperty<T> Property)> cases, IStyleProperty<T> @default)
    {
        Cases = cases;
        Default = @default;
    }
    
    public T GetValue(Dictionary<string, object?>? values = null)
    {
        if (values is null) return Default.GetValue();
        
        foreach (var (filter, property) in Cases)
        {
            if (filter.Filter(values))
            {
                return property.GetValue(values);
            }
        }

        return Default.GetValue(values);
    }

    public override string ToString()
    {
        var str = "switch (";
        foreach (var (filter, property) in Cases)
        {
            str += $"case {filter} => return {property};";
        }
        str += $"default => return {Default};";
        return str;
    }
}