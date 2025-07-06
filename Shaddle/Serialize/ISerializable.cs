using System.Text;

namespace Shaddle.Serialize;

public interface ISerializable
{
    void BuildKdlString(StringBuilder builder);

    public string ToKdlString()
    {
        var builder = new StringBuilder();
        BuildKdlString(builder);
        return builder.ToString();
    }
}