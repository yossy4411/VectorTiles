namespace VectorTiles.Values;

public class ConstStringValue : IConstValue
{
    private readonly string _value;

    public ConstStringValue(string value)
    {
        _value = value;
    }

    public object Value => _value;

    public StyleConstValueType ValueType => StyleConstValueType.String;

    public bool Equals(IConstValue? other)
    {
        if (other is null) return false;
        return other.ValueType switch
        {
            StyleConstValueType.String => _value == (string)other.Value,
            _ => false
        };
    }

    public int CompareTo(IConstValue? other)
    {
        if (other is null) return 1;
        return other.ValueType switch
        {
            StyleConstValueType.String => string.Compare(_value, (string)other.Value, StringComparison.Ordinal),
            _ => 1
        };
    }

    public IConstValue Add(IConstValue value)
    {
        return value.ValueType switch
        {
            StyleConstValueType.String => new ConstStringValue(_value + (string)value.Value),
            _ => this
        };
    }

    public IConstValue Subtract(IConstValue value)
    {
        return this;
    }

    public IConstValue Multiply(IConstValue value)
    {
        return this;
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
        return _value;
    }

    public static implicit operator string(ConstStringValue value)
    {
        return value._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ConstStringValue other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}