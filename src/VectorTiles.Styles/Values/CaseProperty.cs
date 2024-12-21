using VectorTiles.Styles.Filters;
using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
///     Case property for switching between different properties based on filters
/// </summary>
public class CaseProperty : IStyleProperty
{
    public CaseProperty(List<(IStyleFilter Filter, IStyleProperty Property)> cases, IStyleProperty @default)
    {
        Cases = cases;
        Default = @default;
    }

    public List<(IStyleFilter Filter, IStyleProperty Property)> Cases { get; }
    public IStyleProperty Default { get; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null) return Default.GetValue(values);

        foreach (var (filter, property) in Cases)
            if (filter.Filter(values))
                return property.GetValue(values);

        return Default.GetValue(values);
    }

    public override string ToString()
    {
        return
            $"(CASE {string.Join(" ", Cases.Select(c => $"WHEN {c.Filter} THEN {c.Property}").Append($"ELSE {Default}"))} END)";
    }
}