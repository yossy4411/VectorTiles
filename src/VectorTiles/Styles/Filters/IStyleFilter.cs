using VectorTiles.Values;

namespace VectorTiles.Styles.Filters;

public interface IStyleFilter
{
    /// <summary>
    /// Filter values.
    /// </summary>
    /// <param name="values">values dictionary</param>
    /// <returns>True if the values pass the filter, otherwise false</returns>
    public bool Filter(Dictionary<string, IConstValue?>? values);
}

public class FalseFilter : IStyleFilter
{
    public static readonly FalseFilter Instance = new FalseFilter();
    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return false;
    }
    
    public override string ToString()
    {
        return "false";
    }
}

public class TrueFilter : IStyleFilter
{
    public static readonly TrueFilter Instance = new TrueFilter();
    public bool Filter(Dictionary<string, IConstValue?>? values)
    {
        return true;
    }
    
    public override string ToString()
    {
        return "true";
    }
}