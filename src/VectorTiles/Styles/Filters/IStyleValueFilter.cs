using VectorTiles.Styles.Values;

namespace VectorTiles.Styles.Filters;

public interface IStyleValueFilter<out T1, out T2> : IStyleFilter
{
    public IStyleProperty<T1?> Key { get; }
    public T2 Value { get; }
}