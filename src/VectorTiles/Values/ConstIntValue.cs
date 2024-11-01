using System.Diagnostics.CodeAnalysis;

namespace VectorTiles.Values;

public readonly struct ConstIntValue : IConstValue
{
    public object Value => _value;
    private readonly int _value;


    public StyleConstValueType ValueType => StyleConstValueType.Int;

    public ConstIntValue(int value)
    {
        _value = value;
    }

    public bool Equals(IConstValue? other)
    {
        if (other is null) return false;
        return other.ValueType switch
        {
            StyleConstValueType.Int => _value == (int)other.Value,
            StyleConstValueType.Float => Math.Abs(_value - (float)other.Value) < 0.01,
            _ => false
        };
    }

    public int CompareTo(IConstValue? other)
    {
        if (other is null) return 1;
        return other.ValueType switch
        {
            StyleConstValueType.Int => _value.CompareTo((int)other.Value),
            StyleConstValueType.Float => ((float)other.Value).CompareTo(_value),
            _ => 1
        };
    }

    public IConstValue Add(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstIntValue(_value + (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value + (float)value.Value),
            _ => this
        };
    }

    public IConstValue Subtract(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstIntValue(_value - (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value - (float)value.Value),
            _ => this
        };
    }

    public IConstValue Multiply(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstIntValue(_value * (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value * (float)value.Value),
            _ => this
        };
    }

    public IConstValue Divide(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstIntValue(_value / (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value / (float)value.Value),
            _ => this
        };
    }

    public IConstValue Modulo(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstIntValue(_value % (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value % (float)value.Value),
            _ => this
        };
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static implicit operator int(ConstIntValue value)
    {
        return value._value;
    }

    public static implicit operator float(ConstIntValue value)
    {
        return value._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IConstValue value && Equals(value);
    }

    public override int GetHashCode()
    {
        return _value;
    }
}