using System.Drawing;

namespace VectorTiles.Styles.Values;

public abstract class InterpolateSegment<T>
{
    public float Zoom { get; }
    public T Value { get; }

    protected InterpolateSegment(float zoom, T value)
    {
        Zoom = zoom;
        Value = value;
    }
    
    public void Deconstruct(out float zoom, out T value)
    {
        zoom = Zoom;
        value = Value;
    }
    
    /// <summary>
    /// 間の値を補間する
    /// </summary>
    /// <param name="others">他の値</param>
    /// <param name="rate">この値の割合</param>
    /// <returns>補間された値</returns>
    public abstract T Interpolate(T others, float rate);
}

public class InterpolateSegmentFloat : InterpolateSegment<float>
{
    public InterpolateSegmentFloat(float zoom, float value) : base(zoom, value)
    {
        
    }

    public override float Interpolate(float others, float rate)
    {
        return Value + (others - Value) * rate;
    }
}

public class InterpolateSegmentColor : InterpolateSegment<Color>
{
    public InterpolateSegmentColor(float zoom, Color value) : base(zoom, value)
    {
        
    }

    public override Color Interpolate(Color others, float rate)
    {
        return Color.FromArgb(
            (byte) (Value.A + (others.A - Value.A) * rate),
            (byte) (Value.R + (others.R - Value.R) * rate),
            (byte) (Value.G + (others.G - Value.G) * rate),
            (byte) (Value.B + (others.B - Value.B) * rate)
        );
    }
}