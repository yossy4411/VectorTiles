using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace VectorTiles.Values;

public readonly struct ConstFloatValue : IConstValue
{
    public object Value => _value;
    private readonly float _value;

    public StyleConstValueType ValueType => StyleConstValueType.Float;

    public ConstFloatValue(float value)
    {
        _value = value;
    }

    public bool Equals(IConstValue? other)
    {
        if (other is null) return false;
        return other.ValueType switch
        {
            StyleConstValueType.Int => Math.Abs(_value - (int)other.Value) < 0.01,
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
            StyleConstValueType.Float => _value.CompareTo((float)other.Value),
            _ => 1
        };
    }

    public IConstValue Add(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstFloatValue(_value + (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value + (float)value.Value),
            _ => this
        };
    }

    public IConstValue Subtract(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstFloatValue(_value - (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value - (float)value.Value),
            _ => this
        };
    }

    public IConstValue Multiply(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstFloatValue(_value * (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value * (float)value.Value),
            _ => this
        };
    }

    public IConstValue Divide(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstFloatValue(_value / (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value / (float)value.Value),
            _ => this
        };
    }

    public IConstValue Modulo(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstFloatValue(_value % (int)value.Value),
            StyleConstValueType.Float => new ConstFloatValue(_value % (float)value.Value),
            _ => this
        };
    }
    
    public IConstValue Pow(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Int => new ConstFloatValue(MathF.Pow(_value, (int)value.Value)),
            StyleConstValueType.Float => new ConstFloatValue(MathF.Pow(_value, (float)value.Value)),
            _ => this
        };
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }
    
    public int ToInt()
    {
        return (int)_value;
    }
    
    public float ToFloat()
    {
        return _value;
    }
    
    public bool ToBool()
    {
        return Math.Abs(_value) > 0.01;
    }
    
    public Color ToColor()
    {
        return Color.FromArgb((int)_value);
    }

    public static implicit operator float(ConstFloatValue value)
    {
        return value._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IConstValue value && Equals(value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}