using Pidgin;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Keywords
{
    [Fact]
    public void Parse_Null()
    {
        const string val = "#null";
        var e = new KdlNullValue();
        var actual = KdlParser.Keywords.ParseOrThrow(val);
        Assert.Equal(e.Type, actual.Type);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("#Null")]
    public void Parse_WrongNull(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.Keywords.ParseOrThrow(s));
    }
    
    [Theory]
    [InlineData("#true", true)]
    [InlineData("#false", false)]
    public void Parse_Boolean(string s, bool e)
    {
        var actual = KdlParser.Keywords.ParseOrThrow(s) as KdlBooleanValue;
        Assert.Equal(e, actual?.Value);
    }
    
    [Theory]
    [InlineData("#True")]
    [InlineData("#False")]
    [InlineData("false")]
    [InlineData("true")]
    public void Parse_WrongBoolean(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.Keywords.ParseOrThrow(s));
    }

    [Theory]
    [InlineData("#-inf", double.NegativeInfinity)]
    [InlineData("#inf", double.PositiveInfinity)]
    [InlineData("#nan", double.NaN)]
    public void Parse_NumberKeywords(string s, double e)
    {
        var actual = KdlParser.Keywords.ParseOrThrow(s) as KdlNumberValue;
        Assert.Equal(e, actual?.Value);
    }
}