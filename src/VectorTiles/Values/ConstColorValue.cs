using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace VectorTiles.Values;

public readonly struct ConstColorValue : IConstValue
{
    public object Value => _value;

    private readonly Color _value;

    public StyleConstValueType ValueType => StyleConstValueType.Color;

    public ConstColorValue(Color value)
    {
        _value = value;
    }

    public bool Equals(IConstValue? other)
    {
        if (other is null) return false;
        return other.ValueType switch
        {
            StyleConstValueType.Color => _value == (Color)other.Value,
            _ => false
        };
    }

    public int CompareTo(IConstValue? other)
    {
        if (other is null) return 1;
        return other.ValueType switch
        {
            StyleConstValueType.Color => _value.ToArgb().CompareTo(((Color)other.Value).ToArgb()),
            _ => 1
        };
    }

    public IConstValue Add(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Color => new ConstColorValue(
                Color.FromArgb(_value.ToArgb() + ((Color)value.Value).ToArgb())),
            _ => this
        };
    }

    public IConstValue Subtract(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Color => new ConstColorValue(
                Color.FromArgb(_value.ToArgb() - ((Color)value.Value).ToArgb())),
            _ => this
        };
    }

    public IConstValue Multiply(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.Float => new ConstColorValue(Color.FromArgb(
                (int)(_value.A * (float)value.Value),
                (int)(_value.R * (float)value.Value),
                (int)(_value.G * (float)value.Value),
                (int)(_value.B * (float)value.Value)
            )),
            StyleConstValueType.Int => new ConstColorValue(Color.FromArgb(
                _value.A * (int)value.Value,
                _value.R * (int)value.Value,
                _value.G * (int)value.Value,
                _value.B * (int)value.Value
            )),
            _ => this
        };
    }

    public IConstValue Divide(IConstValue value)
    {
        return this;
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
    
    public int ToInt()
    {
        return _value.ToArgb();
    }
    
    public float ToFloat()
    {
        return _value.ToArgb();
    }
    
    public bool ToBool()
    {
        return _value.ToArgb() != 0;
    }
    
    public Color ToColor()
    {
        return _value;
    }

    public static implicit operator Color(ConstColorValue value)
    {
        return value._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ConstColorValue value && Equals(value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}