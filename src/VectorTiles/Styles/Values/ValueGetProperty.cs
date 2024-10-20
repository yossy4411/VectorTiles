namespace VectorTiles.Styles.Values;

/// <summary>
/// A property that gets a value from a dictionary
/// </summary>
public class ValueGetProperty<T> : IStyleProperty<T?>
{
    public string? Key { get; init; }
    
    public T? GetValue(Dictionary<string, object?>? values = null)
    {
        if (values is null || Key is null) return default;
        if (values.TryGetValue(Key, out var value) && value is T tValue)
        {
            return tValue;
        }

        return default;
    }
    
    public override string ToString()
    {
        return $"($'{Key}')";
    }
}