using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>The values CSS Text Decoration Level 4 adds over Level 3, which the CSS engine doesn't accept yet.</summary>
[TestClass]
public sealed class CssTextDecor4Tests
{
    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-line-property")]
    [DataRow("spelling-error")]
    [DataRow("grammar-error")]
    public void TextDecorationLine_ErrorKeywords_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("text-decoration-line", value));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-thickness-property")]
    [DataRow("from-font")]
    [DataRow("3px")]
    [DataRow("10%")]
    public void TextDecorationThickness_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-decoration-thickness", value));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-property")]
    public void TextDecoration_TakesTheThickness()
    {
        Assert.AreEqual("3px", Value("text-decoration", "underline 3px", "text-decoration-thickness"));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-underline-position-property")]
    [DataRow("under")]
    [DataRow("from-font")]
    [DataRow("under left")]
    public void TextUnderlinePosition_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-underline-position", value));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#underline-offset")]
    [DataRow("3px")]
    [DataRow("10%")]
    public void TextUnderlineOffset_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-underline-offset", value));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skipping")]
    public void TextDecorationSkip_IsParsed()
    {
        Assert.AreEqual("none", Value("text-decoration-skip", "none"));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-self-property")]
    public void TextDecorationSkipSelf_IsParsed()
    {
        Assert.AreEqual("none", Value("text-decoration-skip-self", "none"));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-box-property")]
    public void TextDecorationSkipBox_IsParsed()
    {
        Assert.AreEqual("all", Value("text-decoration-skip-box", "all"));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-inset-property")]
    public void TextDecorationSkipInset_IsParsed()
    {
        Assert.AreEqual("auto", Value("text-decoration-skip-inset", "auto"));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-spaces-property")]
    public void TextDecorationSkipSpaces_IsParsed()
    {
        Assert.AreEqual("all", Value("text-decoration-skip-spaces", "all"));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-ink-property")]
    [DataRow("none")]
    [DataRow("all")]
    public void TextDecorationSkipInk_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-decoration-skip-ink", value));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-emphasis-style-property")]
    [DataRow("filled dot")]
    [DataRow("\"x\"")]
    public void TextEmphasisStyle_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-emphasis-style", value));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-emphasis-skip")]
    public void TextEmphasisSkip_IsParsed()
    {
        Assert.AreEqual("spaces symbols", Value("text-emphasis-skip", "spaces symbols"));
    }

    private static string Value(string property, string value, string? longhand = null)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(longhand ?? property);
    }
}
