using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Which CSS Text Level 3 values the CSS engine accepts.</summary>
[TestClass]
public sealed class CssText3PropertyTests
{
    [TestMethod]
    [Spec("css-text-3", "#text-transform-property")]
    [DataRow("none")]
    [DataRow("capitalize")]
    [DataRow("uppercase")]
    [DataRow("lowercase")]
    [DataRow("full-width")]
    public void TextTransform_SingleKeywords_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("text-transform", value));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#text-transform-property")]
    [DataRow("full-size-kana")]
    [DataRow("uppercase full-width")]
    public void TextTransform_FullSizeKanaAndCombinations_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("text-transform", value));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#text-align-property")]
    [DataRow("start")]
    [DataRow("end")]
    [DataRow("match-parent")]
    [DataRow("justify-all")]
    public void TextAlign_Text3Keywords_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("text-align", value));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#text-align-last-property")]
    public void TextAlignLast_MatchParent_IsParsed()
    {
        Assert.AreEqual("match-parent", Value("text-align-last", "match-parent"));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#white-space-property")]
    public void WhiteSpace_BreakSpaces_IsParsed()
    {
        Assert.AreEqual("break-spaces", Value("white-space", "break-spaces"));
    }

    [TestMethod]
    [Spec("css-text-3", "#hyphenation")]
    [DataRow("none")]
    [DataRow("manual")]
    [DataRow("auto")]
    public void Hyphens_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("hyphens", value));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#overflow-wrap-property")]
    public void OverflowWrap_Anywhere_IsParsed()
    {
        Assert.AreEqual("anywhere", Value("overflow-wrap", "anywhere"));
    }

    [Ignore("Gap, see Spec/css-text-3.yml")]
    [TestMethod]
    [Spec("css-text-3", "#word-break-property")]
    public void WordBreak_BreakWord_IsParsed()
    {
        Assert.AreEqual("break-word", Value("word-break", "break-word"));
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
