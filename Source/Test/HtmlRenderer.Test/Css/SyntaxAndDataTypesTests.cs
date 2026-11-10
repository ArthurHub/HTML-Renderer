using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class SyntaxAndDataTypesTests
{
    private const string Green = "rgb(0, 128, 0)";
    private const string Red = "rgb(255, 0, 0)";
    private const double Delta = 0.01;

    [TestMethod]
    [Spec("css21", "syndata.html#tokenization")]
    public void LongestMatch_TokenizesRedCdcAsIdentAndDelim()
    {
        var lexer = new Lexer(new TextSource("red-->"));

        var ident = lexer.Get();
        var delim = lexer.Get();

        Assert.AreEqual(TokenType.Ident, ident.Type);
        Assert.AreEqual("red--", ident.Data);
        Assert.AreEqual(TokenType.GreaterThan, delim.Type);
        Assert.AreEqual(TokenType.EndOfFile, lexer.Get().Type);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#tokenization")]
    public void TabsLineBreaksAndFormFeeds_AreWhitespace()
    {
        Assert.AreEqual(Green, Styled("#x{color:\t\f\r\ngreen\f}").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#tokenization")]
    [DataRow(" ")]
    [DataRow("　")]
    public void OtherSpaceCharacters_AreNotWhitespace(string space)
    {
        Assert.AreEqual(Green, Styled($"#x{{color:green}} #x{{color:{space}red}}").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#tokenization")]
    public void Comments_MayAppearBetweenAnyTokens()
    {
        Assert.AreEqual(Green, Styled("/*a*/#x/*b*/{/*c*/color/*d*/:/*e*/green/*f*/;/*g*/}/*h*/").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#keywords")]
    public void QuotedKeyword_IsAStringAndTheDeclarationIsIgnored()
    {
        var box = Styled("#x{color:green;width:100px} #x{color:\"red\";width:\"auto\"}");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual("100px", box.Width);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#vendor-keywords")]
    [DataRow("-xyz-border-east-color: red")]
    [DataRow("_xyz-border-east-color: red")]
    [DataRow("background-color: -xyz-red")]
    public void VendorExtension_IsParsedAndIgnored(string declaration)
    {
        Assert.AreEqual(Green, Styled($"#x{{background-color:green; {declaration}; color:green}}").BackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    [Spec("css-values-4", "#keywords")]
    [Spec("css-values-4", "#functional-notations")]
    public void PropertyNamesKeywordsAndFunctions_AreCaseInsensitive()
    {
        var box = Styled("DIV#x { COLOR: GREEN; Background-Color: RGB(0, 128, 0) } #x { Background-Image: URL(a.png) }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
        Assert.AreEqual("url(\"a.png\")", box.BackgroundImage);
        Assert.AreEqual(Green, Styled("#p{color:green} #x{color:INHERIT}", "<div id=p><div id=x>a</div></div>").Color);
        Assert.AreEqual("III", Before("#x{counter-reset:c 3} #x::before{content:COUNTER(c, UPPER-ROMAN)}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#characters")]
    [Spec("css-values-4", "#dimensions")]
    public void UnitsAtKeywordsAndImportant_AreCaseInsensitive()
    {
        Assert.AreEqual(10, Styled("#x{width:10PX}").ActualWidth, Delta);
        Assert.AreEqual(Green, Styled("@MEDIA screen { #x { color: green } }").Color);
        Assert.AreEqual(Green, Styled("#x { color: green !IMPORTANT } #x { color: red }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#characters")]
    public void PseudoClassAndPseudoElementNames_AreCaseInsensitive()
    {
        Assert.AreEqual(Green, Styled("#x:FIRST-CHILD { color: green }").Color);
        Assert.AreEqual("a", Before("#x::BEFORE { content: 'a' }"));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    [DataRow(@"#x{c\olor:green}")]
    [DataRow(@"#x{colo\72:green}")]
    [DataRow(@"#x{color:gr\65 en}")]
    [DataRow(@"#x{color:gr\000065en}")]
    [DataRow(@"#x{color:gr\000065 en}")]
    [DataRow("#x{color:gr\\65\r\nen}")]
    [Spec("css-syntax-3", "#consume-escaped-code-point")]
    public void EscapedIdentifier_IsTheSameIdentifier(string css)
    {
        Assert.AreEqual(Green, Styled(css).Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    public void EscapedDigit_CanStartAClassName()
    {
        Assert.AreEqual(Green, Styled(@".\32 a{color:green}", "<div id=x class=2a>a</div>").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    public void EscapedPunctuation_IsPartOfTheIdentifier()
    {
        Assert.AreEqual(Green, Styled(@".a\7B b, .c\{d {color:green}", "<div id=x class='a{b'>a</div>").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    public void IdentifierStartingWithADigit_IsInvalid()
    {
        Assert.AreEqual(Green, Styled(".2a{color:red} #x{color:green}", "<div id=x class=2a>a</div>").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    [Spec("css-syntax-3", "#consume-escaped-code-point")]
    public void OnlyOneWhitespaceAfterAHexEscape_IsConsumed()
    {
        Assert.AreEqual("&B & B", Before(@"#x::before{content:'\26 B \26  B'}"));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#characters")]
    [Spec("css-syntax-3", "#consume-escaped-code-point")]
    public void EscapeOutsideTheUnicodeRange_IsTheReplacementCharacter()
    {
        Assert.AreEqual("�", Before(@"#x::before{content:'\110000'}"));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#statements")]
    [Spec("css-syntax-3", "#parse-stylesheet")]
    [Spec("css-syntax-3", "#at-rules")]
    public void StyleSheet_IsAListOfAtRulesAndRuleSets()
    {
        var sheet = ParseStyleSheet(" @media print { p { color: red } } \n p { color: green } @page { margin: 1cm } ");

        Assert.AreEqual(3, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        Assert.AreEqual(RuleType.Style, sheet.Rules[1].Type);
        Assert.AreEqual(RuleType.Page, sheet.Rules[2].Type);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#at-rules")]
    [Spec("css-syntax-3", "#consume-at-rule")]
    [Spec("css-syntax-3", "#css-stylesheets")]
    public void AtRule_EndsAtTheNextSemicolonOrBlock()
    {
        var box = Styled("@foo bar; #x{color:green} @foo { #x{color:red} } #x{background-color:green}");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#at-rules")]
    [DataRow("@import 'a.css'; p { color: red } @import 'b.css';")]
    [DataRow("@media screen { @import 'b.css'; p { color: red } }")]
    public void MisplacedImport_IsIgnored(string css)
    {
        CollectionAssert.DoesNotContain(RequestedStyleSheets(css), "b.css");
    }

    [TestMethod]
    [Spec("css21", "syndata.html#block")]
    [Spec("css-syntax-3", "#consume-simple-block")]
    public void Block_MatchesBracketsAndSkipsBracesInStrings()
    {
        Assert.AreEqual(Green, Styled(@"@foo { causta: ""}"" + ({7} * '\'') } #x { color: green }").Color);
        Assert.AreEqual(Green, Styled(@"#x { causta: ""}"" + (7 * '\'') ; color: green }").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#block")]
    [Spec("css-syntax-3", "#consume-simple-block")]
    public void BlockInADeclarationValue_IsSkippedAsPartOfTheDeclaration()
    {
        Assert.AreEqual(Green, Styled(@"#x { causta: ""}"" + ({7} * '\'') ; color: green }").Color);
        Assert.AreEqual(Green, Styled("#x { causta: [{7}] ; color: green }").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#rule-sets")]
    [Spec("css-syntax-3", "#style-rules")]
    public void InvalidSelector_DropsTheWholeRuleSet()
    {
        var css = "h1, h2 {color: green} h3, h4 # h5 {color: red} h6 {color: black}";

        Assert.AreNotEqual(Red, Styled(css, "<h3 id=x>a</h3>").Color);
        Assert.AreEqual("rgb(0, 0, 0)", Styled(css, "<h6 id=x>a</h6>").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#rule-sets")]
    public void BracesInASelectorString_DontEndTheSelector()
    {
        Assert.AreEqual(Green, Styled("#x[title=\"public class foo {}\"] { color: green }", "<div id=x title='public class foo {}'>a</div>").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#rule-sets")]
    public void AmpersandInATopLevelSelector_DropsTheWholeRuleSet()
    {
        Assert.AreNotEqual(Red, Styled("h3, h4 & h5 {color: red}", "<h3 id=x>a</h3>").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#declaration")]
    public void InvalidDeclarations_AreIgnoredIndividually()
    {
        var box = Styled("#x { color: red; font-style: 12pt } #x { color: green; font-vendor: any; font-variant: small-caps }");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual("normal", box.FontStyle);
        Assert.AreEqual("small-caps", box.FontVariant);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#declaration")]
    public void SemicolonInAString_DoesntEndTheDeclaration()
    {
        var box = Styled("#x::before { content: 'a;b'; color: green }");

        Assert.AreEqual("a;b", BeforeText(box));
        Assert.AreEqual(Green, box.Boxes.First(b => b.IsBeforePseudoElement).Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#comments")]
    [Spec("css-syntax-3", "#consume-comment")]
    public void Comments_DontNest()
    {
        Assert.AreEqual(Green, Styled("#x { color: red; /* /* */ color: green; /* */ }").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#comments")]
    [Spec("css-syntax-3", "#consume-stylesheet-contents")]
    public void SgmlCommentDelimiters_AreIgnoredAroundStatements()
    {
        var box = Styled("<!-- #x { color: green } --> <!-- #x { background-color: green } -->");

        Assert.AreEqual(Green, box.Color);
        Assert.AreEqual(Green, box.BackgroundColor);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    [DataRow("#x { color: green; rotation: 70minutes }")]
    [DataRow("#x { color: green; float: left here }")]
    [DataRow("#x { color: green; color }")]
    [DataRow("#x { color: red; color; color: green }")]
    [DataRow("#x { color: green; color: }")]
    [DataRow("#x { color: red; color:; color: green }")]
    [DataRow("#x { color: green; color{;color:maroon} }")]
    [DataRow("#x { color: red; color{;color:maroon}; color: green }")]
    [DataRow("#x { color: green } #x @here {color: red} @foo @bar; #x { background-color: red }")]
    [DataRow("}} {{ - }} #x { color: green }")]
    [DataRow(") ( {} ) #x { color: red } #x { color: green }")]
    [DataRow("@three-dee { @background-lighting { azimuth: 30deg; } #x { color: red } } #x { color: green }")]
    [DataRow("@media screen { #x { color: green; azimuth: 30deg; } }")]
    [DataRow("@media screen { #x { color: green")]
    [DataRow("#x { color: green; font-family: 'Courier New Times\n color: red; }")]
    [Spec("css-syntax-3", "#parser-algorithms")]
    public void ParsingError_IsIgnoredAndParsingResumes(string css)
    {
        Assert.AreEqual(Green, Styled(css).Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    [Spec("css-syntax-3", "#consume-string-token")]
    public void UnexpectedEndOfStyleSheet_ClosesTheOpenString()
    {
        Assert.AreEqual("Hello", Before("@media screen { #x::before { content: 'Hello"));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#numbers")]
    [Spec("css-values-4", "#numbers")]
    [DataRow("+10px", 10)]
    [DataRow(".5em", 10)]
    [DataRow("0.5em", 10)]
    [DataRow("10.0px", 10)]
    public void Number_AllowsASignAndAFraction(string value, double expected)
    {
        Assert.AreEqual(expected, Styled($"#x{{font-size:20px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#numbers")]
    [Spec("css-values-4", "#numbers")]
    [DataRow("10.px")]
    [DataRow("1,5px")]
    public void MalformedNumber_IsIgnored(string value)
    {
        Assert.AreEqual(5, Styled($"#x{{margin-left:5px;margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    public void NegativeZero_IsNotNegative()
    {
        Assert.AreEqual(0, Styled("#x{width:50px;width:-0px}").ActualWidth, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#absolute-lengths")]
    [DataRow("1in")]
    [DataRow("2.54cm")]
    [DataRow("25.4mm")]
    [DataRow("72pt")]
    [DataRow("6pc")]
    [DataRow("96px")]
    public void AbsoluteUnits_AreFixedInRelationToEachOther(string value)
    {
        Assert.AreEqual(96, Styled($"#x{{margin-left:{value}}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#lengths")]
    public void UnitAfterZero_IsOptional()
    {
        Assert.AreEqual(0, Styled("#x{margin-left:5px;margin-left:0}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#lengths")]
    public void UnitAfterNonZero_IsRequired()
    {
        Assert.AreEqual(5, Styled("#x{margin-left:5px;margin-left:3}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void Em_IsTheFontSizeOfTheElement()
    {
        Assert.AreEqual(40, Styled("#x{font-size:20px;margin-left:2em}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void EmInFontSize_IsTheFontSizeOfTheParent()
    {
        var box = Styled("#p{font-size:20px} #x{font-size:1.5em;margin-left:1em}", "<div id=p><div id=x>a</div></div>");

        Assert.AreEqual(30, box.ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void Ex_IsHalfAnEmWithoutFontMetrics()
    {
        Assert.AreEqual(20, Styled("#x{font-size:20px;margin-left:2ex}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#font-relative-lengths")]
    public void EmOnTheRoot_RefersToTheInitialFontSize()
    {
        var (root, _) = LayoutHarness.Layout("<html style='font-size:2em'><body style='margin:0'><div id=x style='margin-left:1em'>a</div></body></html>");
        var medium = Styled("#x{font-size:medium;margin-left:1em}").ActualMarginLeft;

        Assert.AreEqual(2 * medium, LayoutHarness.FindById(root, "x")!.ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#length-units")]
    public void Children_InheritTheComputedLengthNotTheEmValue()
    {
        var box = Styled("#p{font-size:12px;text-indent:3em} #x{font-size:15px}", "<div id=p><div id=x>a</div></div>");

        Assert.AreEqual(36, box.ActualTextIndent, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#length-units")]
    [Spec("css-values-4", "#numeric-ranges")]
    [DataRow("width")]
    [DataRow("height")]
    [DataRow("padding-left")]
    [DataRow("border-left-width")]
    [DataRow("font-size")]
    [DataRow("line-height")]
    public void NegativeLength_OnAPropertyThatDoesntAllowIt_IsIgnored(string property)
    {
        var box = Styled($"#x{{border-left-style:solid;{property}:5px;{property}:-10px}}");

        Assert.AreEqual("5px", property switch
        {
            "width" => box.Width,
            "height" => box.Height,
            "padding-left" => box.PaddingLeft,
            "border-left-width" => box.BorderLeftWidth,
            "font-size" => box.FontSize,
            _ => box.LineHeight,
        });
    }

    [TestMethod]
    [Spec("css21", "syndata.html#percentage-units")]
    [Spec("css-values-4", "#percentages")]
    public void Percentage_ResolvesAgainstTheReferenceValue()
    {
        Assert.AreEqual(50, Styled("#x{width:25%}", "<div style='width:200px'><div id=x>a</div></div>").ActualWidth, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#percentage-units")]
    [Spec("css-values-4", "#percentages")]
    public void Children_InheritTheComputedFontSizeNotThePercentage()
    {
        var box = Styled("#p{font-size:10px} #q{font-size:200%} #x{margin-left:1em}", "<div id=p><div id=q><div id=x>a</div></div></div>");

        Assert.AreEqual(20, box.ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#percentage-units")]
    public void Children_InheritTheComputedLineHeightNotThePercentage()
    {
        var box = Styled("#p{font-size:10px;line-height:120%} #x{font-size:20px}", "<div id=p><div id=x>a</div></div>");

        Assert.AreEqual("12px", box.LineHeight);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#uri")]
    [Spec("css-values-4", "#urls")]
    [DataRow("url(a.png)")]
    [DataRow("url( a.png )")]
    [DataRow("url('a.png')")]
    [DataRow("url( \"a.png\" )")]
    public void Url_AllowsQuotesAndWhitespace(string value)
    {
        Assert.AreEqual("url(\"a.png\")", Styled($"#x{{background-image:{value}}}").BackgroundImage);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#uri")]
    [Spec("css-values-4", "#urls")]
    public void UnquotedUrl_TakesEscapedParentheses()
    {
        Assert.AreEqual("url(\"a(b).png\")", Styled(@"#x{background-image:url(a\(b\).png)}").BackgroundImage);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#uri")]
    public void CommentInsideAUrl_IsPartOfTheUrl()
    {
        Assert.AreEqual("url(\"/*x*/pic.png\")", Styled("#x{background-image:url(/*x*/pic.png)}").BackgroundImage);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#uri")]
    [Spec("css-values-4", "#relative-urls")]
    [Spec("css-values-4", "#url-processing")]
    [DataRow("url(yellow)")]
    [DataRow("url('yellow')")]
    public void RelativeUrl_ResolvesAgainstTheStyleSheet(string value)
    {
        var (box, href) = StyledWithFile("style/basic.css", Encoding.UTF8.GetBytes($"#x {{ background-image: {value} }}"));

        StringAssert.Contains(box.BackgroundImage, new Uri(new Uri(href), "yellow").AbsoluteUri);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#uri")]
    [Spec("css-values-4", "#relative-urls")]
    public void DoubleQuotedRelativeUrl_ResolvesAgainstTheStyleSheet()
    {
        var (box, href) = StyledWithFile("style/basic.css", Encoding.UTF8.GetBytes("#x { background-image: url(\"yellow\") }"));

        StringAssert.Contains(box.BackgroundImage, new Uri(new Uri(href), "yellow").AbsoluteUri);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#counter")]
    [DataRow("counter(c)", "3")]
    [DataRow("counter( c )", "3")]
    [DataRow("counter(c, upper-roman)", "III")]
    public void CounterNotation_FormatsTheCounter(string value, string expected)
    {
        Assert.AreEqual(expected + "|", Before($"#x{{counter-reset:c 3}} #x::before{{content:{value} '|'}}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#counter")]
    [DataRow("counter(c, none)", "")]
    [DataRow("counters(c, '.')", "2.3")]
    [DataRow("counters( c , '.' , lower-alpha )", "b.c")]
    public void CounterNoneAndCountersNotation_FormatTheCounters(string value, string expected)
    {
        var (root, _) = LayoutHarness.Layout(Document(
            $"#o{{counter-reset:c 2}} #x{{counter-reset:c 3}} #x::before{{content:{value} '|'}}",
            "<div id=o><div id=x>a</div></div>"));

        Assert.AreEqual(expected + "|", BeforeText(LayoutHarness.FindById(root, "x")!));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    [Spec("css-color-4", "#rgb-functions")]
    [DataRow("maroon", 0x80, 0x00, 0x00)]
    [DataRow("red", 0xff, 0x00, 0x00)]
    [DataRow("orange", 0xff, 0xa5, 0x00)]
    [DataRow("yellow", 0xff, 0xff, 0x00)]
    [DataRow("olive", 0x80, 0x80, 0x00)]
    [DataRow("purple", 0x80, 0x00, 0x80)]
    [DataRow("fuchsia", 0xff, 0x00, 0xff)]
    [DataRow("white", 0xff, 0xff, 0xff)]
    [DataRow("lime", 0x00, 0xff, 0x00)]
    [DataRow("green", 0x00, 0x80, 0x00)]
    [DataRow("navy", 0x00, 0x00, 0x80)]
    [DataRow("blue", 0x00, 0x00, 0xff)]
    [DataRow("aqua", 0x00, 0xff, 0xff)]
    [DataRow("teal", 0x00, 0x80, 0x80)]
    [DataRow("black", 0x00, 0x00, 0x00)]
    [DataRow("silver", 0xc0, 0xc0, 0xc0)]
    [DataRow("gray", 0x80, 0x80, 0x80)]
    [DataRow("#fb0", 0xff, 0xbb, 0x00)]
    [DataRow("#FFBB00", 0xff, 0xbb, 0x00)]
    [DataRow("rgb(255,187,0)", 0xff, 0xbb, 0x00)]
    [DataRow("rgb( 100% , 0% , 0% )", 0xff, 0x00, 0x00)]
    [DataRow("rgb(300,0,0)", 0xff, 0x00, 0x00)]
    public void Color_ResolvesToTheSpecifiedRgbValue(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Styled($"#x{{color:{value}}}").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#color-units")]
    [Spec("css-color-4", "#rgb-functions")]
    [DataRow("rgb(255,-10,0)")]
    [DataRow("rgb(110%, 0%, 0%)")]
    public void ColorOutsideTheGamut_IsClipped(string value)
    {
        Assert.AreEqual(RColor.FromArgb(255, 255, 0, 0), Styled($"#x{{color:{value}}}").ActualColor);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    [Spec("css-color-4", "#hex-notation")]
    [DataRow("#ff000")]
    [DataRow("#gg0000")]
    [DataRow("ff0000")]
    [DataRow("\"red\"")]
    public void InvalidColor_IsIgnored(string value)
    {
        Assert.AreEqual(Green, Styled($"#x{{color:green;color:{value}}}").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#color-units")]
    [Spec("css-color-4", "#color-syntax-legacy")]
    [DataRow("rgb(255, 0%, 0)")]
    [DataRow("rgb(255, 0)")]
    public void InvalidRgbFunction_IsIgnored(string value)
    {
        Assert.AreEqual(Green, Styled($"#x{{color:green;color:{value}}}").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#strings")]
    [Spec("css-values-4", "#strings")]
    [DataRow("\"this is a 'string'\"", "this is a 'string'")]
    [DataRow("\"this is a \\\"string\\\"\"", "this is a \"string\"")]
    [DataRow("'this is a \"string\"'", "this is a \"string\"")]
    [DataRow("'this is a \\'string\\''", "this is a 'string'")]
    [DataRow("'\\22  and \\27'", "\" and '")]
    public void String_TakesEitherQuoteAndEscapes(string value, string expected)
    {
        Assert.AreEqual(expected, Before($"#x::before{{content:{value}}}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#strings")]
    [Spec("css-syntax-3", "#consume-string-token")]
    [Spec("css-values-4", "#strings")]
    public void StringBrokenOverLines_IsTheSameString()
    {
        var css = "#x[title=\"a not s\\\no very long title\"] { color: green }";

        Assert.AreEqual(Green, Styled(css, "<div id=x title='a not so very long title'>a</div>").Color);
        Assert.AreEqual("ab", Before("#x::before{content:'a\\\nb'}"));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#unsupported-values")]
    public void UnsupportedValue_IsIgnored()
    {
        Assert.AreEqual("inline", Styled("#x{display:inline;display:run-in}").Display);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#charset")]
    [DataRow("utf-8")]
    [DataRow("utf-16")]
    [DataRow("utf-16BE")]
    [Spec("css-syntax-3", "#input-byte-stream")]
    public void ExternalStyleSheet_IsDecodedFromItsByteOrderMark(string name)
    {
        var encoding = Encoding.GetEncoding(name);
        var (box, _) = StyledWithFile("style.css", encoding.GetPreamble().Concat(encoding.GetBytes("#x::before{content:'κ'}")).ToArray());

        Assert.AreEqual("κ", BeforeText(box));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#charset")]
    [Spec("css-syntax-3", "#input-byte-stream")]
    public void ExternalStyleSheet_WithoutBomOrCharset_IsUtf8()
    {
        var (box, _) = StyledWithFile("style.css", Encoding.UTF8.GetBytes("#x::before{content:'κ'}"));

        Assert.AreEqual("κ", BeforeText(box));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "syndata.html#charset")]
    [Spec("css-syntax-3", "#input-byte-stream")]
    public void ExternalStyleSheet_IsDecodedWithItsCharsetRule()
    {
        var bytes = Encoding.ASCII.GetBytes("@charset \"ISO-8859-1\"; #x::before{content:'L")
            .Concat(new byte[] { 0xFC })
            .Concat(Encoding.ASCII.GetBytes("beck'}")).ToArray();

        Assert.AreEqual("Lübeck", BeforeText(StyledWithFile("style.css", bytes).Box));
    }

    [TestMethod]
    [Spec("css21", "syndata.html#charset")]
    [Spec("css-syntax-3", "#charset-rule")]
    public void CharsetRuleNotAtTheStart_IsIgnored()
    {
        Assert.AreEqual(Green, Styled("#x{color:red} @charset \"ISO-8859-1\"; #x{color:green}").Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#escaping")]
    public void HexEscapes_ReferToUnicodeCharacters()
    {
        Assert.AreEqual("κουρος", Before(@"#x::before{content:'\3BA\3BF\3C5\3C1\3BF\3C2'}"));
        Assert.AreEqual("Lübeck", Before(@"#x::before{content:'L\FC beck'}"));
    }

    private static string Document(string css, string body) =>
        $"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>";

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout(Document(css, body));
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static string Before(string css) => BeforeText(Styled(css));

    private static string BeforeText(CssBox box)
    {
        var before = box.Boxes.FirstOrDefault(b => b.IsBeforePseudoElement);
        Assert.IsNotNull(before, "No ::before box");
        return before.Text ?? string.Concat(LayoutHarness.Descendants(before).SelectMany(b => b.Words).Select(w => w.Text));
    }

    /// <summary>The style sheets the renderer requests while loading a document with <paramref name="css"/>.</summary>
    private static string[] RequestedStyleSheets(string css)
    {
        var requested = new List<string>();
        var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(1000, 4000) };
        container.StylesheetLoad += (_, e) =>
        {
            requested.Add(e.Src);
            e.SetStyleSheet = "p { color: green }";
        };

        container.SetHtml(Document(css, "<p>a</p>")).GetAwaiter().GetResult();
        return requested.ToArray();
    }

    /// <summary>Lays out a document that links the given style sheet file from a temporary folder.</summary>
    private static (CssBox Box, string Href) StyledWithFile(string path, byte[] bytes)
    {
        var dir = Path.Combine(Path.GetTempPath(), "html-renderer-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var file = Path.Combine(dir, path);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllBytes(file, bytes);
            var href = new Uri(file).AbsoluteUri;

            var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(1000, 4000) };
            container.SetHtml($"<html><head><link rel=stylesheet href='{href}'></head><body><div id=x>a</div></body></html>").GetAwaiter().GetResult();
            using var graphics = new RecordingGraphics();
            container.PerformLayout(graphics);

            var box = LayoutHarness.FindById(container.Root!, "x");
            Assert.IsNotNull(box);
            return (box, href);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
