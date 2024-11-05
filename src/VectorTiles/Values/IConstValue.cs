namespace VectorTiles.Values;

public interface IConstValue
{
    object Value { get; }
    StyleConstValueType ValueType { get; }
    bool Equals(IConstValue? other);
    int CompareTo(IConstValue? other);
    string ToString();
    IConstValue Add(IConstValue value);
    IConstValue Subtract(IConstValue value);
    IConstValue Multiply(IConstValue value);
    IConstValue Divide(IConstValue value);
    IConstValue Modulo(IConstValue value);
    IConstValue Pow(IConstValue value);
}

public enum StyleConstValueType
{
    Int,
    Float,
    String,
    Bool,
    Color
}