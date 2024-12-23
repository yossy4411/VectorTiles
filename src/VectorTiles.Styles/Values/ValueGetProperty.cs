using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
///     A property that gets a value from a dictionary
/// </summary>
public class ValueGetProperty : IStyleProperty
{
    public string? Key { get; init; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null || Key is null) return default;
        return values.GetValueOrDefault(Key);
    }

    public override string ToString()
    {
        return $"( ${Key} )";
    }
}