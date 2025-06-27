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

    private static readonly List<char> _whitespaces = new()
    {
        '\u0009', '\u0020',
        '\u00A0', '\u1680',
        '\u2000', '\u2001',
        '\u2002', '\u2003',
        '\u2004', '\u2005',
        '\u2006', '\u2007',
        '\u2008', '\u2009',
        '\u200a', '\u202f',
        '\u205f', '\u3000'
    };

    internal static readonly Parser<char, Unit> Whitespace =
        ToUnit(Token(c => _whitespaces.Contains(c))).Labelled("whitespace");

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

    private static Parser<char, double> UnsignedDigits(ushort b) => Map(
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

    #region String

    private static readonly List<char> NonidentifierCharacters = new()
    {
        '{', '}',
        '(', ')',
        '[', ']',
        '/', '\\',
        '"', '#',
        ';', '='
    };

    private static readonly Parser<char, string> InitialCharacters = Letter.Select(c => c.ToString())
        .Or(Token(c => c is '-' or '+').Then(OneOf(
                    Token(c => c == '.').Then(Token(c => !char.IsDigit(c)).Labelled("not digit"),
                        (f, l) => string.Concat(f, l)),
                    Token(c => !char.IsDigit(c)).Labelled("not digit").Select(c => c.ToString())
                ),
                (f, l) => f + l
            )
        );

    internal static readonly Parser<char, char> Escapes = Char('\\').Then(OneOf(
        Char('\\').ThenReturn('\\'),
        Char('"').ThenReturn('"'),
        Char('n').ThenReturn('\n'),
        Char('r').ThenReturn('\r'),
        Char('t').ThenReturn('\t'),
        Char('f').ThenReturn('\f'),
        Char('b').ThenReturn('\b'),
        Char('s').ThenReturn('\u0020'),
        DigitByBase(16).AtLeastOnce().Between(String("u{"), Char('}'))
            .Map(digits => (char)short.Parse(string.Concat(digits), NumberStyles.HexNumber))
    ));

    internal static readonly Parser<char, string> UnquotedString =
        InitialCharacters.Then(
            Token(c => !NonidentifierCharacters.Contains(c)).Until(OneOf(End, Newline, Whitespace)),
            (f, l) => f + string.Concat(l)
        );

    internal static readonly Parser<char, Unit> EscapeWhitespace = Char('\\').Then(Whitespace.SkipAtLeastOnce());

    internal static readonly Parser<char, string> QuotedString =
        Char('"').Then(OneOf(
            Try(EscapeWhitespace.Select(_ => "")),
            Escapes.Select(c => c.ToString()),
            Any.Select(c => c.ToString())
        ).Until(Char('"'))).Select(string.Concat);

    internal static readonly Parser<char, string> RawString = Char('#').AtLeastOnce().Select(hashes => hashes.Count())
        .Then(count =>
            Char('"').Then(
                Any.Until(Try(String('"' + new string('#', count)))).Select(string.Concat)
            )
        );

    internal static readonly Parser<char, KdlValue> String = OneOf(
        UnquotedString,
        QuotedString,
        RawString
    ).Select(KdlValue (s) => new KdlStringValue(s));

    #endregion

    #region Null

    internal static readonly Parser<char, KdlValue> Null = String("#null").ThenReturn<KdlValue>(new KdlNullValue());

    #endregion
}