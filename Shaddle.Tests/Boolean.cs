using Pidgin;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Boolean
{
    [Theory]
    [InlineData("#true", true)]
    [InlineData("#false", false)]
    public void Parse_Boolean(string s, bool e)
    {
        var actual = KdlParser.Boolean.ParseOrThrow(s) as KdlBooleanValue;
        Assert.Equal(e, actual?.Value);
    }
    
    [Theory]
    [InlineData("#True")]
    [InlineData("#False")]
    [InlineData("false")]
    [InlineData("true")]
    public void Parse_WrongBoolean(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.Boolean.ParseOrThrow(s));
    }
}