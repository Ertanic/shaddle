using System.Text;
using Shaddle.Serialize;

namespace Shaddle.Values;

public class KdlBooleanValue(bool value) : KdlValue<bool>(value, nameof(Boolean)), ISerializable
{
    public void BuildKdlString(StringBuilder builder)
    {
        if (Value)
            builder.Append("#true");
        else
            builder.Append("#false");
    }
}
