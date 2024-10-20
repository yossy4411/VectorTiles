namespace VectorTiles.Values;

public interface IConstValue
{
    bool Equals(IConstValue? other);
    int CompareTo(IConstValue? other);
    string ToString();
    IConstValue Add(IConstValue value);
    IConstValue Subtract(IConstValue value);
    IConstValue Multiply(IConstValue value);
    IConstValue Divide(IConstValue value);
    IConstValue Modulo(IConstValue value);
    
    object Value { get; }
    StyleConstValueType ValueType { get; }
}

public enum StyleConstValueType
{
    Int,
    Float,
    String,
    Bool,
    Color
}