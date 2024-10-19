namespace VectorTiles.Styles.Filters;

public class AnyFilter : IStyleFilter
{
    public List<IStyleFilter> Filters { get; }
    
    public AnyFilter(List<IStyleFilter> filters)
    {
        Filters = filters;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        return values is not null && Filters.Any(f => f.Filter(values));
    }
}