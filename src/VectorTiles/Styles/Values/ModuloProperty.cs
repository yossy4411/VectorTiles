using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

public class ModuloProperty : IStyleProperty
{
    public IStyleProperty Value { get; }
    public IStyleProperty Modulo { get; }
    
    public ModuloProperty(IStyleProperty value, IStyleProperty modulo)
    {
        Value = value;
        Modulo = modulo;
    }
    
    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value = Value.GetValue(values);
        var modulo = Modulo.GetValue(values);
        if (value is null || modulo is null) return null;
        return value.Modulo(modulo);
    }
    
    public override string ToString()
    {
        return $"( {Value} % {Modulo} )";
    }
}