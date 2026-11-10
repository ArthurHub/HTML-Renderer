using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

/// <summary>Which CSS Masking values the CSS engine accepts.</summary>
[TestClass]
public sealed class CssMaskingTests
{
    [Ignore("Gap, see Spec/css-masking-1.yml")]
    [TestMethod]
    [Spec("css-masking-1", "#the-clip-path")]
    [DataRow("url(#clip)")]
    [DataRow("border-box")]
    [DataRow("fill-box")]
    [DataRow("inset(10px) padding-box")]
    [DataRow("content-box circle(10px)")]
    public void ClipPath_ClipSourceAndGeometryBox_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("clip-path", value));
    }

    [Ignore("Gap, see Spec/css-masking-1.yml")]
    [TestMethod]
    [Spec("css-masking-1", "#the-clip-rule")]
    [Spec("css-masking-1", "#the-mask-image")]
    [Spec("css-masking-1", "#the-mask-mode")]
    [Spec("css-masking-1", "#the-mask-repeat")]
    [Spec("css-masking-1", "#the-mask-position")]
    [Spec("css-masking-1", "#the-mask-clip")]
    [Spec("css-masking-1", "#the-mask-origin")]
    [Spec("css-masking-1", "#the-mask-size")]
    [Spec("css-masking-1", "#the-mask-composite")]
    [Spec("css-masking-1", "#the-mask-type")]
    [DataRow("clip-rule", "evenodd")]
    [DataRow("mask-image", "url(a.png)")]
    [DataRow("mask-mode", "luminance")]
    [DataRow("mask-repeat", "no-repeat")]
    [DataRow("mask-position", "left top")]
    [DataRow("mask-clip", "content-box")]
    [DataRow("mask-origin", "padding-box")]
    [DataRow("mask-size", "contain")]
    [DataRow("mask-composite", "subtract")]
    [DataRow("mask-type", "alpha")]
    public void MaskProperties_AreParsed(string property, string value)
    {
        Assert.AreEqual(value, Value(property, value));
    }

    [Ignore("Gap, see Spec/css-masking-1.yml")]
    [TestMethod]
    [Spec("css-masking-1", "#the-mask")]
    public void MaskShorthand_SetsItsLonghands()
    {
        var style = Style("mask: url(a.png) no-repeat left top / contain");

        Assert.AreEqual("url(a.png)", style.GetPropertyValue("mask-image"));
        Assert.AreEqual("contain", style.GetPropertyValue("mask-size"));
    }

    [Ignore("Gap, see Spec/css-masking-1.yml")]
    [TestMethod]
    [Spec("css-masking-1", "#the-mask-border")]
    [Spec("css-masking-1", "#the-mask-border-source")]
    [Spec("css-masking-1", "#the-mask-border-slice")]
    public void MaskBorderShorthand_SetsItsLonghands()
    {
        var style = Style("mask-border: url(a.png) 30 fill");

        Assert.AreEqual("url(a.png)", style.GetPropertyValue("mask-border-source"));
        Assert.AreEqual("30 fill", style.GetPropertyValue("mask-border-slice"));
    }

    private static string Value(string property, string value) => Style($"{property}: {value}").GetPropertyValue(property);

    private static StyleDeclaration Style(string declarations)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {declarations}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style;
    }
}
