using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssSyntaxTests
{
    private const string Green = "rgb(0, 128, 0)";
    private const double Delta = 0.01;
    private const string Replacement = "\xFFFD";

    [TestMethod]
    [Spec("css-syntax-3", "#input-preprocessing")]
    public void FormFeed_IsANewline()
    {
        var tokens = Tokens("'a\fb'");

        Assert.AreEqual(TokenType.String, tokens[0].Type);
        Assert.AreEqual("a", tokens[0].Data);
        Assert.AreEqual(TokenType.Whitespace, tokens[1].Type);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#input-preprocessing")]
    public void Null_IsTheReplacementCharacter()
    {
        var tokens = Tokens("'a\0b' c\0d");

        Assert.AreEqual("a" + Replacement + "b", tokens[0].Data);
        Assert.AreEqual("c" + Replacement + "d", tokens[2].Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#tokenizer-definitions")]
    [DataRow("a\x00D7b")]
    [DataRow("a\x00A0b")]
    [DataRow("a\x2028b")]
    public void NonAsciiCodePointOutsideTheIdentRanges_EndsTheIdent(string css)
    {
        var tokens = Tokens(css);

        Assert.AreEqual("a", tokens[0].Data);
        Assert.AreEqual(TokenType.Delim, tokens[1].Type);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#tokenizer-definitions")]
    [DataRow("a\x00B7b")]
    [DataRow("\x00E9t\x00E9")]
    [DataRow("\x4E2D\x6587")]
    [DataRow("_a-1")]
    [Spec("css-syntax-3", "#consume-name")]
    public void IdentCodePoints_FormOneIdent(string css)
    {
        var tokens = Tokens(css);

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(TokenType.Ident, tokens[0].Type);
        Assert.AreEqual(css, tokens[0].Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-token")]
    [DataRow("#a", "Hash", "a")]
    [DataRow(@"#\31 a", "Hash", "1a")]
    [DataRow("#", "Delim", "#")]
    [DataRow("+.5", "Number", "+.5")]
    [DataRow("-.5", "Number", "-.5")]
    [DataRow(".5", "Number", ".5")]
    [DataRow("-->", "Cdc", "-->")]
    [DataRow("<!--", "Cdo", "<!--")]
    [DataRow("@media", "AtKeyword", "media")]
    [DataRow("@-x", "AtKeyword", "-x")]
    [DataRow("@", "Delim", "@")]
    [DataRow("+", "Delim", "+")]
    [DataRow(".", "Delim", ".")]
    [DataRow(",", "Comma", ",")]
    [DataRow(":", "Colon", ":")]
    [DataRow(";", "Semicolon", ";")]
    [DataRow("[", "SquareBracketOpen", "[")]
    [DataRow("]", "SquareBracketClose", "]")]
    [DataRow("{", "CurlyBracketOpen", "{")]
    [DataRow("}", "CurlyBracketClose", "}")]
    [DataRow("(", "RoundBracketOpen", "(")]
    [DataRow(")", "RoundBracketClose", ")")]
    public void Token_IsConsumedByItsFirstCodePoint(string css, string type, string data)
    {
        var tokens = Tokens(css);

        Assert.AreEqual(type, tokens[0].Type.ToString());
        Assert.AreEqual(data, tokens[0].Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-token")]
    public void BackslashBeforeANewline_IsADelim()
    {
        var tokens = Tokens("\\\na");

        Assert.AreEqual(TokenType.Delim, tokens[0].Type);
        Assert.AreEqual("\\", tokens[0].Data);
        Assert.AreEqual(TokenType.Whitespace, tokens[1].Type);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-token")]
    public void AtFollowedByTwoHyphens_IsAnAtKeyword()
    {
        Assert.AreEqual(TokenType.AtKeyword, Tokens("@--x")[0].Type);
        Assert.AreEqual(Green, Styled("@--x; #x { color: green }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-token")]
    public void UPlus_IsNoUnicodeRangeOutsideTheUnicodeRangeDescriptor()
    {
        Assert.AreEqual(Green, Styled("u+a { color: green }", "<u>u</u><a id=x>a</a>").Color);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-comment")]
    public void Comments_ProduceNoTokens()
    {
        var tokens = Tokens("/* a */b/**/c/* d");

        Assert.AreEqual("b", tokens.Single(t => t.Type == TokenType.Ident && t.Data == "b").Data);
        Assert.AreEqual("c", tokens.Last(t => t.Type == TokenType.Ident).Data);
        Assert.AreEqual(Green, Styled("#x/* a */{/**/color/**/:/**/green/* b").Color);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-numeric-token")]
    [Spec("css-syntax-3", "#consume-number")]
    [DataRow("10", "Number", "10", null)]
    [DataRow("+10", "Number", "+10", null)]
    [DataRow("-1.5", "Number", "-1.5", null)]
    [DataRow("10%", "Percentage", "10", "%")]
    [DataRow("10px", "Dimension", "10", "px")]
    [DataRow("1.5em", "Dimension", "1.5", "em")]
    [DataRow("1e", "Dimension", "1", "e")]
    [DataRow(@"10\70x", "Dimension", "10", "px")]
    public void NumericToken_IsANumberPercentageOrDimension(string css, string type, string number, string? unit)
    {
        var token = Tokens(css)[0];

        Assert.AreEqual(type, token.Type.ToString());
        Assert.AreEqual(number, token.Data);
        if (unit != null) Assert.AreEqual(unit, ((UnitToken)token).Unit);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-numeric-token")]
    [DataRow("10x2", "x2")]
    [DataRow("10--x", "--x")]
    [DataRow("10a_b", "a_b")]
    [DataRow("1e-", "e-")]
    public void DimensionUnit_IsAnIdentSequence(string css, string unit)
    {
        var tokens = Tokens(css);

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(TokenType.Dimension, tokens[0].Type);
        Assert.AreEqual(unit, ((UnitToken)tokens[0]).Unit);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-number")]
    [DataRow("1e3", "1e3")]
    [DataRow("1E-3", "1E-3")]
    [DataRow(".5e+2", ".5e+2")]
    public void Exponent_IsPartOfTheNumber(string css, string number)
    {
        var tokens = Tokens(css);

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(TokenType.Number, tokens[0].Type);
        Assert.AreEqual(number, tokens[0].Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-number")]
    public void NumberWithExponent_CanHaveAUnit()
    {
        Assert.AreEqual(100, Styled("#x{width:1e2px}").ActualWidth, Delta);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-ident-like-token")]
    [DataRow("foo", "Ident", "foo")]
    [DataRow("rgb(", "Function", "rgb")]
    [DataRow("url(a.png)", "Url", "a.png")]
    [DataRow("URL(a.png)", "Url", "a.png")]
    [DataRow(@"u\72l(a.png)", "Url", "a.png")]
    public void IdentLikeToken_IsAnIdentFunctionOrUrl(string css, string type, string data)
    {
        var token = Tokens(css)[0];

        Assert.AreEqual(type, token.Type.ToString());
        Assert.AreEqual(data, token.Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-ident-like-token")]
    public void UrlWithAQuotedArgument_IsAFunctionToken()
    {
        var token = Tokens("url(  'a.png'  )")[0];

        Assert.AreEqual(TokenType.Function, token.Type);
        Assert.AreEqual("url", token.Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-ident-like-token")]
    public void UrlWithALineBreakInTheQuotedArgument_IsAFunctionWithABadString()
    {
        var tokens = Tokens("url(\"a\nb\") c");

        Assert.AreEqual(TokenType.Function, tokens[0].Type);
        Assert.AreEqual(TokenType.String, tokens[1].Type);
        Assert.IsTrue(((StringToken)tokens[1]).IsValid, "StringToken.IsValid holds whether the string is bad");
        Assert.AreEqual(TokenType.Whitespace, tokens[2].Type);
        Assert.AreEqual("b", tokens[3].Data);
        Assert.AreEqual(TokenType.String, tokens[4].Type);
        Assert.AreEqual(") c", tokens[4].Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void StringAtTheEndOfInput_IsClosed()
    {
        var token = Tokens("'abc")[0];

        Assert.AreEqual(TokenType.String, token.Type);
        Assert.AreEqual("abc", token.Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void BackslashAtTheEndOfInput_IsDroppedFromTheString()
    {
        var token = (StringToken)Tokens("'abc\\")[0];

        Assert.AreEqual("abc", token.Data);
        Assert.IsFalse(token.IsValid, "StringToken.IsValid holds whether the string is bad");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void BadString_InvalidatesTheDeclaration()
    {
        Assert.AreEqual("a", Before("#x::before{content:'a'} #x::before{content:'b\n}"));
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-url-token")]
    [DataRow("url(a.png", "a.png")]
    [DataRow(@"url(a\).png)", "a).png")]
    [DataRow("url(\n a.png \n)", "a.png")]
    public void UnquotedUrl_IsAUrlToken(string css, string url)
    {
        var token = Tokens(css)[0];

        Assert.AreEqual(TokenType.Url, token.Type);
        Assert.AreEqual(url, token.Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-url-token")]
    [DataRow("url(a b.png)")]
    [DataRow("url(a(b.png)")]
    [DataRow("url(a\u0001.png)")]
    public void BadUrl_InvalidatesTheDeclaration(string url)
    {
        Assert.AreEqual("none", Styled($"#x{{background-image:{url}}}").BackgroundImage);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-escaped-code-point")]
    [DataRow(@"#x::before{content:'\26 B'}", "&B")]
    [DataRow(@"#x::before{content:'\000026B'}", "&B")]
    [DataRow(@"#x::before{content:'\1F600'}", "\xD83D\xDE00")]
    [DataRow(@"#x::before{content:'\110000'}", Replacement)]
    [DataRow(@"#x::before{content:'\0'}", Replacement)]
    [DataRow(@"#x::before{content:'\g'}", "g")]
    public void EscapedCodePoint_IsDecoded(string css, string expected)
    {
        Assert.AreEqual(expected, Before(css));
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-escaped-code-point")]
    public void EscapedSurrogate_IsTheReplacementCharacter()
    {
        Assert.AreEqual(Replacement + Replacement + Replacement, Tokens(@"'\D800\DABC\DFFF'")[0].Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-escaped-code-point")]
    [Spec("css-syntax-3", "#starts-with-a-valid-escape")]
    public void BackslashAtTheEndOfInput_IsTheReplacementCharacterInAnIdent()
    {
        var tokens = Tokens("a\\");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual("a" + Replacement, tokens[0].Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#starts-with-a-valid-escape")]
    [Spec("css-syntax-3", "#would-start-an-identifier")]
    [DataRow("-a", "Ident", "-a")]
    [DataRow("--", "Ident", "--")]
    [DataRow("--a", "Ident", "--a")]
    [DataRow(@"-\31 ", "Ident", "-1")]
    [DataRow(@"\31 a", "Ident", "1a")]
    [DataRow("-1", "Number", "-1")]
    [DataRow("- a", "Delim", "-")]
    public void IdentStart_IsALetterHyphenOrEscape(string css, string type, string data)
    {
        var token = Tokens(css)[0];

        Assert.AreEqual(type, token.Type.ToString());
        Assert.AreEqual(data, token.Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#starts-with-a-number")]
    [DataRow("+1", "Number")]
    [DataRow("-1", "Number")]
    [DataRow("+.1", "Number")]
    [DataRow("-.1", "Number")]
    [DataRow(".1", "Number")]
    [DataRow("+.a", "Delim")]
    [DataRow(".a", "Delim")]
    public void NumberStart_IsADigitAfterAnOptionalSignAndDot(string css, string type)
    {
        Assert.AreEqual(type, Tokens(css)[0].Type.ToString());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-remnants-of-bad-url")]
    public void BadUrl_RunsOverASemicolon()
    {
        Assert.AreEqual(Green, Styled("#x { color: green; background-image: url(a\"b; color: red; x) }").Color);
        Assert.AreEqual("d", Tokens("url(a\"b; c) d").Last().Data);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-remnants-of-bad-url")]
    public void BadUrl_EndsAtTheFirstClosingParenthesisEvenAfterAnOpeningOne()
    {
        Assert.AreEqual("c", Tokens("url(a\"(b) c").Last().Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-remnants-of-bad-url")]
    public void BadUrl_SkipsAnEscapedClosingParenthesis()
    {
        var tokens = Tokens(@"url(a""b\) c) d");

        Assert.AreEqual(3, tokens.Count);
        Assert.AreEqual(TokenType.Url, tokens[0].Type);
        Assert.AreEqual("d", tokens[2].Data);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#parse-block-contents")]
    public void StyleAttribute_IsParsedAsABlocksContents()
    {
        var box = Styled("", "<div id=x style='; color: green ;; background-color: green; '>a</div>");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#parse-declaration")]
    public void SupportsCondition_ParsesADeclaration()
    {
        var supports = (SupportsRule)ParseStyleSheet("@supports ( color : green ) { }").Rules[0];

        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-at-rule")]
    public void TopLevelCloseBraceInAnAtRulePrelude_IsPartOfThePrelude()
    {
        Assert.AreEqual(Green, Styled("#x { color: green } @foo } #x { color: red }").Color);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-at-rule")]
    public void AtRule_EndsAtASemicolonOrAfterItsBlock()
    {
        var box = Styled("@foo a ( ; ) [ ; ] ; #x { color: green } @foo { ; } #x { background-color: green }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-at-rule")]
    public void BraceInsideABracketBlockOfAnAtRulePrelude_DoesNotStartTheAtRuleBlock()
    {
        // The [ block is never closed, it takes the rest of the style sheet into the prelude.
        Assert.AreEqual(Green, Styled("#x { color: green } @foo [ { ] ; } #x { color: red }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-qualified-rule")]
    public void TopLevelCloseBraceInAQualifiedRulePrelude_IsPartOfThePrelude()
    {
        Assert.AreEqual(Green, Styled("#x { color: green } } #x { color: red }").Color);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-qualified-rule")]
    public void QualifiedRule_WithoutABlock_IsDropped()
    {
        Assert.AreEqual(Green, Styled("#x { color: green } #x").Color);
        Assert.AreEqual(Green, Styled("#x { color: green } --x: { color: red } #x { background-color: green }").BackgroundColor);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-block")]
    [Spec("css-syntax-3", "#consume-block-contents")]
    public void SemicolonsAndWhitespaceInABlock_AreDiscarded()
    {
        Assert.AreEqual(Green, Styled("#x { ; ; color: green ; ; }").Color);
        Assert.AreEqual(Green, Styled("@media screen { ; #x { color: green } ; }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-block-contents")]
    [Spec("css-syntax-3", "#style-rules")]
    public void UnknownAtRuleInADeclarationBlock_IsDropped()
    {
        var box = Styled("#x { color: green; @foo { color: red } background-color: green; @bar; border-top-style: solid }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
        Assert.AreEqual("solid", box.BorderTopStyle);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-declaration")]
    public void Declaration_IsANameColonAndValue()
    {
        var box = Styled("#x { color : green ! important ; color: red; background-color: red; background-color green; width: 10px }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual("rgb(255, 0, 0)", box.BackgroundColor);
        Assert.AreEqual("10px", box.Width);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-declaration")]
    public void Important_IsCaseInsensitive()
    {
        Assert.AreEqual(Green, Styled("#x { color: green !IMPORTANT } #x { color: red }").Color);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#consume-list-of-components")]
    [Spec("css-syntax-3", "#consume-component-value")]
    [Spec("css-syntax-3", "#consume-function")]
    public void FunctionAtTheEndOfInput_IsClosed()
    {
        Assert.AreEqual(15, Styled("#x { width: calc(10px + 5px").ActualWidth, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#consume-simple-block")]
    [DataRow("(")]
    [DataRow("[")]
    public void SemicolonInASimpleBlock_DoesntEndTheDeclaration(string open)
    {
        var close = open == "(" ? ")" : "]";

        Assert.AreEqual(Green, Styled($"#x {{ color: green; foo: {open} ; color: red; {close} }}").Color);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#anb-microsyntax")]
    [Spec("css-syntax-3", "#the-anb-type")]
    [DataRow("odd", "1,3,5")]
    [DataRow("EVEN", "2,4,6")]
    [DataRow("3", "3")]
    [DataRow("+3", "3")]
    [DataRow("2n", "2,4,6")]
    [DataRow("2N+1", "1,3,5")]
    [DataRow("n", "1,2,3,4,5,6")]
    [DataRow("+n", "1,2,3,4,5,6")]
    [DataRow("-n+3", "1,2,3")]
    [DataRow("2n-1", "1,3,5")]
    [DataRow("2n-3", "1,3,5")]
    [DataRow("3n + 1", "1,4")]
    [DataRow(" +3n - 2 ", "1,4")]
    [DataRow("-n+ 6", "1,2,3,4,5,6")]
    [DataRow("2n- 1", "1,3,5")]
    [DataRow("0n+0", "")]
    [DataRow("-4n+10", "2,6")]
    [DataRow("3 n", "")]
    [DataRow("+ 2n", "")]
    [DataRow("+ 2", "")]
    [DataRow("3n + -6", "")]
    [DataRow("2.0n", "")]
    public void AnPlusB_SelectsTheElementsAtTheIndices(string anb, string expected)
    {
        Assert.AreEqual(expected, Matching($"li:nth-child({anb})"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-syntax-3.yml")]
    [Spec("css-syntax-3", "#the-anb-type")]
    [DataRow("n-1", "1,2,3,4,5,6")]
    [DataRow("+n-1", "1,2,3,4,5,6")]
    [DataRow("n- 1", "1,2,3,4,5,6")]
    [DataRow("N-2", "1,2,3,4,5,6")]
    public void AnPlusB_TakesBInTheIdentOrUnit(string anb, string expected)
    {
        Assert.AreEqual(expected, Matching($"li:nth-child({anb})"));
    }

    [TestMethod]
    [Spec("css-syntax-3", "#charset-rule")]
    public void CharsetRule_HasNoEffect()
    {
        Assert.AreEqual(Green, Styled("@charset \"utf-8\"; #x { color: green }").Color);
        Assert.AreEqual(Green, Styled("#x { color: green } @charset \"utf-8\"; #x { background-color: green }").BackgroundColor);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#style-rules")]
    public void PropertyNames_AreCaseInsensitiveButCustomPropertyNamesArent()
    {
        var box = Styled("#x { --Foo: red; --foo: green; COLOR: var(--foo); Background-Color: var(--Foo) }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual("rgb(255, 0, 0)", box.BackgroundColor);
    }

    private static List<Token> Tokens(string css)
    {
        var lexer = new Lexer(new TextSource(css));
        var tokens = new List<Token>();
        for (var token = lexer.Get(); token.Type != TokenType.EndOfFile; token = lexer.Get())
        {
            tokens.Add(token);
        }

        return tokens;
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string Before(string css)
    {
        var before = Styled(css).Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, "No ::before box");
        return before.Text ?? string.Concat(LayoutHarness.Descendants(before).SelectMany(b => b.Words).Select(w => w.Text));
    }

    /// <summary>The 1-based indices of the six list items that <paramref name="selector"/> turns green.</summary>
    private static string Matching(string selector)
    {
        var items = string.Concat(Enumerable.Range(1, 6).Select(i => $"<li id=i{i}>{i}</li>"));
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{selector} {{ color: green }}</style></head><body><ul id=x>{items}</ul></body></html>");

        return string.Join(",", Enumerable.Range(1, 6).Where(i => LayoutHarness.FindById(root, "i" + i)!.Color == Green));
    }
}
