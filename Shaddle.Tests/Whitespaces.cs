using Pidgin;

namespace Shaddle.Tests;

public class Whitespaces
{
    [Theory]
    [InlineData("\u0009")]
    [InlineData("\u0020")]
    [InlineData("\u00A0")]
    [InlineData("\u1680")]
    [InlineData("\u2000")]
    [InlineData("\u2001")]
    [InlineData("\u2002")]
    [InlineData("\u2003")]
    [InlineData("\u2004")]
    [InlineData("\u2005")]
    [InlineData("\u2006")]
    [InlineData("\u2007")]
    [InlineData("\u2008")]
    [InlineData("\u2009")]
    [InlineData("\u200a")]
    [InlineData("\u202f")]
    [InlineData("\u205f")]
    [InlineData("\u3000")]
    public void Parse_Whitespace(string s)
    {
        KdlParser.Whitespace.ParseOrThrow(s);
    }

    [Theory]
    [InlineData("\u000D\u000A")]
    [InlineData("\u000D")]
    [InlineData("\u000A")]
    [InlineData("\u0085")]
    [InlineData("\u000B")]
    [InlineData("\u000C")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    public void Parse_Newline(string s)
    {
        KdlParser.Newline.ParseOrThrow(s);
    }
}