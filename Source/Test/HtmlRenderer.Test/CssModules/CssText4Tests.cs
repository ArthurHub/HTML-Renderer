using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>The values CSS Text Level 4 adds over Level 3, which the CSS engine doesn't accept yet.</summary>
[TestClass]
public sealed class CssText4Tests
{
    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#text-transform-property")]
    public void TextTransform_MathAuto_IsParsed()
    {
        Assert.AreEqual("math-auto", Value("text-transform", "math-auto"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#word-space-transform")]
    [DataRow("space")]
    [DataRow("ideographic-space auto-phrase")]
    public void WordSpaceTransform_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("word-space-transform", value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#white-space-property")]
    [DataRow("collapse")]
    [DataRow("preserve nowrap")]
    public void WhiteSpace_LonghandCombinations_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("white-space", value));
    }

    [TestMethod]
    [Spec("css-text-4", "#white-space-property")]
    public void WhiteSpace_KeywordWithALonghandValue_IsRejected()
    {
        Assert.AreEqual("", Value("white-space", "pre-line discard-after"));
    }

    [TestMethod]
    [Spec("css-text-4", "#white-space-collapsing")]
    [DataRow("preserve")]
    [DataRow("preserve-breaks")]
    [DataRow("preserve-spaces")]
    public void WhiteSpaceCollapse_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("white-space-collapse", value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#white-space-collapsing")]
    [DataRow("discard")]
    public void WhiteSpaceCollapse_AcceptsDiscard(string value)
    {
        Assert.AreEqual(value, Value("white-space-collapse", value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#white-space-trim")]
    public void WhiteSpaceTrim_IsParsed()
    {
        Assert.AreEqual("discard-before discard-after", Value("white-space-trim", "discard-before discard-after"));
    }

    [TestMethod]
    [Spec("css-text-4", "#tab-size-property")]
    [DataRow("4")]
    [DataRow("20px")]
    public void TabSize_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("tab-size", value));
    }

    [TestMethod]
    [Spec("css-text-4", "#text-wrap-mode")]
    public void TextWrapMode_IsParsed()
    {
        Assert.AreEqual("nowrap", Value("text-wrap-mode", "nowrap"));
    }

    // The box only knows the white-space keyword, the longhands are combined into it
    [TestMethod]
    [Spec("css-text-4", "#text-wrap-mode")]
    [Spec("css-text-4", "#white-space-collapsing")]
    [DataRow("text-wrap-mode:nowrap", "nowrap")]
    [DataRow("white-space-collapse:preserve", "pre-wrap")]
    [DataRow("white-space-collapse:preserve;text-wrap-mode:nowrap", "pre")]
    [DataRow("white-space-collapse:preserve-breaks", "pre-line")]
    [DataRow("white-space:pre;text-wrap-mode:wrap", "pre-wrap")]
    public void Longhands_SetTheWhiteSpaceOfTheBox(string declarations, string whiteSpace)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id=x style='{declarations}'>a</div>"));

        Assert.AreEqual(whiteSpace, LayoutHarness.FindById(root, "x")!.WhiteSpace);
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#wrap-inside")]
    public void WrapInside_IsParsed()
    {
        Assert.AreEqual("avoid", Value("wrap-inside", "avoid"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#wrap-before")]
    [DataRow("wrap-before")]
    [DataRow("wrap-after")]
    public void WrapBeforeAfter_IsParsed(string property)
    {
        Assert.AreEqual("avoid-line", Value(property, "avoid-line"));
    }

    [TestMethod]
    [Spec("css-text-4", "#text-wrap-style")]
    [DataRow("balance")]
    [DataRow("pretty")]
    [DataRow("stable")]
    public void TextWrapStyle_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-wrap-style", value));
    }

    [TestMethod]
    [Spec("css-text-4", "#text-wrap-shorthand")]
    [DataRow("nowrap", "nowrap", "auto")]
    [DataRow("wrap balance", "wrap", "balance")]
    public void TextWrap_SetsItsLonghands(string value, string mode, string style)
    {
        var declarations = CssConstructionFunctions.ParseDeclarations($"text-wrap: {value}");

        Assert.AreEqual(mode, declarations.GetPropertyValue("text-wrap-mode"));
        Assert.AreEqual(style, declarations.GetPropertyValue("text-wrap-style"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#word-break-property")]
    [DataRow("manual")]
    [DataRow("auto-phrase")]
    public void WordBreak_Level4Keywords_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("word-break", value));
    }

    [TestMethod]
    [Spec("css-text-4", "#line-break-property")]
    [DataRow("strict")]
    [DataRow("anywhere")]
    public void LineBreak_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("line-break", value));
    }

    [TestMethod]
    [Spec("css-text-4", "#hyphenate-character")]
    public void HyphenateCharacter_IsParsed()
    {
        Assert.AreEqual("\"-\"", Value("hyphenate-character", "\"-\""));
    }

    [TestMethod]
    [Spec("css-text-4", "#hyphenate-size-limits")]
    public void HyphenateLimitZone_IsParsed()
    {
        Assert.AreEqual("10%", Value("hyphenate-limit-zone", "10%"));
    }

    [TestMethod]
    [Spec("css-text-4", "#hyphenate-char-limits")]
    public void HyphenateLimitChars_IsParsed()
    {
        Assert.AreEqual("6 3 2", Value("hyphenate-limit-chars", "6 3 2"));
    }

    [TestMethod]
    [Spec("css-text-4", "#hyphenate-line-limits")]
    [DataRow("hyphenate-limit-lines", "2")]
    [DataRow("hyphenate-limit-last", "always")]
    public void HyphenateLineLimits_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#character-alignment")]
    public void TextAlign_String_IsParsed()
    {
        Assert.AreEqual("\".\" center", Value("text-align", "\".\" center"));
    }

    [TestMethod]
    [Spec("css-text-4", "#text-align-all-property")]
    public void TextAlignAll_IsParsed()
    {
        Assert.AreEqual("center", Value("text-align-all", "center"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#text-justify-property")]
    [DataRow("none")]
    [DataRow("inter-character")]
    [DataRow("ruby")]
    [DataRow("auto no-compress")]
    public void TextJustify_Level4Keywords_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("text-justify", value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#text-group-align-property")]
    public void TextGroupAlign_IsParsed()
    {
        Assert.AreEqual("center", Value("text-group-align", "center"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#word-spacing-property")]
    public void WordSpacing_Percentage_IsParsed()
    {
        Assert.AreEqual("10%", Value("word-spacing", "10%"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#letter-spacing-property")]
    public void LetterSpacing_Percentage_IsParsed()
    {
        Assert.AreEqual("10%", Value("letter-spacing", "10%"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#line-padding-property")]
    public void LinePadding_IsParsed()
    {
        Assert.AreEqual("2px", Value("line-padding", "2px"));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#text-autospace-property")]
    [DataRow("normal")]
    [DataRow("ideograph-alpha")]
    [DataRow("no-autospace")]
    public void TextAutospace_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-autospace", value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#text-spacing-trim-property")]
    [DataRow("trim-start")]
    [DataRow("space-all")]
    public void TextSpacingTrim_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-spacing-trim", value));
    }

    [Ignore("Gap, see Spec/css-text-4.yml")]
    [TestMethod]
    [Spec("css-text-4", "#text-spacing-property")]
    [DataRow("none")]
    [DataRow("auto")]
    public void TextSpacing_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-spacing", value));
    }

    [TestMethod]
    [Spec("css-text-4", "#hanging-punctuation-property")]
    [DataRow("first")]
    [DataRow("first allow-end last")]
    public void HangingPunctuation_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("hanging-punctuation", value));
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
