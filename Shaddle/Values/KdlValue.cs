using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public abstract class KdlValue(string? type)
{
    public string? Type { get; } = type;
}

public class KdlValue<TValue>(TValue value, string? type) : KdlValue(type), ISerializable
{
    public TValue Value { get; } = value;

    public void BuildKdlString(StringBuilder builder)
    {
        if (Type is not null)
        {
            builder.Append($"({Type})");
        }

        builder.Append($"\"{Value}\"");
    }
}