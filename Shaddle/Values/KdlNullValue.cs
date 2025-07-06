using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public class KdlNullValue() : KdlValue(null), ISerializable
{
    public void BuildKdlString(StringBuilder builder) => builder.Append("#null");
}
