using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class AllFilter : IStyleFilter
{
    public AllFilter(List<IStyleFilter> filters)
    {
        Filters = filters;
    }

    public List<IStyleFilter> Filters { get; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is not null && Filters.All(f => f.Filter(values));
    }

    public override string ToString()
    {
        return $"( {string.Join(" && ", Filters)} )";
    }
}

public class NoneFilter : IStyleFilter
{
    public NoneFilter(List<IStyleFilter> filters)
    {
        Filters = filters;
    }

    public List<IStyleFilter> Filters { get; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is null || Filters.All(f => !f.Filter(values));
    }

    public override string ToString()
    {
        return $"( {string.Join(" && ", "!" + Filters)} )";
    }
}