using System.Globalization;
using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public sealed class KdlNumberValue(double value) : KdlValue<double>(value, nameof(Double)), ISerializable
{
    public void BuildKdlPrettyString(StringBuilder builder) => builder.Append(Value.ToString(NumberFormatInfo.InvariantInfo));

    public void BuildKdlString(StringBuilder builder) => builder.Append(Value.ToString(NumberFormatInfo.InvariantInfo));
}
