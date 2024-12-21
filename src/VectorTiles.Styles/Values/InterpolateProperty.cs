using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
///     Interpolate segments
/// </summary>
public class InterpolateProperty : IStyleProperty
{
    private readonly IStyleProperty? _key;

    private readonly List<(float, IStyleProperty)> _segments;
    private readonly InterpolateType _type;
    private readonly float _factor;

    public InterpolateProperty(InterpolateType type, IStyleProperty? key, List<(float, IStyleProperty)>? segments = null, float factor = 1)
    {
        _segments = segments ?? new List<(float, IStyleProperty)>();
        _type = type;
        _key = key;
        _factor = factor;
    }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null) return default;
        var zoomValue = _key?.GetValue(values);
        if (zoomValue is not ConstFloatValue zoomValue2) return default;
        float zoom = zoomValue2;
        switch (_type)
        {
            case InterpolateType.Exponential:
            {
                if (_segments.Count == 1) return _segments[0].Item2.GetValue(values);
                
                if (zoom < _segments[0].Item1) return _segments[0].Item2.GetValue(values);
                if (zoom >= _segments[^1].Item1) return _segments[^1].Item2.GetValue(values);
                
                for (var i = 0; i < _segments.Count - 1; i++)
                {
                    var (za, a) = _segments[i];
                    var (zb, b) = _segments[i + 1];
                    if (!(za <= zoom) || !(zoom < zb)) continue;
                    var rate = (zoom - za) / (zb - za);
                    var thisValue = a.GetValue(values);
                    var otherValue = b.GetValue(values);

                    if (thisValue is null || otherValue is null) return null;
                    return thisValue.Multiply(new ConstFloatValue(MathF.Pow(1 - rate, _factor))).Add(otherValue.Multiply(new ConstFloatValue(
                        MathF.Pow(rate, _factor))));
                }
                return null;
            }
            case InterpolateType.Linear:
            default:
            {
                if (_segments.Count == 1) return _segments[0].Item2.GetValue(values);
                
                if (zoom < _segments[0].Item1) return _segments[0].Item2.GetValue(values);
                if (zoom >= _segments[^1].Item1) return _segments[^1].Item2.GetValue(values);
                
                for (var i = 0; i < _segments.Count - 1; i++)
                {
                    var (za, a) = _segments[i];
                    var (zb, b) = _segments[i + 1];
                    if (!(za <= zoom) || !(zoom < zb)) continue;
                    var rate = (zoom - za) / (zb - za);
                    var thisValue = a.GetValue(values);
                    var otherValue = b.GetValue(values);

                    if (thisValue is null || otherValue is null) return null;
                    return thisValue.Add(otherValue.Subtract(thisValue).Multiply(new ConstFloatValue(rate)));
                }
                return null;
            }
        }
    }

    public void Add(float zoom, IStyleProperty prop)
    {
        _segments.Add((zoom, prop));
    }

    public override string ToString()
    {
        var type = _type == InterpolateType.Exponential ? "EXPONENTIAL" : "LINEAR";
        return $"({type} INTERPOLATE {_key} WITH {string.Join(", ", _segments)} )";
    }
}

public enum InterpolateType
{
    Linear,
    Exponential
    // todo: Add more types
}