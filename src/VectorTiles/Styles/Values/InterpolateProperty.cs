using VectorTiles.Values;

namespace VectorTiles.Styles.Values;

/// <summary>
/// Interpolate segments
/// </summary>
public class InterpolateProperty : IStyleProperty
{
    private readonly InterpolateType _type;
    
    private readonly IStyleProperty? _key;

    private readonly List<InterpolateSegment> _segments;

    public InterpolateProperty(InterpolateType type, IStyleProperty? key, List<InterpolateSegment>? segments = null)
    {
        _segments = segments ?? new List<InterpolateSegment>();
        _type = type;
        _key = key;
    }
    
    public void Add(InterpolateSegment segment)
    {
        _segments.Add(segment);
    }

    public IConstValue? GetValue(Dictionary<string, IConstValue?>? values = null)
    {
        if (values is null) return default;
        var zoomValue = _key?.GetValue(values);
        if (zoomValue is not ConstFloatValue zoom) return default;

        switch (_type)
        {
            case InterpolateType.Linear:
            default:
            {
                // 1点のみの場合
                if (_segments.Count == 1) return _segments[0].Value.GetValue(values);
                // 範囲外の場合
                if (zoom < _segments[0].Zoom) return _segments[0].Value.GetValue(values);
                if (zoom >= _segments[^1].Zoom) return _segments[^1].Value.GetValue(values);

                // 2点以上での線形補間
                var (a, (zoomB, valueB)) =
                    _segments.Zip(_segments.Skip(1)).First(x => x.First.Zoom <= zoom && zoom < x.Second.Zoom);
                var rate = (zoom - a.Zoom) / (zoomB - a.Zoom);
                return a.Interpolate(values, valueB, rate);
            }
        }
    }
    
    public override string ToString()
    {
        return $"( {string.Join(", ", _segments)} )";
    }
}

public enum InterpolateType
{
    Linear,
    // todo: Add more types
}