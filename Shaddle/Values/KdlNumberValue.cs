using System.Globalization;
using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public sealed class KdlNumberValue(double value) : KdlValue<double>(value, nameof(Double)), ISerializable
{
    public void BuildKdlString(StringBuilder builder) => builder.Append(Value.ToString(NumberFormatInfo.InvariantInfo));
}
