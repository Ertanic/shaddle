using Shaddle.Values;

namespace Shaddle;

public class KdlNode(string name, string? type = null)
{
    public string Name { get; } = name;
    
    public string? Type { get; } = type;
    
    public IReadOnlyDictionary<string, KdlValue> Properties { get; init; } = new Dictionary<string, KdlValue>();

    public IReadOnlyCollection<KdlValue> Arguments { get; init; } = new List<KdlValue>();

    public KdlDocument? Children { get; init; }
}