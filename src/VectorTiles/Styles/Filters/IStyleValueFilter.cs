namespace VectorTiles.Styles.Filters;

public interface IStyleValueFilter<out T> : IStyleFilter
{
    public string Key { get; }
    public T Value { get; }
}