using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssFonts5Tests
{
    [TestMethod]
    [Spec("css-fonts-5", "#font-size-adjust-prop")]
    [DataRow("none")]
    [DataRow("0.5")]
    public void FontSizeAdjust_NoneAndNumber_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("font-size-adjust", value));
    }

    [Ignore("Gap, see Spec/css-fonts-5.yml")]
    [TestMethod]
    [Spec("css-fonts-5", "#font-size-adjust-prop")]
    [DataRow("from-font")]
    [DataRow("ex-height 0.5")]
    [DataRow("cap-height from-font")]
    [DataRow("ic-height 0.5")]
    public void FontSizeAdjust_MetricKeywordsAndFromFont_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("font-size-adjust", value));
    }

    [TestMethod]
    [Spec("css-fonts-5", "#font-face-src-parsing")]
    public void Src_WithFormatKeywordAndTech_IsKept()
    {
        var src = Descriptor("src", "url(a.woff2) format(woff2) tech(variations, avar2), url(b.ttf)");

        StringAssert.Contains(src, "a.woff2");
        StringAssert.Contains(src, "b.ttf");
    }

    [Ignore("Gap, see Spec/css-fonts-5.yml")]
    [TestMethod]
    [Spec("css-fonts-5", "#font-size-desc")]
    public void FontSizeDescriptor_IsParsed()
    {
        Assert.AreEqual("8 12", Descriptor("font-size", "8 12"));
    }

    [Ignore("Gap, see Spec/css-fonts-5.yml")]
    [TestMethod]
    [Spec("css-fonts-5", "#size-adjust-desc")]
    public void SizeAdjustDescriptor_IsParsed()
    {
        Assert.AreEqual("90%", Descriptor("size-adjust", "90%"));
    }

    [Ignore("Gap, see Spec/css-fonts-5.yml")]
    [TestMethod]
    [Spec("css-fonts-5", "#font-metrics-override-desc")]
    [DataRow("ascent-override")]
    [DataRow("descent-override")]
    [DataRow("line-gap-override")]
    public void MetricsOverrideDescriptors_AreParsed(string descriptor)
    {
        Assert.AreEqual("80%", Descriptor(descriptor, "80%"));
    }

    [Ignore("Gap, see Spec/css-fonts-5.yml")]
    [TestMethod]
    [Spec("css-fonts-5", "#font-sup-sub-override-desc")]
    [DataRow("superscript-position-override")]
    [DataRow("subscript-position-override")]
    [DataRow("superscript-size-override")]
    [DataRow("subscript-size-override")]
    public void SuperscriptAndSubscriptOverrideDescriptors_AreParsed(string descriptor)
    {
        Assert.AreEqual("from-font", Descriptor(descriptor, "from-font"));
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }

    private static string Descriptor(string descriptor, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($"@font-face {{ font-family: x; {descriptor}: {value}; }}");
        return sheet.Rules.OfType<FontFaceRule>().Single().GetPropertyValue(descriptor);
    }
}
