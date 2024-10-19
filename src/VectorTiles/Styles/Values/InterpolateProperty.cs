namespace VectorTiles.Styles.Values;

/// <summary>
/// Interpolate segments
/// </summary>
/// <typeparam name="T"></typeparam>
public class InterpolateProperty<T> : List<InterpolateSegment<T>>, IStyleProperty<T?>
{
    private readonly InterpolateType _type;
    
    private readonly string _key;
    
    public InterpolateProperty(InterpolateType type, string key = "$zoom")
    {
        _type = type;
        _key = key;
    }

    public virtual T? GetValue(Dictionary<string, object?>? values = null)
    {
        if (values is null) return default;
        var zoom = values.TryGetValue(_key, out var zoomValue)
            ? zoomValue switch
            {
                int zoomI => zoomI,
                float zoomF => zoomF,
                double zoomD => (float)zoomD,
                _ => 0
            }
            : 0;

        switch (_type)
        {
            case InterpolateType.Linear:
            default:
            {
                // 1点のみの場合
                if (Count == 1) return this[0].Value;
                // 範囲外の場合
                if (zoom < this[0].Zoom) return this[0].Value;
                if (zoom >= this[^1].Zoom) return this[^1].Value;

                // 2点以上での線形補間
                var (a, (zoomB, valueB)) =
                    this.Zip(this.Skip(1)).First(x => x.First.Zoom <= zoom && zoom < x.Second.Zoom);
                var rate = (zoom - a.Zoom) / (zoomB - a.Zoom);
                return a.Interpolate(valueB, rate);
            }
        }
    }
}

public enum InterpolateType
{
    Linear,
    // todo: Add more types
}