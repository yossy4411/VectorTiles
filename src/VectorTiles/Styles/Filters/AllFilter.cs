using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class AllFilter : IStyleFilter
{
    public List<IStyleFilter> Filters { get; }
    
    public AllFilter(List<IStyleFilter> filters)
    {
        Filters = filters;
    }
    
    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is not null && Filters.All(f => f.Filter(values));
    }
}

public class NoneFilter : IStyleFilter
{
    public List<IStyleFilter> Filters { get; }
    
    public NoneFilter(List<IStyleFilter> filters)
    {
        Filters = filters;
    }
    
    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is null || Filters.All(f => !f.Filter(values));
    }
}

