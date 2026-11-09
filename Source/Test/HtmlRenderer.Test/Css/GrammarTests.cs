using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class GrammarTests
{
    private const string Green = "rgb(0, 128, 0)";
    private const double Delta = 0.01;

    [TestMethod]
    [Spec("css21", "grammar.html#grammar")]
    public void StyleSheet_IsCharsetImportsThenRuleSetsMediaAndPage()
    {
        var sheet = ParseStyleSheet("@charset \"utf-8\";\n<!-- @import 'a.css' print, screen; -->\np { color: red } <!-- @media print { p { color: red } } --> @page :first { margin: 1cm }");

        CollectionAssert.AreEqual(
            new[] { RuleType.Charset, RuleType.Import, RuleType.Style, RuleType.Media, RuleType.Page },
            sheet.Rules.Select(r => r.Type).ToArray());
        Assert.AreEqual("print, screen", ((ImportRule)sheet.Rules[1]).Media.MediaText);
        Assert.AreEqual(":first", ((PageRule)sheet.Rules[4]).SelectorText);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#grammar")]
    public void MediaList_IsACommaSeparatedListOfMedia()
    {
        Assert.AreEqual(Green, Styled("@media print, screen { #x { color: green } }").Color);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#grammar")]
    [DataRow("div p")]
    [DataRow("div > p")]
    [DataRow("h1 + p")]
    [DataRow("*.a#b")]
    [DataRow("[title]")]
    [DataRow("a[title=x]")]
    [DataRow("a[title = \"x y\"]")]
    [DataRow("a[class~=x]")]
    [DataRow("a[lang|=en]")]
    [DataRow("a:first-child")]
    [DataRow("p:lang(en)")]
    [DataRow("p:first-line")]
    [DataRow("h1, h2 > p, .a .b")]
    public void Selector_FollowsTheGrammar(string selector)
    {
        var sheet = ParseStyleSheet(selector + " { color: green }");

        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Style, sheet.Rules[0].Type);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#grammar")]
    public void Declaration_TakesImportantWithWhitespaceAndComments()
    {
        Assert.AreEqual(Green, Styled("#x { color: green ! /* c */ important } #x { color: red }").Color);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#grammar")]
    [DataRow("-10px", -10)]
    [DataRow("+10px", 10)]
    [DataRow("-.5em", -10)]
    public void Term_TakesAUnaryOperator(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x {{ font-size: 20px; margin-left: {value} }}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#grammar")]
    public void Expression_TakesSlashAndCommaOperators()
    {
        var style = ParseDeclarations("font: italic 12px/30px Georgia, serif");

        Assert.AreEqual("12px", style.FontSize);
        Assert.AreEqual("30px", style.LineHeight);
        Assert.AreEqual("Georgia, serif", style.FontFamily);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    [DataRow("~=", "Match")]
    [DataRow("|=", "Match")]
    [DataRow("<!--", "Cdo")]
    [DataRow("-->", "Cdc")]
    [DataRow("@media", "AtKeyword")]
    [DataRow("#a1", "Hash")]
    [DataRow("-a_b", "Ident")]
    [DataRow("rgb(", "Function")]
    [DataRow("'a'", "String")]
    [DataRow("1.5", "Number")]
    [DataRow(".5", "Number")]
    [DataRow("50%", "Percentage")]
    [DataRow("10px", "Dimension")]
    [DataRow("10foo", "Dimension")]
    [DataRow("url( a.png )", "Url")]
    public void Scanner_ProducesTheToken(string input, string expected)
    {
        var lexer = new Lexer(new TextSource(input));

        Assert.AreEqual(expected, lexer.Get().Type.ToString());
        Assert.AreEqual(TokenType.EndOfFile, lexer.Get().Type);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void SpaceTabLineBreaksAndFormFeed_AreWhitespace()
    {
        var lexer = new Lexer(new TextSource(" \t\r\n\fa"));

        var types = new List<TokenType>();
        for (var token = lexer.Get(); token.Type != TokenType.EndOfFile; token = lexer.Get())
            types.Add(token.Type);

        Assert.AreEqual(TokenType.Ident, types[^1]);
        Assert.IsTrue(types.Take(types.Count - 1).All(t => t == TokenType.Whitespace));
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void UnclosedCommentAtTheEnd_IsIgnored()
    {
        Assert.AreEqual(Green, Styled("#x { color: green } /* #x { color: red }").Color);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    [DataRow(@"@med\ia screen { #x { color: green } }")]
    [DataRow(@"@\6D edia screen { #x { color: green } }")]
    [DataRow(@"#x { color: green !imp\ortant } #x { color: red }")]
    public void EscapedKeywordLetters_AreTheLetters(string css)
    {
        Assert.AreEqual(Green, Styled(css).Color);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#scanner")]
    public void EscapedUnitLetters_AreTheUnit()
    {
        Assert.AreEqual(10, Styled(@"#x { margin-left: 10p\x }").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "grammar.html#scanner")]
    [DataRow("#x { margin-left: 10PX }")]
    [DataRow("#x { margin-left: 10Px }")]
    [DataRow("#x { font-size: 20px; margin-left: 0.5EM }")]
    [DataRow("@MEDIA screen { #x { margin-left: 10px } }")]
    [DataRow("@Media screen { #x { margin-left: 10px } }")]
    [DataRow("#x { margin-left: 10px !IMPORTANT } #x { margin-left: 0 }")]
    public void Scanner_IsCaseInsensitive(string css)
    {
        Assert.AreEqual(10, Styled(css).ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#tokenizer-diffs")]
    public void SixHexDigits_AreOneEscape()
    {
        Assert.AreEqual(1, Before(@"#x::before { content: '\abcdef' }").Length);
        Assert.AreEqual("઼" + "def", Before(@"#x::before { content: '\abc def' }"));
    }

    [TestMethod]
    [Spec("css21", "grammar.html#tokenizer-diffs")]
    public void NumberFollowedByAnIdentifier_IsADimension()
    {
        var box = Styled("#x { font-size: 20px; margin-left: 1em } #x { font: 10pt/1.2serif }");

        Assert.AreEqual(20, box.ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "grammar.html#tokenizer-diffs")]
    public void ClassStartingWithADigit_NeedsAnEscape()
    {
        Assert.AreNotEqual(Green, Styled(".55ft { color: green }", "<div id=x class=55ft>a</div>").Color);
        Assert.AreEqual(Green, Styled(@".\35 5ft { color: green }", "<div id=x class=55ft>a</div>").Color);
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
}
