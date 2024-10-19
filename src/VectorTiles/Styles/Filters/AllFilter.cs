namespace VectorTiles.Styles.Filters;

public class AllFilter : IStyleFilter
{
    IEnumerable<IStyleFilter> Filters { get; }
    
    public AllFilter(IEnumerable<IStyleFilter> filters)
    {
        Filters = filters;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        return values is not null && Filters.All(f => f.Filter(values));
    }
}

public class NoneFilter : IStyleFilter
{
    IEnumerable<IStyleFilter> Filters { get; }
    
    public NoneFilter(IEnumerable<IStyleFilter> filters)
    {
        Filters = filters;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        return values is null || Filters.All(f => !f.Filter(values));
    }
}

