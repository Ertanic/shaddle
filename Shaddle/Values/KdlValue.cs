namespace Shaddle.Values;

public abstract class KdlValue(string? type)
{
    public string? Type { get; } = type;
}

public class KdlValue<TValue>(TValue value, string? type) : KdlValue(type)
{
    public TValue Value { get; } = value;
}