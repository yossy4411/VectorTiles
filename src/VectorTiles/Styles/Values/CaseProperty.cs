using VectorTiles.Styles.Filters;
using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
/// Case property for switching between different properties based on filters
/// </summary>
public class CaseProperty : IStyleProperty
{
    public List<(IStyleFilter Filter, IConstValue Property)> Cases { get; }
    public IConstValue Default { get; }
    
    public CaseProperty(List<(IStyleFilter Filter, IConstValue Property)> cases, IConstValue @default)
    {
        Cases = cases;
        Default = @default;
    }
    
    public IConstValue GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null) return Default;
        
        foreach (var (filter, property) in Cases)
        {
            if (filter.Filter(values))
            {
                return property;
            }
        }

        return Default;
    }

    public override string ToString()
    {
        return $"(CASE {string.Join(" ", Cases.Select(c => $"WHEN {c.Filter} THEN {c.Property}").Append($"ELSE {Default}"))} END)";
    }
}