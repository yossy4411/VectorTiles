using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

public class ModuloProperty : IStyleProperty
{
    public ModuloProperty(IStyleProperty value, IStyleProperty modulo)
    {
        Value = value;
        Modulo = modulo;
    }

    public IStyleProperty Value { get; }
    public IStyleProperty Modulo { get; }

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

public class PlusProperty : IStyleProperty
{
    public PlusProperty(IStyleProperty value1, IStyleProperty value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public IStyleProperty Value1 { get; }
    public IStyleProperty Value2 { get; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value1 = Value1.GetValue(values);
        var value2 = Value2.GetValue(values);
        if (value1 is null || value2 is null) return null;
        return value1.Add(value2);
    }

    public override string ToString()
    {
        return $"( {Value1} + {Value2} )";
    }
}

public class MinusProperty : IStyleProperty
{
    public MinusProperty(IStyleProperty value1, IStyleProperty value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public IStyleProperty Value1 { get; }
    public IStyleProperty Value2 { get; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value1 = Value1.GetValue(values);
        var value2 = Value2.GetValue(values);
        if (value1 is null || value2 is null) return null;
        return value1.Subtract(value2);
    }

    public override string ToString()
    {
        return $"( {Value1} - {Value2} )";
    }
}

public class MultiplyProperty : IStyleProperty
{
    public MultiplyProperty(IStyleProperty value1, IStyleProperty value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public IStyleProperty Value1 { get; }
    public IStyleProperty Value2 { get; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value1 = Value1.GetValue(values);
        var value2 = Value2.GetValue(values);
        if (value1 is null || value2 is null) return null;
        return value1.Multiply(value2);
    }

    public override string ToString()
    {
        return $"( {Value1} * {Value2} )";
    }
}

public class DivideProperty : IStyleProperty
{
    public DivideProperty(IStyleProperty value1, IStyleProperty value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public IStyleProperty Value1 { get; }
    public IStyleProperty Value2 { get; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value1 = Value1.GetValue(values);
        var value2 = Value2.GetValue(values);
        if (value1 is null || value2 is null) return null;
        return value1.Divide(value2);
    }

    public override string ToString()
    {
        return $"( {Value1} / {Value2} )";
    }
}

public class PowerProperty : IStyleProperty
{
    public PowerProperty(IStyleProperty value1, IStyleProperty value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public IStyleProperty Value1 { get; }
    public IStyleProperty Value2 { get; }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        var value1 = Value1.GetValue(values);
        var value2 = Value2.GetValue(values);
        if (value1 is null || value2 is null) return null;
        return value1.Pow(value2);
    }

    public override string ToString()
    {
        return $"( {Value1} ^ {Value2} )";
    }
}