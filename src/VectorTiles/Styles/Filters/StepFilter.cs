namespace VectorTiles.Styles.Filters;

public class StepFilter : IStyleValueFilter<List<float>>
{
    public List<IStyleFilter> Filters { get; }
    public List<float> Value { get; }
    public string Key { get; }
    
    public StepFilter(List<IStyleFilter> filters, List<float> stops, string key)
    {
        Filters = filters;
        Value = stops;
        Key = key;
    }
    
    public bool Filter(Dictionary<string, object?>? values)
    {
        // ["step", ["get", "zoom"], <filter1>, 5, <filter2>, 10, <filter3>, 15, <filter4>]
        // 0-5: filter1, 5-10: filter2, 10-15: filter3, 15-: filter4
        if (values is null || !values.TryGetValue(Key, out var v)) return false;
        if (v is not float intValue) return false;
        if (intValue < Value[0]) return Filters[0].Filter(values);
                    
        if (Value.Count == 1) return Filters[1].Filter(values);
        // Range check
        for (var i = 0; i < Value.Count - 1; i++)
        {
            if (intValue >= Value[i] && intValue < Value[i + 1])
            {
                return Filters[i + 1].Filter(values);
            }
        }
        // exceeded the last stop, return the last filter
        return Filters[^1].Filter(values);
    }
}