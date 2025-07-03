using System.Text;

namespace Shaddle.Serialize;

public interface ISerializable
{
    void BuildKdlString(StringBuilder builder);

    void BuildKdlPrettyString(StringBuilder builder);

    public string ToKdlString()
    {
        var builder = new StringBuilder();
        BuildKdlString(builder);
        return builder.ToString();
    }

    public string ToKdlPrettyString()
    {
        var builder = new StringBuilder();
        BuildKdlPrettyString(builder);
        return builder.ToString();
    }
}