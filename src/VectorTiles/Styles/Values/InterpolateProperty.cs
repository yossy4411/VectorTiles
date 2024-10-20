namespace VectorTiles.Styles.Values;

/// <summary>
/// Interpolate segments
/// </summary>
/// <typeparam name="T"></typeparam>
public class InterpolateProperty<T> : List<InterpolateSegment<T>>, IStyleProperty<T?>
{
    private readonly InterpolateType _type;
    
    private readonly IStyleProperty<float>? _key;
    
    public InterpolateProperty(InterpolateType type, IStyleProperty<float>? key)
    {
        _type = type;
        _key = key;
    }

    public virtual T? GetValue(Dictionary<string, object?>? values = null)
    {
        if (values is null) return default;
        if (_key is null) return default;
        var zoom = _key.GetValue(values);

        switch (_type)
        {
            case InterpolateType.Linear:
            default:
            {
                // 1点のみの場合
                if (Count == 1) return this[0].Value.GetValue(values);
                // 範囲外の場合
                if (zoom < this[0].Zoom) return this[0].Value.GetValue(values);
                if (zoom >= this[^1].Zoom) return this[^1].Value.GetValue(values);

                // 2点以上での線形補間
                var (a, (zoomB, valueB)) =
                    this.Zip(this.Skip(1)).First(x => x.First.Zoom <= zoom && zoom < x.Second.Zoom);
                var rate = (zoom - a.Zoom) / (zoomB - a.Zoom);
                return a.Interpolate(values, valueB, rate);
            }
        }
    }
    
    public override string ToString()
    {
        return $"( {string.Join(", ", this)} )";
    }
}

public enum InterpolateType
{
    Linear,
    // todo: Add more types
}