using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class HasFilter : IStyleFilter
{
    public HasFilter(string key)
    {
        Key = key;
    }

    public string Key { get; init; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is not null && values.ContainsKey(Key);
    }

    public override string ToString()
    {
        return $"( HAS {Key} )";
    }
}

public class NotHasFilter : IStyleFilter
{
    public NotHasFilter(string key)
    {
        Key = key;
    }

    public string Key { get; init; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return values is null || !values.ContainsKey(Key);
    }

    public override string ToString()
    {
        return $"( HAS {Key} )";
    }
}