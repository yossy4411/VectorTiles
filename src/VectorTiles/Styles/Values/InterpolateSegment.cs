using System.Drawing;

namespace VectorTiles.Styles.Values;

public abstract class InterpolateSegment<T>
{
    public float Zoom { get; }
    public IStyleProperty<T> Value { get; }

    protected InterpolateSegment(float zoom, IStyleProperty<T> value)
    {
        Zoom = zoom;
        Value = value;
    }
    
    public void Deconstruct(out float zoom, out IStyleProperty<T> value)
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
    public abstract T Interpolate(Dictionary<string, object?> values, IStyleProperty<T> others, float rate);
    
    public override string ToString()
    {
        return $"( {Zoom}, {Value} )";
    }
}

public class InterpolateSegmentFloat : InterpolateSegment<float>
{
    public InterpolateSegmentFloat(float zoom, IStyleProperty<float> value) : base(zoom, value)
    {
        
    }

    public override float Interpolate(Dictionary<string, object?> values, IStyleProperty<float> others, float rate)
    {
        var thisValue = Value.GetValue(values);
        var otherValue = others.GetValue(values);
        return thisValue + (otherValue - thisValue) * rate;
    }
}

public class InterpolateSegmentColor : InterpolateSegment<Color>
{
    public InterpolateSegmentColor(float zoom, IStyleProperty<Color> value) : base(zoom, value)
    {
        
    }

    public override Color Interpolate(Dictionary<string, object?> values, IStyleProperty<Color> others, float rate)
    {
        var thisValue = Value.GetValue(values);
        var otherValue = others.GetValue(values);
        return Color.FromArgb(
            (byte) (thisValue.A + (otherValue.A - thisValue.A) * rate),
            (byte) (thisValue.R + (otherValue.R - thisValue.R) * rate),
            (byte) (thisValue.G + (otherValue.G - thisValue.G) * rate),
            (byte) (thisValue.B + (otherValue.B - thisValue.B) * rate)
        );
    }
}