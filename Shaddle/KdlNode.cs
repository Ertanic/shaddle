using System.Diagnostics;
using System.Text;
using Shaddle.Serialize;
using Shaddle.Values;

namespace Shaddle;

public sealed class KdlNode(string name, string? type = null) : ISerializable
{
    public string Name { get; } = name;

    public string? Type { get; } = type;

    public IReadOnlyDictionary<string, KdlValue> Properties { get; init; } = new Dictionary<string, KdlValue>();

    public IReadOnlyCollection<KdlValue> Arguments { get; init; } = new List<KdlValue>();

    public KdlDocument? Children { get; init; }
    
    public void BuildKdlString(StringBuilder builder)
    {
        if (Type is not null)
        {
            builder.Append($"({type})");
        }

        builder.Append($"\"{Name}\"");

        foreach (var prop in Properties)
        {
            builder.Append($" {prop.Key}=");
            ValueToString(builder, prop.Value);
        }

        foreach (var arg in Arguments)
        {
            builder.Append(' ');
            ValueToString(builder, arg);
        }

        if (Children is not null)
        {
            builder.Append('{');

            Children.BuildKdlString(builder);

            builder.Append('}');
        }
    }

    private void ValueToString(StringBuilder builder, KdlValue kdlValue)
    {
        switch (kdlValue)
        {
            case KdlBooleanValue value:
                value.BuildKdlString(builder);
                break;
            case KdlStringValue value:
                value.BuildKdlString(builder);
                break;
            case KdlNullValue value:
                value.BuildKdlString(builder);
                break;
            case KdlNumberValue value:
                value.BuildKdlString(builder);
                break;
            case KdlValue<string> value:
                value.BuildKdlString(builder);
                break;
            default:
                throw new UnreachableException();
        }
    }
}