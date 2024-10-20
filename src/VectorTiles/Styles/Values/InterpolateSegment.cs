using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

public class InterpolateSegment
{
    public float Zoom { get; }
    public IStyleProperty Value { get; }

    public InterpolateSegment(float zoom, IStyleProperty value)
    {
        Zoom = zoom;
        Value = value;
    }
    
    public void Deconstruct(out float zoom, out IStyleProperty value)
    {
        zoom = Zoom;
        value = Value;
    }

    /// <summary>
    /// 間の値を補間する
    /// </summary>
    /// <param name="values"></param>
    /// <param name="others">他の値</param>
    /// <param name="rate">この値の割合</param>
    /// <returns>補間された値</returns>
    public IConstValue? Interpolate(Dictionary<string, IConstValue?> values, IStyleProperty others, float rate)
    {
        var thisValue = Value.GetValue(values);
        var otherValue = others.GetValue(values);
        
        if (thisValue is null || otherValue is null) return null;
        return thisValue.Add(otherValue.Subtract(thisValue).Multiply(new ConstFloatValue(rate)));
    }
    
    public override string ToString()
    {
        return $"( {Zoom}, {Value} )";
    }
}