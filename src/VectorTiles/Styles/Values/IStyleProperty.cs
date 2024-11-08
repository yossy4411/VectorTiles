using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
///     Interface for style property
/// </summary>
/// <typeparam name="T">Type of the return value</typeparam>
public interface IStyleProperty
{
    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null);
}
