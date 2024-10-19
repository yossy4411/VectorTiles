namespace VectorTiles.Styles.Values;

public class ModuloProperty : IStyleProperty<float>
{
    public IStyleProperty<float> Value { get; }
    public IStyleProperty<float> Modulo { get; }
    
    public ModuloProperty(IStyleProperty<float> value, IStyleProperty<float> modulo)
    {
        Value = value;
        Modulo = modulo;
    }
    
    public float GetValue(Dictionary<string, object?>? values = null)
    {
        var value = Value.GetValue(values);
        var modulo = Modulo.GetValue(values);
        return value % modulo;
    }
}