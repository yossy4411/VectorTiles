namespace VectorTiles.Styles.Values;

public interface IStyleValues<out T>
{
    public T GetValue(float zoom, Dictionary<string, object?>? values = null);
}