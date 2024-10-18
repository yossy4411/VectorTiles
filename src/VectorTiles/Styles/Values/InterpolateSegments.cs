namespace VectorTiles.Styles.Values;

public class InterpolateSegments<T> : List<InterpolateSegment<T>>
{
    public InterpolateType Type { get; }
    
    public InterpolateSegments(InterpolateType type)
    {
        Type = type;
    }
    
    public T Interpolate(float zoom)
    {
        switch (Type)
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