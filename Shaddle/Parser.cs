using System.Diagnostics.CodeAnalysis;
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

    private static readonly List<string> _newlines = new()
    {
        "\r\n", "\r",
        "\n", "\u0085",
        "\v", "\f",
        "\u2028", "\u2029",
    };

    internal static readonly Parser<char, Unit> Whitespace =
        ToUnit(Token(c => _whitespaces.Contains(c))).Labelled("whitespace");

    internal static readonly Parser<char, Unit> Newline = ToUnit(OneOf(
        Try(String("\r\n")),
        String("\r"),
        String("\n"),
        String("\u0085"),
        String("\v"),
        String("\f"),
        String("\u2028"),
        String("\u2029")
    )).Labelled("newline");

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

    internal static readonly Parser<char, KdlValue> Number = Map(
        KdlValue (sign, num) => new KdlNumberValue(sign.HasValue ? num * sign.Value : num),
        Sign.Optional(),
        OneOf(
            Try(UnsignedHex),
            Try(UnsignedOctal),
            Try(UnsignedBinary),
            UnsignedDouble
        )
    );

    #endregion

    #region String

    private static readonly List<char> NonidentifierCharacters = new()
    {
        '\n', '=',
        '{', '}',
        '(', ')',
        '[', ']',
        '/', '\\',
        '"', '#',
        ';'
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
            Token(c =>
                !NonidentifierCharacters.Contains(c)
                && !_whitespaces.Contains(c)
                && !_newlines.Contains(c.ToString())).ManyString(),
            (f, l) => f + l
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

    internal static readonly Parser<char, string> String = OneOf(
        UnquotedString,
        QuotedString,
        RawString
    );

    private static readonly Parser<char, KdlValue> TypedString =
        UnquotedString.Between(Char('('), Char(')')).Optional()
            .Then<string, KdlValue>(String,
                (type, str) => type.HasValue
                    ? new KdlValue<string>(str, type.Value)
                    : new KdlStringValue(str));

    #endregion

    #region Keywords

    internal static readonly Parser<char, KdlValue> Keywords = Char('#').Then(OneOf(
        Try(String("null")).ThenReturn<KdlValue>(new KdlNullValue()),
        String("true").ThenReturn<KdlValue>(new KdlBooleanValue(true)),
        String("false").ThenReturn<KdlValue>(new KdlBooleanValue(false)),
        String("-inf").ThenReturn<KdlValue>(new KdlNumberValue(double.NegativeInfinity)),
        String("inf").ThenReturn<KdlValue>(new KdlNumberValue(double.PositiveInfinity)),
        String("nan").ThenReturn<KdlValue>(new KdlNumberValue(double.NaN))
    ));

    #endregion

    #region Nodes

    internal static readonly Parser<char, KdlValue> Value = OneOf(
        Try(Keywords),
        Number,
        TypedString
    );

    internal static readonly Parser<char, KeyValuePair<string?, KdlValue>> NodeEntry = OneOf(
        Try(String.Then(
            Char('=').Then(Value),
            (key, val) => new KeyValuePair<string?, KdlValue>(key, val)
        )).Labelled("node property"),
        Value.Select(val => new KeyValuePair<string?, KdlValue>(null, val)).Labelled("node argument")
    );

    private static KdlNode MapKdlNode(Maybe<string> type,
        string name,
        IEnumerable<KeyValuePair<string?, KdlValue>> entries,
        Maybe<KdlDocument> body)
    {
        var properties = new Dictionary<string, KdlValue>();
        var arguments = new List<KdlValue>();

        foreach (var entry in entries)
        {
            if (entry.Key is null)
                arguments.Add(entry.Value);
            else
                properties.Add(entry.Key, entry.Value);
        }

        return new KdlNode(name, type.HasValue ? type.Value : null)
        {
            Arguments = arguments,
            Properties = properties,
            Children = body.HasValue ? body.Value : null
        };
    }

    internal static readonly Parser<char, Unit> Comment = Char('/').Then(OneOf(
        Char('/').Then(ToUnit(Any.ManyThen(Newline))),
        Char('*').Then(ToUnit(Any.ManyThen(String("*/"))))
    )).Labelled("comment");

    private static readonly Parser<char, Unit> LineSpace = Comment.Or(Whitespace.Or(Newline));

    private static readonly Parser<char, Unit> NodeSpace = Char('\\').Then(LineSpace.SkipAtLeastOnce())
        .Or(ToUnit(String("/-").Then(NodeEntry))).Or(Whitespace);

    private static readonly Parser<char, Unit> DocumentSpace =
        Try(ToUnit(String("/-").Then(Rec(() => Node!)))).Or(LineSpace);

    private static readonly Parser<char, Unit> NodeTerminator =
        ToUnit(Char(';'))
            .Or(Newline)
            .Or(Whitespace.SkipMany().Then(Comment))
            .Or(End);

    private static readonly Parser<char, KdlDocument> NodeBody =
        Char('{').Then(LineSpace.SkipMany().Then(Rec(() => Document!))).Before(Char('}'));

    private static readonly Parser<char, Unit> ChildrenSpace = ToUnit(String("/-").Then(NodeBody)).Or(Whitespace);

    internal static readonly Parser<char, KdlNode> Node = Map(
        MapKdlNode,
        UnquotedString.Between(Char('('), Char(')')).Optional(),
        String,
        Try(NodeSpace.SkipMany().Then(NodeEntry)).Many(),
        Try(NodeSpace).SkipMany().Then(NodeBody.Optional())
    ).Before(Try(ChildrenSpace).Many().Then(NodeTerminator));

    internal static readonly Parser<char, KdlDocument> Document = Map(
        nodes => new KdlDocument(nodes.ToList()),
        DocumentSpace.SkipMany().Then(Node).Before(DocumentSpace.Many()).Many()
    );

    #endregion

    /// <summary>
    /// Parses the input stream into a <see cref="KdlDocument"/>.
    /// </summary>
    /// <param name="reader">An input stream.</param>
    /// <returns>Kdl document.</returns>
    /// <exception cref="ParseException">In case of an error during parsing, it throws an exception.</exception>
    public static KdlDocument Parse(TextReader reader) => Document.ParseOrThrow(reader);

    /// <summary>
    /// Tries to parse the input stream into a kdl document.
    /// If parsing is successful, it returns <c>true</c> and <see cref="KdlDocument"/>.
    /// In case of an error, it returns <c>false</c> and an error <see cref="ParseError{TToken}"/>.
    /// </summary>
    /// <param name="reader">An input stream.</param>
    /// <param name="document">Kdl document.</param>
    /// <param name="parseError">Parse error.</param>
    /// <returns><c>true</c> or <c>false</c> depending on the success of the parsing.</returns>
    public static bool TryParse(TextReader reader,
        [NotNullWhen(true)] out KdlDocument? document,
        [NotNullWhen(false)] out ParseError<char>? parseError)
    {
        var result = Document.Parse(reader);
        if (result.Success)
        {
            document = result.Value;
            parseError = null;
            return true;
        }

        document = null;
        parseError = result.Error!;
        return false;
    }

    /// <summary>
    /// The TryParse option, which is useful if you don't need error details.
    /// </summary>
    /// <param name="reader">An input stream.</param>
    /// <param name="document">Kdl document.</param>
    /// <returns><c>true</c> or <c>false</c> depending on the success of the parsing.</returns>
    public static bool TryParse(TextReader reader, [NotNullWhen(true)] out KdlDocument? document)
    {
        if (TryParse(reader, out var doc, out var _))
        {
            document = doc;
            return true;
        }

        document = null;
        return false;
    }

    /// <summary>
    /// Parses the input string into a <see cref="KdlDocument"/>.
    /// </summary>
    /// <param name="input">An input string.</param>
    /// <returns>Kdl document.</returns>
    /// <exception cref="ParseException">In case of an error during parsing, it throws an exception.</exception>
    public static KdlDocument Parse(string input)
    {
        using var reader = new StringReader(input);
        return Parse(reader);
    }

    /// <summary>
    /// Tries to parse the input string into a kdl document.
    /// If parsing is successful, it returns <c>true</c> and <see cref="KdlDocument"/>.
    /// In case of an error, it returns <c>false</c> and an error <see cref="ParseError{TToken}"/>.
    /// </summary>
    /// <param name="input">An input string.</param>
    /// <param name="document">Kdl document.</param>
    /// <param name="parseError">Parse error.</param>
    /// <returns><c>true</c> or <c>false</c> depending on the success of the parsing.</returns>
    public static bool TryParse(string input,
        [NotNullWhen(true)] out KdlDocument? document,
        [NotNullWhen(false)] out ParseError<char>? parseError)
    {
        using var reader = new StringReader(input);
        if (TryParse(reader, out var doc, out var err))
        {
            document = doc;
            parseError = null;
            return true;
        }

        document = null;
        parseError = err;
        return false;
    }

    /// <summary>
    /// The TryParse option, which is useful if you don't need error details.
    /// </summary>
    /// <param name="input">An input stream.</param>
    /// <param name="document">Kdl document.</param>
    /// <returns><c>true</c> or <c>false</c> depending on the success of the parsing.</returns>
    public static bool TryParse(string input, [NotNullWhen(true)] out KdlDocument? document)
    {
        using var reader = new StringReader(input);
        if (TryParse(reader, out var doc, out var _))
        {
            document = doc;
            return true;
        }

        document = null;
        return false;
    }
}