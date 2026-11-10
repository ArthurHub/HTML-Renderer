using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Which CSS Fonts Level 4 values and descriptors the CSS engine accepts.</summary>
[TestClass]
public sealed class CssFonts4PropertyTests
{
    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-style-prop")]
    [DataRow("left")]
    [DataRow("right")]
    public void FontStyle_LeftAndRight_AreAccepted(string value)
    {
        Assert.AreEqual(value, Value("font-style", value));
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-style-prop")]
    [DataRow("oblique 90deg")]
    [DataRow("oblique -90deg")]
    [DataRow("oblique 0deg")]
    public void FontStyle_ObliqueAngleWithinRange_IsAccepted(string value)
    {
        Assert.AreEqual(value, Value("font-style", value));
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-style-prop")]
    [DataRow("oblique 91deg")]
    [DataRow("oblique -100deg")]
    public void FontStyle_ObliqueAngleOutOfRange_IsRejected(string value)
    {
        Assert.AreEqual("", Value("font-style", value));
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-size-prop")]
    [DataRow("xxx-large")]
    [DataRow("math")]
    public void FontSize_Fonts4Keywords_AreAccepted(string value)
    {
        Assert.AreEqual(value, Value("font-size", value));
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-variant-prop")]
    [DataRow("normal")]
    [DataRow("small-caps")]
    public void FontVariant_Css21Keywords_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("font-variant", value));
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-variant-prop")]
    [DataRow("all-small-caps")]
    [DataRow("common-ligatures tabular-nums")]
    [DataRow("none")]
    public void FontVariant_Fonts4Values_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("font-variant", value));
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-prop-desc")]
    [DataRow("font-weight: 100 900", "100 900")]
    [DataRow("font-weight: 350", "350")]
    public void FontFace_WeightDescriptor_AcceptsRangesAndAnyNumber(string declaration, string expected)
    {
        Assert.AreEqual(expected, FontFace(declaration).Weight);
    }

    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    [TestMethod]
    [Spec("css-fonts-4", "#font-prop-desc")]
    public void FontFace_StretchDescriptor_AcceptsPercentages()
    {
        Assert.AreEqual("75%", FontFace("font-stretch: 75%").Stretch);
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }

    private static IFontFaceRule FontFace(string declaration)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@font-face { font-family: X; src: local(Arial); " + declaration + " }");
        return (IFontFaceRule)sheet.Rules[0];
    }
}
