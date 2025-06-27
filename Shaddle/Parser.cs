using System.Globalization;
using System.Runtime.CompilerServices;
using Pidgin;
using Shaddle.Values;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

[assembly: InternalsVisibleTo("Shaddle.Tests")]

namespace Shaddle;

public class KdlParser
{
    #region Spaces

    private static Parser<char, Unit> ToUnit<T>(Parser<char, T> parser) => parser.Select(_ => Unit.Value);

    internal static readonly Parser<char, Unit> Whitespace = ToUnit(OneOf(
        Char('\u0009'),
        Char('\u0020'),
        Char('\u00A0'),
        Char('\u1680'),
        Char('\u2000'),
        Char('\u2001'),
        Char('\u2002'),
        Char('\u2003'),
        Char('\u2004'),
        Char('\u2005'),
        Char('\u2006'),
        Char('\u2007'),
        Char('\u2008'),
        Char('\u2009'),
        Char('\u200a'),
        Char('\u202f'),
        Char('\u205f'),
        Char('\u3000')
    ));

    internal static readonly Parser<char, Unit> Newline = ToUnit(OneOf(
        Try(String("\u000D\u000A")),
        String("\u000D"),
        String("\u000A"),
        String("\u0085"),
        String("\u000B"),
        String("\u000C"),
        String("\u2028"),
        String("\u2029")
    ));

    #endregion

    #region Numbers

    private static readonly Parser<char, double> Sign = Char('-').ThenReturn(-1d).Or(Char('+').ThenReturn(+1d));

    private static Parser<char, char> DigitByBase(uint b) => Token(c => b <= 10
        ? c >= '0' && c < '0' + b
        : (c >= '0' && c <= '9') || (c >= 'A' && c < 'A' + b - 10) || (c >= 'a' && c < 'a' + b - 10));

    internal static Parser<char, double> UnsignedDigits(ushort b) => Map(
        (f, l) => (double)Convert.ToInt64((f + l).Replace("_", ""), b),
        DigitByBase(b),
        Char('_').Or(DigitByBase(b)).ManyString()
    );

    internal static readonly Parser<char, double> UnsignedHex = String("0x").Then(UnsignedDigits(16));

    internal static readonly Parser<char, double> UnsignedOctal = String("0o").Then(UnsignedDigits(8));

    internal static readonly Parser<char, double> UnsignedBinary = String("0b").Then(UnsignedDigits(2));

    private static readonly Parser<char, string> Exponent = Char('e').Or(Char('E')).Then(
        Sign.Optional().Then(DigitByBase(10).Or(Char('_')).ManyString(), (sign, digits) => sign.HasValue
            ? (sign.Value == -1
                ? "-" + digits
                : "+" + digits)
            : digits),
        ((e, digits) => e + digits.Replace("_", ""))
    );

    internal static readonly Parser<char, double> UnsignedDouble = DigitByBase(10)
        .Then(OneOf(DigitByBase(10), Char('_')).ManyString(), (f, l) => f + l)
        .Then(Char('.')
                .Then(DigitByBase(10).Then(OneOf(DigitByBase(10), Char('_')).ManyString(), (f, l) => f + l))
                .Select(digits => "." + string.Concat(digits))
                .Optional()
                .Then(Exponent.Optional(),
                    (dot, ex) => dot.HasValue
                        ? dot.Value + (ex.HasValue
                            ? ex.Value
                            : "")
                        : ex.HasValue
                            ? ex.Value
                            : ""),
            (digits, dot) =>
                double.Parse(
                    digits.Replace("_", "") + dot.Replace("_", ""),
                    CultureInfo.InvariantCulture
                )
        );

    internal static readonly Parser<char, double> NumberKeywords = Char('#').Then(OneOf(
        String("-inf").ThenReturn(double.NegativeInfinity),
        String("inf").ThenReturn(double.PositiveInfinity),
        String("nan").ThenReturn(double.NaN)
    ));

    internal static readonly Parser<char, KdlValue> Number = Map(
        KdlValue (sign, num) => new KdlNumberValue(sign.HasValue ? num * sign.Value : num),
        Sign.Optional(),
        OneOf(
            Try(UnsignedHex),
            Try(UnsignedOctal),
            Try(UnsignedBinary),
            UnsignedDouble,
            NumberKeywords
        )
    );

    #endregion
    
    #region Boolean

    internal static readonly Parser<char, KdlValue> Boolean = Char('#').Then(OneOf(
        String("true").ThenReturn(true),
        String("false").ThenReturn(false)
    )).Select(KdlValue (val) => new KdlBooleanValue(val));

    #endregion
}