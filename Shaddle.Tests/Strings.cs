using Pidgin;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Strings
{
    [Theory]
    [InlineData("HelloWorld!", "HelloWorld!")]
    [InlineData("hello-world", "hello-world")]
    [InlineData("+helloworld", "+helloworld")]
    [InlineData("+.helloworld", "+.helloworld")]
    [InlineData("hello world", "hello")]
    [InlineData("hello=world", "hello")]
    public void Parse_UnquotedString(string s, string e)
    {
        var actual = KdlParser.UnquotedString.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("1HelloWorld")]
    [InlineData("+1HelloWorld")]
    [InlineData(".1HelloWorld")]
    [InlineData("+.1HelloWorld")]
    public void Parse_Nonidentifier_UnquotedString(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.UnquotedString.ParseOrThrow(s));
    }

    [Theory]
    [InlineData("\\r", '\r')]
    [InlineData("\\t", '\t')]
    [InlineData("\\b", '\b')]
    [InlineData("\\f", '\f')]
    [InlineData("\\s", '\u0020')]
    [InlineData("\\\"", '\"')]
    [InlineData("\\\\", '\\')]
    [InlineData("\\u{0020}", '\u0020')]
    public void Parse_Escape(string s, char e)
    {
        var actual = KdlParser.Escapes.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Fact]
    public void Parse_EscapeWhitespace()
    {
        var val = "\\    \nHelloWorld";
        var e = "\nHelloWorld";
        var actual = KdlParser.EscapeWhitespace
            .Then(KdlParser.Newline.Then(KdlParser.UnquotedString, (_, s) => '\n' + s)).ParseOrThrow(val);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"HelloWorld!\"", "HelloWorld!")]
    [InlineData("\"Hello World!\"", "Hello World!")]
    [InlineData("\"+.1HelloWorld\"", "+.1HelloWorld")]
    [InlineData("\"Hello \\      World\"", "Hello World")]
    [InlineData("\"Hello, \\\"World\\\"\"", "Hello, \"World\"")]
    [InlineData("\"Hello\\tWorld\"", "Hello\tWorld")]
    public void Parse_QuotedString(string s, string e)
    {
        var actual = KdlParser.QuotedString.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("#\"\"#", "")]
    [InlineData("#\"\"Hello, World!\"\"#", "\"Hello, World!\"")]
    public void Parse_RawString(string s, string e)
    {
        var actual = KdlParser.RawString.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"Hello \\      World\"", "Hello World")]
    [InlineData("#\"\"Hello, World!\"\"#", "\"Hello, World!\"")]
    public void Parse_String(string s, string e)
    {
        var actual = KdlParser.String.ParseOrThrow(s) as KdlStringValue;
        Assert.Equal(e, actual?.Value);
    }
}