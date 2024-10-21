using VectorTiles.Styles.Values;
using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public class StepFilter : IStyleFilter
{
    public StepFilter(List<IStyleFilter> filters, List<IConstValue> stops, IStyleProperty key)
    {
        Filters = filters;
        Values = stops;
        Key = key;
    }

    public List<IStyleFilter> Filters { get; }
    public List<IConstValue> Values { get; }
    public IStyleProperty Key { get; }

    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        // ["step", ["get", "zoom"], <filter1>, 5, <filter2>, 10, <filter3>, 15, <filter4>]
        // 0-5: filter1, 5-10: filter2, 10-15: filter3, 15-: filter4
        var intValue = Key.GetValue(values);
        if (intValue is null) return false;
        if (intValue.CompareTo(Values[0]) <= 0) return Filters[0].Filter(values);

        if (Values.Count == 1) return Filters[1].Filter(values);
        // Range check
        for (var i = 0; i < Values.Count - 1; i++)
            if (intValue.CompareTo(Values[i]) >= 0 && intValue.CompareTo(Values[i + 1]) < 0)
                return Filters[i + 1].Filter(values);
        // exceeded the last stop, return the last filter
        return Filters[^1].Filter(values);
    }

    public override string ToString()
    {
        return $"( {Key} STEP {string.Join(", ", Values)} )";
    }
}