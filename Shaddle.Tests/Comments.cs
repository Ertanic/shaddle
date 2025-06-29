using Pidgin;

namespace Shaddle.Tests;

public class Comments
{
    // [Theory]
    // [InlineData("//")]
    // [InlineData("// Hello World")]
    // [InlineData("//\n")]
    // [InlineData("//      \n")]
    // [InlineData("// Hello World\n// Goodbye Edward")]
    // public void Parse_OnelineComment(string comment) => KdlParser.Comment.ParseOrThrow(comment);
    //
    // [Theory]
    // [InlineData("/**/")]
    // [InlineData("/*/**/*/")]
    // [InlineData("/*\n* Hello\n* World!\n*/")]
    // public void Parse_MultilineComment(string comment) => KdlParser.Comment.ParseOrThrow(comment);
    //
    // [Theory]
    // [InlineData("/-commented")]
    // [InlineData("/-node1 { node2 }")]
    // public void Parse_SlashdashComment(string comment) => KdlParser.Comment.ParseOrThrow(comment);
    //
    // [Theory]
    // [InlineData("/-node1 { node2 }")]
    // [InlineData("/* Hello World */")]
    // [InlineData("// Hello World")]
    // [InlineData("// /* Comment in comment */")]
    // public void Parse_Comment(string comment) => KdlParser.Comment.ParseOrThrow(comment);
}