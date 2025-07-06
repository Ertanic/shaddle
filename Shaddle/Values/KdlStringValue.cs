using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public class KdlStringValue(string value) : KdlValue<string>(value, nameof(String)), ISerializable
{
    public void BuildKdlString(StringBuilder builder) => builder.Append($"\"{Value}\"");
}