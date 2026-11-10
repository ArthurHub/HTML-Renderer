using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssColor5Tests
{
    private static readonly RColor Green = RColor.FromArgb(255, 0, 128, 0);

    [TestMethod]
    [Spec("css-color-5", "#color-mix")]
    [Spec("css-color-5", "#color-syntax")]
    [DataRow("color-mix(in srgb, red, blue)", 128, 0, 128, 255)]
    [DataRow("COLOR-MIX(IN SRGB, red, blue)", 128, 0, 128, 255)]
    [DataRow("color-mix(in srgb, #ff0000, rgb(0 0 255))", 128, 0, 128, 255)]
    [DataRow("color-mix(in srgb, color-mix(in srgb, red, blue), white)", 191, 128, 191, 255)]
    [DataRow("color-mix(in srgb, 25% red, blue)", 64, 0, 191, 255)]
    public void ColorMix_MixesTwoColors(string value, int r, int g, int b, int a)
    {
        AssertColor(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#color-mix")]
    public void ColorMix_ResolvesCurrentColor()
    {
        var box = Styled("#x{color:red;background-color:color-mix(in srgb, currentcolor, blue)}");

        AssertColor(RColor.FromArgb(255, 128, 0, 128), box.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#color-mix-result")]
    [DataRow("color-mix(in srgb, red)", 255, 0, 0)]
    [DataRow("color-mix(in srgb, red, blue, lime)", 85, 85, 85)]
    public void ColorMix_TakesOneOrMoreColors(string value, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-5", "#color-mix-space")]
    [DataRow("srgb", 128, 0, 128)]
    [DataRow("srgb-linear", 188, 0, 188)]
    [DataRow("xyz", 188, 0, 188)]
    [DataRow("oklab", 140, 83, 162)]
    public void ColorMix_MixesInTheGivenColorSpace(string space, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color($"color-mix(in {space}, red, blue)"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#color-mix-space")]
    public void ColorMix_WithoutAColorSpace_MixesInOklab()
    {
        AssertColor(RColor.FromArgb(255, 140, 83, 162), Color("color-mix(red, blue)"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#color-mix-space")]
    [Spec("css-color-5", "#color-mix-percent-norm")]
    [DataRow("color-mix(in foo, red, blue)")]
    [DataRow("color-mix(in srgb, red -10%, blue)")]
    [DataRow("color-mix(in srgb, red 110%, blue)")]
    public void InvalidColorMix_IsIgnored(string value)
    {
        Assert.AreEqual(Green, Color($"green;color:{value}"));
    }

    [TestMethod]
    [Spec("css-color-5", "#color-mix-percent-norm")]
    [Spec("css-color-5", "#color-mix-result")]
    [DataRow("color-mix(in srgb, red 25%, blue)", 64, 0, 191, 255)]
    [DataRow("color-mix(in srgb, red, blue 75%)", 64, 0, 191, 255)]
    [DataRow("color-mix(in srgb, red 60%, blue 60%)", 128, 0, 128, 255)]
    [DataRow("color-mix(in srgb, red 20%, blue 20%)", 128, 0, 128, 102)]
    [DataRow("color-mix(in srgb, red 0%, blue 0%)", 0, 0, 0, 0)]
    public void Percentages_AreNormalized(string value, int r, int g, int b, int a)
    {
        var color = Color(value);

        if (a == 0) Assert.AreEqual(0, color.A);
        else AssertColor(RColor.FromArgb(a, r, g, b), color);
    }

    [TestMethod]
    [Spec("css-color-5", "#color-mix-with-alpha")]
    public void ColorMix_PremultipliesTheAlpha()
    {
        AssertColor(RColor.FromArgb(83, 137, 118, 0), Color("color-mix(in srgb, rgba(255, 0, 0, 0.7) 25%, rgba(0, 255, 0, 0.2))"));
    }

    [TestMethod]
    [Spec("css-color-5", "#interpolation-space")]
    [DataRow("in hsl longer hue", 255, 0, 0)]
    [DataRow("in hsl increasing hue", 0, 255, 255)]
    public void ColorMix_TakesAHueInterpolationMethod(string method, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color($"color-mix({method}, lime, blue)"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#relative-syntax")]
    [DataRow("rgb(from red b g r)", 0, 0, 255)]
    [DataRow("hsl(from red calc(h + 120) s l)", 0, 255, 0)]
    [DataRow("oklch(from red l c h)", 255, 0, 0)]
    public void RelativeColor_IsDerivedFromTheOriginColor(string value, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#light-dark")]
    public void LightDark_IsTheLightColorInALightColorScheme()
    {
        Assert.AreEqual(RColor.FromArgb(255, 255, 0, 0), Color("light-dark(red, blue)"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#contrast-color")]
    [DataRow("white", 0, 0, 0)]
    [DataRow("black", 255, 255, 255)]
    public void ContrastColor_IsBlackOrWhite(string background, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color($"green;color:contrast-color({background})"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-5.yml")]
    [Spec("css-color-5", "#device-cmyk")]
    [Spec("css-color-5", "#cmyk-rgb")]
    [DataRow("device-cmyk(0 1 1 0)", 255, 0, 0)]
    [DataRow("device-cmyk(0% 0% 0% 100%)", 0, 0, 0)]
    public void DeviceCmyk_IsConvertedNaively(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color($"green;color:{value}"));
    }

    private static RColor Color(string value) => Styled($"#x{{color:{value}}}").ActualColor;

    private static void AssertColor(RColor expected, RColor actual)
    {
        var message = $"Expected {expected}, got {actual}";
        Assert.AreEqual((double)expected.R, actual.R, 1, message);
        Assert.AreEqual((double)expected.G, actual.G, 1, message);
        Assert.AreEqual((double)expected.B, actual.B, 1, message);
        Assert.AreEqual((double)expected.A, actual.A, 1, message);
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }
}
