using System.Diagnostics.CodeAnalysis;

namespace VectorTiles.Values;

public readonly struct ConstBoolValue : IConstValue
{
    public object Value => _value;
    private readonly bool _value;

    public StyleConstValueType ValueType => StyleConstValueType.Bool;

    public ConstBoolValue(bool value)
    {
        _value = value;
    }

    public bool Equals(IConstValue? other)
    {
        if (other is null) return false;
        return other.ValueType switch
        {
            StyleConstValueType.Bool => _value == (bool)other.Value,
            _ => false
        };
    }

    public int CompareTo(IConstValue? other)
    {
        if (other is null) return 1;
        return other.ValueType switch
        {
            StyleConstValueType.Bool => _value.CompareTo((bool)other.Value),
            _ => 1
        };
    }

    public IConstValue Add(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Bool => new ConstBoolValue(_value || (bool)value.Value),
            _ => this
        };
    }

    public IConstValue Subtract(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Bool => new ConstBoolValue(_value && !(bool)value.Value),
            _ => this
        };
    }

    public IConstValue Multiply(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Bool => new ConstBoolValue(_value && (bool)value.Value),
            _ => this
        };
    }

    public IConstValue Divide(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Bool => new ConstBoolValue(_value && !(bool)value.Value),
            _ => this
        };
    }

    public IConstValue Modulo(IConstValue value)
    {
        return this;
    }
    
    public IConstValue Pow(IConstValue value)
    {
        return this;
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static implicit operator bool(ConstBoolValue value)
    {
        return value._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ConstBoolValue value && Equals(value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}