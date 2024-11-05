using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
///     Interpolate segments
/// </summary>
public class InterpolateProperty : IStyleProperty
{
    private readonly IStyleProperty? _key;

    private readonly List<InterpolateSegment> _segments;
    private readonly InterpolateType _type;

    public InterpolateProperty(InterpolateType type, IStyleProperty? key, List<InterpolateSegment>? segments = null)
    {
        _segments = segments ?? new List<InterpolateSegment>();
        _type = type;
        _key = key;
    }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null) return default;
        var zoomValue = _key?.GetValue(values);
        if (zoomValue is not ConstFloatValue zoom) return default;

        switch (_type)
        {
            case InterpolateType.Exponential:
            {
                // todo: Implement exponential interpolation
                if (_segments.Count == 1) return _segments[0].Value.GetValue(values);
                
                if (zoom < _segments[0].Zoom) return _segments[0].Value.GetValue(values);
                if (zoom >= _segments[^1].Zoom) return _segments[^1].Value.GetValue(values);
                
                var (a, (zoomB, valueB)) =
                    _segments.Zip(_segments.Skip(1)).First(x => x.First.Zoom <= zoom && zoom < x.Second.Zoom);
                var rate = (zoom - a.Zoom) / (zoomB - a.Zoom);
                return a.Interpolate(values, valueB, rate);
            }
            case InterpolateType.Linear:
            default:
            {
                if (_segments.Count == 1) return _segments[0].Value.GetValue(values);
                if (zoom < _segments[0].Zoom) return _segments[0].Value.GetValue(values);
                if (zoom >= _segments[^1].Zoom) return _segments[^1].Value.GetValue(values);
                
                var (a, (zoomB, valueB)) =
                    _segments.Zip(_segments.Skip(1)).First(x => x.First.Zoom <= zoom && zoom < x.Second.Zoom);
                var rate = (zoom - a.Zoom) / (zoomB - a.Zoom);
                return a.Interpolate(values, valueB, rate);
            }
        }
    }

    public void Add(InterpolateSegment segment)
    {
        _segments.Add(segment);
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