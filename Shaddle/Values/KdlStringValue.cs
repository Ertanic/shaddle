using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public class KdlStringValue(string value) : KdlValue<string>(value, nameof(String)), ISerializable
{
    public void BuildKdlPrettyString(StringBuilder builder) => builder.Append($"\"{Value}\"");

    public void BuildKdlString(StringBuilder builder) => builder.Append($"\"{Value}\"");
}