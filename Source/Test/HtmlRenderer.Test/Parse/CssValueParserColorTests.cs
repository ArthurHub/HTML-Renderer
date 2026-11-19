using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Parse;

/// <summary>
/// <see cref="CssValueParser.TryGetColor"/> on raw values. The cascade hands it serialized colors, HTML attributes
/// such as <c>&lt;font color&gt;</c> and <c>bgcolor</c> hand it what the author wrote, which HTML parses with its legacy
/// color rules instead, see <see cref="Html.CommonMicrosyntaxesTests"/>.
/// </summary>
[TestClass]
public sealed class CssValueParserColorTests
{
    private static readonly CssValueParser Parser = new(new MockAdapter());

    private static (bool Valid, RColor Color) Parse(string value)
    {
        var valid = Parser.TryGetColor(value, 0, value.Length, out var color);
        return (valid, color);
    }

    private static void AssertColor(string value, int a, int r, int g, int b)
    {
        var (valid, color) = Parse(value);
        Assert.IsTrue(valid, value);
        Assert.AreEqual(RColor.FromArgb(a, r, g, b), color, value);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    [DataRow("#abc", 0xaa, 0xbb, 0xcc)]
    [DataRow("#A1f", 0xaa, 0x11, 0xff)]
    [DataRow("#9F9F9F", 0x9f, 0x9f, 0x9f)]
    [DataRow("#A9A9A9", 0xa9, 0xa9, 0xa9)]
    [DataRow("#9A9A9A", 0x9a, 0x9a, 0x9a)]
    [DataRow("#9a9a9a", 0x9a, 0x9a, 0x9a)]
    [DataRow("#102030", 0x10, 0x20, 0x30)]
    public void Hex_ThreeAndSixDigits_AreOpaqueRgb(string value, int r, int g, int b)
    {
        AssertColor(value, 255, r, g, b);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    public void Hex_FourDigits_LastDigitIsAlpha()
    {
        AssertColor("#abcd", 0xdd, 0xaa, 0xbb, 0xcc);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    [DataRow("#")]
    [DataRow("#1")]
    [DataRow("#12345")]
    [DataRow("#ab:")]
    [DataRow("#ab@")]
    [DataRow("#ab`")]
    public void Hex_WrongLengthOrDigit_IsInvalid(string value)
    {
        Assert.IsFalse(Parse(value).Valid);
    }

    [TestMethod]
    [Spec("css-color-4", "#rgb-functions")]
    [DataRow("rgb(1,2,3)", 1, 2, 3)]
    [DataRow("rgb(10, 20, 30)", 10, 20, 30)]
    [DataRow("rgb( 255 , 128 , 0 )", 255, 128, 0)]
    [DataRow("rgb(100%, 0%, 0%)", 255, 0, 0)]
    public void Rgb_LegacySyntax_IsOpaque(string value, int r, int g, int b)
    {
        AssertColor(value, 255, r, g, b);
    }

    [TestMethod]
    [Spec("css-color-4", "#rgb-functions")]
    [Spec("css-color-4", "#alpha-syntax")]
    [DataRow("rgba(10,20,30,0.5)", 128)]
    [DataRow("rgba(10, 20, 30, .25)", 64)]
    [DataRow("rgba(10, 20, 30, 0.2)", 51)]
    [DataRow("rgba(10,20,30,50%)", 128)]
    [DataRow("rgba(10, 20, 30, 25%)", 64)]
    [DataRow("rgba(10,20,30,0)", 0)]
    [DataRow("rgba(10,20,30,1)", 255)]
    [DataRow("rgba(10,20,30,2)", 255)]
    [DataRow("rgba(10,20,30,150%)", 255)]
    public void Rgba_Alpha_IsANumberOrPercentageClampedToOne(string value, int a)
    {
        AssertColor(value, a, 10, 20, 30);
    }

    [TestMethod]
    [Spec("css-color-4", "#rgb-functions")]
    [DataRow("rgb(1,2)")]
    [DataRow("rgb(a,b,c)")]
    [DataRow("rgb(1234567)")]
    public void Rgb_MissingOrNonNumericComponent_IsInvalid(string value)
    {
        Assert.IsFalse(Parse(value).Valid);
    }

    [TestMethod]
    [Spec("css-color-4", "#hsl-to-rgb")]
    [Spec("css-color-4", "#hue-syntax")]
    [DataRow("hsl(0,100%,50%)", 255, 255, 0, 0)]
    [DataRow("hsl(120, 100%, 25%)", 255, 0, 128, 0)]
    [DataRow("hsl(-120,100%,50%)", 255, 0, 0, 255)]
    [DataRow("hsl(180deg,100%,50%)", 255, 0, 255, 255)]
    [DataRow("hsl(0.5turn,100%,50%)", 255, 0, 255, 255)]
    [DataRow("hsl(200grad,100%,50%)", 255, 0, 255, 255)]
    [DataRow("hsl(3.14159265rad,100%,50%)", 255, 0, 255, 255)]
    [DataRow("hsla(240, 100%, 50%, 0.4)", 102, 0, 0, 255)]
    [DataRow("hsla(0,100%,50%,50%)", 128, 255, 0, 0)]
    public void Hsl_LegacySyntax_ConvertsToSrgb(string value, int a, int r, int g, int b)
    {
        AssertColor(value, a, r, g, b);
    }

    [TestMethod]
    [Spec("css-color-4", "#named-colors")]
    [DataRow("red", 255, 255, 0, 0)]
    [DataRow("transparent", 0, 0, 0, 0)]
    public void Keyword_IsValid(string value, int a, int r, int g, int b)
    {
        AssertColor(value, a, r, g, b);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("foo")]
    [DataRow("light-dark(red)")]
    public void UnknownOrEmpty_IsInvalid(string value)
    {
        Assert.IsFalse(Parse(value).Valid);
    }

    [TestMethod]
    [Spec("css-color-5", "#light-dark")]
    [DataRow("light-dark(red, blue)", 255, 0, 0)]
    [DataRow("light-dark(#00f, red)", 0, 0, 255)]
    [DataRow("light-dark(rgb(0, 128, 0), red)", 0, 128, 0)]
    public void LightDark_ResolvesToTheLightColor(string value, int r, int g, int b)
    {
        AssertColor(value, 255, r, g, b);
    }
}
