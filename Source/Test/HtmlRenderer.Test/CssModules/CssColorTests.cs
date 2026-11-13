using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssColorTests
{
    private static readonly RColor Green = RColor.FromArgb(255, 0, 128, 0);

    [TestMethod]
    [Spec("css-color-4", "#color-syntax-modern")]
    [DataRow("rgb(255 0 0)", 255, 0, 0)]
    [DataRow("rgba(255 0 0)", 255, 0, 0)]
    [DataRow("RGB(255 0 0)", 255, 0, 0)]
    [DataRow("hwb(120 0% 0%)", 0, 255, 0)]
    [DataRow("lab(50 40 59.5)", 191, 87, 0)]
    [DataRow("lch(50 72 56deg)", 191, 87, 0)]
    [DataRow("oklab(0.628 0.225 0.126)", 255, 0, 0)]
    [DataRow("oklch(62.8% 0.258 29.2)", 255, 0, 0)]
    public void ModernSyntax_SeparatesTheComponentsWithSpaces(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#color-syntax-modern")]
    [DataRow("hsl(120 100% 50%)", 0, 255, 0)]
    [DataRow("hsla(120 100% 50%)", 0, 255, 0)]
    [DataRow("rgb(calc(200 + 55) 0 0)", 255, 0, 0)]
    public void ModernSyntax_IsSupportedByHslAndCalc(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#color-syntax-modern")]
    [DataRow("hsl(120deg 100 50)", 0, 255, 0)]
    [DataRow("rgb(100% 0% 0%)", 255, 0, 0)]
    [DataRow("rgb(255.4 0 0)", 255, 0, 0)]
    public void ModernSyntax_TakesNumbersPercentagesAndFractions(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#color-syntax-legacy")]
    [DataRow("rgb(255, 0, 0)", 255, 0, 0, 255)]
    [DataRow("rgba(255, 0, 0, 0.5)", 255, 0, 0, 128)]
    [DataRow("rgba(255, 0, 0, 50%)", 255, 0, 0, 128)]
    [DataRow("hsl(120, 100%, 50%)", 0, 255, 0, 255)]
    [DataRow("hsla(120, 100%, 50%, 0.5)", 0, 255, 0, 128)]
    public void LegacySyntax_SeparatesTheComponentsWithCommas(string value, int r, int g, int b, int a)
    {
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#color-syntax-legacy")]
    [DataRow("hsl(120, 100%, 50%, 0.5)", 0, 255, 0, 128)]
    public void LegacyHsl_TakesAFourthValueAsAlpha(string value, int r, int g, int b, int a)
    {
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#color-syntax-legacy")]
    [DataRow("rgb(255, 0, 0, 0.5)", 255, 0, 0, 128)]
    [DataRow("rgb(127.6, 0, 0)", 128, 0, 0, 255)]
    public void LegacySyntax_TakesAnAlphaInRgbAndFractions(string value, int r, int g, int b, int a)
    {
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#color-syntax-legacy")]
    [DataRow("rgb(1, 2 3)")]
    [DataRow("rgb(255 0 0 0.5)")]
    [DataRow("hwb(120, 0%, 0%)")]
    [DataRow("hwb(120 0% 0% 0.5)")]
    [DataRow("lab(50, 40, 59.5)")]
    [DataRow("oklab(0.628, 0.225, 0.126)")]
    [DataRow("rgb(1 2)")]
    public void InvalidSyntax_IsIgnored(string value)
    {
        Assert.AreEqual(Green, Color($"green;color:{value}"));
    }

    [TestMethod]
    [Spec("css-color-4", "#alpha-syntax")]
    [DataRow("rgba(255, 0, 0, 0.5)", 128)]
    [DataRow("hwb(0 0% 0% / 0.5)", 128)]
    [DataRow("lab(50 40 59.5 / 50%)", 128)]
    [DataRow("oklch(62.8% 0.258 29.2 / 0.25)", 64)]
    [DataRow("lch(50 72 56 / 2)", 255)]
    public void Alpha_FollowsASlash(string value, int alpha)
    {
        Assert.AreEqual(alpha, Color(value).A);
    }

    [TestMethod]
    [Spec("css-color-4", "#alpha-syntax")]
    [DataRow("rgba(255 0 0 / 50%)", 128)]
    public void Alpha_AfterASlash_IsAppliedInRgba(string value, int alpha)
    {
        Assert.AreEqual(alpha, Color(value).A);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#alpha-syntax")]
    [DataRow("rgb(255 0 0 / 0.5)", 128)]
    [DataRow("lab(50 40 59.5 / -1)", 0)]
    public void Alpha_IsAppliedAndClampedInEveryFunction(string value, int alpha)
    {
        Assert.AreEqual(alpha, Color(value).A);
    }

    [TestMethod]
    [Spec("css-color-4", "#hue-syntax")]
    [DataRow("hsl(120, 100%, 50%)")]
    [DataRow("hsl(120deg, 100%, 50%)")]
    [DataRow("hsl(0.33333turn, 100%, 50%)")]
    [DataRow("hsl(480, 100%, 50%)")]
    [DataRow("hwb(120deg 0% 0%)")]
    [DataRow("lch(87.8 113.3 134.4)")]
    public void Hue_IsANumberOfDegreesOrAnAngle(string value)
    {
        AssertColor(RColor.FromArgb(255, 0, 255, 0), Color(value), 1);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#hue-syntax")]
    [DataRow("hwb(480 0% 0%)")]
    [DataRow("hwb(-240deg 0% 0%)")]
    public void Hue_OutsideOneTurn_WrapsAround(string value)
    {
        Assert.AreEqual(RColor.FromArgb(255, 0, 255, 0), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#missing")]
    [DataRow("lab(none 0 0)", 0, 0, 0)]
    [DataRow("lch(50 72 none)", 221, 34, 123)]
    [DataRow("hwb(none 0% 0%)", 255, 0, 0)]
    public void MissingComponent_IsZero(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#missing")]
    [DataRow("lab(50 40 59.5 / none)", 191, 87, 0, 0)]
    public void MissingAlpha_IsZero(string value, int r, int g, int b, int a)
    {
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#missing")]
    [DataRow("rgb(none 0 0)", 0, 0, 0, 255)]
    [DataRow("hsl(none 100% 50%)", 255, 0, 0, 255)]
    public void MissingComponent_IsZeroInRgbAndHsl(string value, int r, int g, int b, int a)
    {
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#powerless")]
    [DataRow("color-mix(in lch, white, blue)", 175, 137, 255)]
    [DataRow("color-mix(in hsl, white, blue)", 159, 159, 223)]
    public void PowerlessHue_IsMissingWhenInterpolating(string value, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color(value), 1);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    [DataRow("#f00", 255, 0, 0, 255)]
    [DataRow("#F00", 255, 0, 0, 255)]
    [DataRow("#ff0000", 255, 0, 0, 255)]
    [DataRow("#f000", 255, 0, 0, 0)]
    [DataRow("#ff000080", 255, 0, 0, 128)]
    public void HexColor_HasThreeFourSixOrEightDigits(string value, int r, int g, int b, int a)
    {
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), Color(value));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#hex-notation")]
    [DataRow("#f008")]
    [DataRow("#ff000088")]
    public void HexAlpha_KeepsItsPrecision(string value)
    {
        Assert.AreEqual(0x88, Color(value).A);
    }

    [TestMethod]
    [Spec("css-color-4", "#named-colors")]
    [Spec("css-color-4", "#untagged")]
    [DataRow("rebeccapurple", 102, 51, 153)]
    [DataRow("RebeccaPurple", 102, 51, 153)]
    [DataRow("aliceblue", 240, 248, 255)]
    [DataRow("darkgrey", 169, 169, 169)]
    [DataRow("darkgray", 169, 169, 169)]
    [DataRow("lightgoldenrodyellow", 250, 250, 210)]
    [DataRow("yellowgreen", 154, 205, 50)]
    public void NamedColor_IsItsSrgbValue(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#css-system-colors")]
    [DataRow("ButtonFace")]
    [DataRow("ButtonText")]
    [DataRow("GrayText")]
    [DataRow("Highlight")]
    [DataRow("HighlightText")]
    public void SystemColor_IsAColor(string value)
    {
        Assert.AreNotEqual(RColor.FromArgb(255, 1, 2, 3), Color($"rgb(1,2,3);color:{value}"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#css-system-colors")]
    [DataRow("AccentColor")]
    [DataRow("AccentColorText")]
    [DataRow("ActiveText")]
    [DataRow("ButtonBorder")]
    [DataRow("Canvas")]
    [DataRow("CanvasText")]
    [DataRow("Field")]
    [DataRow("FieldText")]
    [DataRow("LinkText")]
    [DataRow("Mark")]
    [DataRow("MarkText")]
    [DataRow("SelectedItem")]
    [DataRow("SelectedItemText")]
    [DataRow("VisitedText")]
    public void NewSystemColor_IsAColor(string value)
    {
        Assert.AreNotEqual(RColor.FromArgb(255, 1, 2, 3), Color($"rgb(1,2,3);color:{value}"));
    }

    [TestMethod]
    [Spec("css-color-4", "#transparent-color")]
    public void Transparent_IsTransparentBlack()
    {
        Assert.AreEqual(0, Color("transparent").A);
        Assert.AreEqual(0, Styled("#x{background-color:red;background-color:transparent}").ActualBackgroundColor.A);
    }

    [TestMethod]
    [Spec("css-color-4", "#currentcolor-color")]
    [DataRow("currentcolor")]
    [DataRow("currentColor")]
    public void CurrentColor_InColor_IsTheInheritedColor(string value)
    {
        var box = Styled($"#x{{color:{value}}}", "<div style='color:rgb(1,2,3)'><div id=x>a</div></div>");

        Assert.AreEqual(RColor.FromArgb(255, 1, 2, 3), box.ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#currentcolor-color")]
    public void CurrentColor_InOtherProperties_IsTheColor()
    {
        var box = Styled("#x{color:rgb(1,2,3);background-color:currentcolor;border-left:1px solid currentcolor}");

        Assert.AreEqual(RColor.FromArgb(255, 1, 2, 3), box.ActualBackgroundColor);
        Assert.AreEqual(RColor.FromArgb(255, 1, 2, 3), box.ActualBorderLeftColor);
    }

    [TestMethod]
    [Spec("css-color-4", "#rgb-to-hsl")]
    [Spec("css-color-4", "#rgb-to-hwb")]
    [DataRow("color-mix(in hsl, red, lime)", 255, 255, 0)]
    [DataRow("color-mix(in hwb, red, lime)", 255, 255, 0)]
    [DataRow("color-mix(in hsl, lime, blue)", 0, 255, 255)]
    public void SrgbColor_IsConvertedToHslAndHwb(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#cie-lab")]
    [Spec("css-color-4", "#specifying-lab-lch")]
    [Spec("css-color-4", "#color-convert-algorithm")]
    [DataRow("lab(50% 40 59.5)", 191, 87, 0)]
    [DataRow("lab(29.2345 39.3825 20.0664)", 125, 35, 41)]
    [DataRow("lab(100 0 0)", 255, 255, 255)]
    [DataRow("lab(150 0 0)", 255, 255, 255)]
    [DataRow("lab(-10 0 0)", 0, 0, 0)]
    [DataRow("lch(50% 72 56)", 191, 87, 0)]
    public void LabColor_IsConvertedToSrgb(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#ok-lab")]
    [Spec("css-color-4", "#specifying-oklab-oklch")]
    [DataRow("oklab(62.8% 0.225 0.126)", 255, 0, 0)]
    [DataRow("oklch(0.628 0.258 29.2)", 255, 0, 0)]
    [DataRow("oklch(1 0 0)", 255, 255, 255)]
    [DataRow("oklch(0 0 0)", 0, 0, 0)]
    public void OklabColor_IsConvertedToSrgb(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    [TestMethod]
    [Spec("css-color-4", "#lab-to-lch")]
    [Spec("css-color-4", "#lch-to-lab")]
    [DataRow("lab", "lch")]
    [DataRow("oklab", "oklch")]
    public void PolarForm_IsTheSameColor(string rectangular, string polar)
    {
        Assert.AreEqual(Color($"color-mix(in {rectangular}, lab(50 40 59.5), lab(50 40 59.5))"),
            Color($"color-mix(in {polar}, lab(50 40 59.5), lab(50 40 59.5))"));
    }

    [TestMethod]
    [Spec("css-color-4", "#specifying-lab-lch")]
    public void NegativeChroma_IsClampedToZero()
    {
        Assert.AreEqual(Color("lch(50 0 0)"), Color("lch(50 -10 0)"));
    }

    [TestMethod]
    [Spec("css-color-4", "#color-function")]
    [Spec("css-color-4", "#predefined-sRGB")]
    [Spec("css-color-4", "#predefined-sRGB-linear")]
    [Spec("css-color-4", "#predefined-display-p3")]
    [Spec("css-color-4", "#predefined-xyz")]
    [DataRow("color(srgb 1 0 0)", 255, 0, 0)]
    [DataRow("color(srgb 100% 0% 0%)", 255, 0, 0)]
    [DataRow("color(srgb-linear 1 0 0)", 255, 0, 0)]
    [DataRow("color(display-p3 1 0 0)", 255, 0, 0)]
    [DataRow("color(xyz-d65 0.4124 0.2126 0.0193)", 255, 0, 0)]
    public void ColorFunction_IsConvertedToSrgb(string value, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color($"black;color:{value}"), 1);
    }

    [TestMethod]
    [Spec("css-color-4", "#predefined-sRGB")]
    [Spec("css-color-4", "#predefined-sRGB-linear")]
    [Spec("css-color-4", "#predefined-display-p3")]
    [Spec("css-color-4", "#predefined-xyz")]
    [Spec("css-color-4", "#predefined-to-predefined")]
    [Spec("css-color-4", "#predefined-to-lab-oklab")]
    [Spec("css-color-4", "#oklab-lab-to-predefined")]
    [Spec("css-color-4", "#interpolation-space")]
    [Spec("css-color-4", "#color-convert-algorithm")]
    [DataRow("srgb", 128, 0, 128)]
    [DataRow("srgb-linear", 188, 0, 188)]
    [DataRow("display-p3", 128, 10, 145)]
    [DataRow("xyz", 188, 0, 188)]
    [DataRow("xyz-d65", 188, 0, 188)]
    [DataRow("xyz-d50", 187, 0, 188)]
    [DataRow("lab", 193, 0, 136)]
    [DataRow("oklab", 140, 83, 162)]
    public void ColorSpace_IsUsedToInterpolate(string space, int r, int g, int b)
    {
        AssertColor(RColor.FromArgb(255, r, g, b), Color($"color-mix(in {space}, red, blue)"), 1);
    }

    [TestMethod]
    [Spec("css-color-4", "#predefined-a98-rgb")]
    [Spec("css-color-4", "#predefined-prophoto-rgb")]
    [Spec("css-color-4", "#predefined-rec2020")]
    [Spec("css-color-4", "#interpolation-space")]
    [DataRow("rec2020", 197, 160, 16)]
    [DataRow("prophoto-rgb", 0, 0, 0)]
    [DataRow("a98-rgb", 0, 0, 0)]
    public void WideGamutColorSpace_IsUsedToInterpolate(string space, int r, int g, int b)
    {
        var mixed = Color($"color-mix(in {space}, red, lime)");

        Assert.AreNotEqual(RColor.FromArgb(255, 128, 128, 0), mixed);
        if (r + g + b > 0) AssertColor(RColor.FromArgb(255, r, g, b), mixed, 1);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#predefined-display-p3-linear")]
    [Spec("css-color-4", "#interpolation-space")]
    public void DisplayP3Linear_IsUsedToInterpolate()
    {
        Assert.AreNotEqual(RColor.FromArgb(255, 128, 128, 0), Color("color-mix(in display-p3-linear, red, lime)"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-color-4.yml")]
    [Spec("css-color-4", "#css-gamut-mapping")]
    [Spec("css-color-4", "#GMA-Binary-local-MINDE")]
    [Spec("css-color-4", "#color-convert-algorithm")]
    public void OutOfGamutColor_IsMappedByReducingChroma()
    {
        AssertColor(RColor.FromArgb(255, 0, 195, 0), Color("oklch(0.7 0.4 145)"), 2);
    }

    [TestMethod]
    [Spec("css-color-4", "#component-wise-linear-interpolation")]
    [DataRow("color-mix(in srgb, red, blue)", 128, 0, 128, 255)]
    [DataRow("color-mix(in srgb, red 25%, blue)", 64, 0, 191, 255)]
    [DataRow("color-mix(in srgb, black, white)", 128, 128, 128, 255)]
    public void Components_AreInterpolatedLinearly(string value, int r, int g, int b, int a)
    {
        AssertColor(RColor.FromArgb(a, r, g, b), Color(value), 1);
    }

    [TestMethod]
    [Spec("css-color-4", "#interpolation-missing")]
    [DataRow("color-mix(in lch, lch(50 72 none), lch(50 72 120))", "lch(50 72 120)")]
    [DataRow("color-mix(in hsl, hsl(none 50% 50%), hsl(120, 50%, 50%))", "hsl(120, 50%, 50%)")]
    public void MissingComponent_TakesTheOtherColorsValue(string value, string expected)
    {
        AssertColor(Color(expected), Color(value), 1);
    }

    [TestMethod]
    [Spec("css-color-4", "#interpolation-alpha")]
    [DataRow("color-mix(in srgb, red, transparent)", 255, 0, 0, 128)]
    [DataRow("color-mix(in srgb, rgba(255, 0, 0, 0.5), blue)", 85, 0, 170, 191)]
    public void Alpha_IsPremultiplied(string value, int r, int g, int b, int a)
    {
        AssertColor(RColor.FromArgb(a, r, g, b), Color(value), 1);
    }

    [TestMethod]
    [Spec("css-color-4", "#hue-interpolation")]
    [Spec("css-color-4", "#hue-shorter")]
    [Spec("css-color-4", "#hue-longer")]
    [Spec("css-color-4", "#hue-increasing")]
    [Spec("css-color-4", "#hue-decreasing")]
    [DataRow("", 0, 255, 255)]
    [DataRow("shorter hue", 0, 255, 255)]
    [DataRow("longer hue", 255, 0, 0)]
    [DataRow("increasing hue", 0, 255, 255)]
    [DataRow("decreasing hue", 255, 0, 0)]
    public void Hue_IsInterpolatedInTheGivenDirection(string method, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color($"color-mix(in hsl {method}, lime, blue)"));
    }

    [TestMethod]
    [Spec("css-color-4", "#hue-interpolation")]
    [DataRow("color-mix(in hsl, red, blue)", 255, 0, 255)]
    [DataRow("color-mix(in hwb decreasing hue, red, blue)", 255, 0, 255)]
    public void InterpolatedHue_BelowZeroDegrees_WrapsAround(string value, int r, int g, int b)
    {
        Assert.AreEqual(RColor.FromArgb(255, r, g, b), Color(value));
    }

    private static RColor Color(string value) => Styled($"#x{{color:{value}}}").ActualColor;

    private static void AssertColor(RColor expected, RColor actual, int tolerance)
    {
        var message = $"Expected {expected}, got {actual}";
        Assert.AreEqual((double)expected.R, actual.R, tolerance, message);
        Assert.AreEqual((double)expected.G, actual.G, tolerance, message);
        Assert.AreEqual((double)expected.B, actual.B, tolerance, message);
        Assert.AreEqual((double)expected.A, actual.A, tolerance, message);
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>");
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }
}
