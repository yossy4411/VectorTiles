using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
/// A property that gets a static value
/// </summary>
public class StaticValueProperty : IStyleProperty
{
    private readonly IConstValue? _value;
    
    public StaticValueProperty(IConstValue? value)
    {
        _value = value;
    }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? _ = null)
    {
        return _value;
    }

    public override string ToString()
    {
        return $"( {_value} )";
    }
}