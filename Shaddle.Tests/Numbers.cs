using Pidgin;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Numbers
{
    [Theory]
    [InlineData("0x0", 0)]
    [InlineData("0x1", 1)]
    [InlineData("0x729C", 29340)]
    [InlineData("0x1BFFC4", 1834948)]
    [InlineData("0x1_76_62", 95842)]
    [InlineData("0xC1__61___0AD8", 3244362456)]
    public void Parse_UnsignedHex(string s, double e)
    {
        var actual = KdlParser.UnsignedHex.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("_0x0")]
    [InlineData("0x_00")]
    public void Parse_InvalidUnsignedHex(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.UnsignedHex.ParseOrThrow(s));
    }

    [Theory]
    [InlineData("0o0", 0)]
    [InlineData("0o1", 1)]
    [InlineData("0o705717", 232399)]
    [InlineData("0o2542265", 705717)]
    [InlineData("0o202457572", 034234234)]
    [InlineData("0o14_3005_02", 3244354)]
    [InlineData("0o1340_32243__7542", 98839445346)]
    public void Parse_UnsignedOctal(string s, double e)
    {
        var actual = KdlParser.UnsignedOctal.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("_0o0")]
    [InlineData("0o_00")]
    public void Parse_InvalidUnsignedOctal(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.UnsignedOctal.ParseOrThrow(s));
    }

    [Theory]
    [InlineData("0b0", 0)]
    [InlineData("0b1", 1)]
    [InlineData("0b0001", 1)]
    [InlineData("0b111010011010", 3738)]
    [InlineData("0b1_00110__110010110010__00100111111", 325423423)]
    public void Parse_UnsignedBinary(string s, double e)
    {
        var actual = KdlParser.UnsignedBinary.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("_0b0")]
    [InlineData("0b_00")]
    public void Parse_InvalidUnsignedBinary(string s)
    {
        Assert.ThrowsAny<ParseException>(() => KdlParser.UnsignedBinary.ParseOrThrow(s));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("0.1", 0.1)]
    [InlineData("0.0001", 0.0001)]
    [InlineData("000.0001", 0.0001)]
    [InlineData("000", 0)]
    [InlineData("123423", 123423)]
    [InlineData("12_34_23.83_94", 123423.8394)]
    [InlineData("1.4_2e+3", 1420)]
    [InlineData("1.42e+3_", 1420)]
    [InlineData("1e+5", 100000)]
    [InlineData("1e5", 100000)]
    [InlineData("1e-2", 0.01)]
    public void Parse_UnsignedDouble(string s, double e)
    {
        var actual = KdlParser.UnsignedDouble.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("#inf", double.PositiveInfinity)]
    [InlineData("#-inf", double.NegativeInfinity)]
    [InlineData("#nan", double.NaN)]
    public void Parse_NumberKeywords(string s, double e)
    {
        var actual = KdlParser.NumberKeywords.ParseOrThrow(s);
        Assert.Equal(e, actual);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("0.0", 0)]
    [InlineData("0o0", 0)]
    [InlineData("0b0001", 1)]
    [InlineData("0x0", 0)]
    [InlineData("1.42e+3", 1420)]
    [InlineData("-#inf", -double.PositiveInfinity)]
    [InlineData("+#-inf", +double.NegativeInfinity)]
    public void Parse_Number(string s, double e)
    {
        var actual = KdlParser.Number.ParseOrThrow(s) as KdlNumberValue;
        Assert.Equal(e, actual?.Value);
    }
}