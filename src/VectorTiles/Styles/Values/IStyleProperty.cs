namespace VectorTiles.Styles.Values;

public interface IStyleProperty<out T>
{
    public T GetValue(Dictionary<string, object?>? values = null);
}