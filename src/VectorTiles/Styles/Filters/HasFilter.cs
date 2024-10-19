namespace VectorTiles.Styles.Filters;

public class HasFilter : IStyleFilter
{
    public HasFilter(string key)
    {
        Key = key;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        return values is not null && values.ContainsKey(Key);
    }

    public string Key { get; init; }
}

public class NotHasFilter : IStyleFilter
{
    public NotHasFilter(string key)
    {
        Key = key;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        return values is null || !values.ContainsKey(Key);
    }

    public string Key { get; init; }
}