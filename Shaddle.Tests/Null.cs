using Pidgin;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Null
{
    [Fact]
    public void Parse_Null()
    {
        const string val = "#null";
        var e = new KdlNullValue();
        var actual = KdlParser.Null.ParseOrThrow(val);
        Assert.Equal(e.Type, actual.Type);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("#Null")]
    public void Parse_WrongNull(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.Null.ParseOrThrow(s));
    }
}