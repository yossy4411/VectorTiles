namespace VectorTiles.Styles.Values;

public interface IStyleProperty<out T>
{
    public T GetValue(float zoom, Dictionary<string, object?>? values = null);
}