using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class AnyFilter : IStyleFilter
{
    public AnyFilter(List<IStyleFilter> filters)
    {
        Filters = filters;
    }

    public List<IStyleFilter> Filters { get; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is not null && Filters.Any(f => f.Filter(values));
    }

    public override string ToString()
    {
        return $"( {string.Join(" || ", Filters)} )";
    }
}