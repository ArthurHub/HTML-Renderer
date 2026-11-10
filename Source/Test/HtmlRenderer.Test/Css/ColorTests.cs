using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ColorTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var parser = new StylesheetParser();
        parser.Parse("body .class #item:hover{ background-color: red;}");
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    public void ColorInvalidHexDigitString()
    {
        var color = "BCDEFG";
        var result = Color.TryFromHex(color, out _);
        Assert.IsFalse(result);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    public void ColorValidFourLetterString()
    {
        var color = "abcd";
        var result = Color.TryFromHex(color, out Color hc);
        Assert.AreEqual(new Color(170, 187, 204, 221), hc);
        Assert.IsTrue(result);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    public void ColorInvalidLengthString()
    {
        var color = "abcde";
        var result = Color.TryFromHex(color, out _);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ColorValidLengthShortString()
    {
        var color = "fff";
        var result = Color.TryFromHex(color, out _);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ColorValidLengthLongString()
    {
        var color = "fffabc";
        var result = Color.TryFromHex(color, out _);
        Assert.IsTrue(result);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    public void ColorWhiteShortString()
    {
        var color = "fff";
        var result = Color.FromHex(color);
        Assert.AreEqual(Color.FromRgb(255, 255, 255), result);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    public void ColorRedShortString()
    {
        var color = "f00";
        var result = Color.FromHex(color);
        Assert.AreEqual(Color.FromRgb(255, 0, 0), result);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    public void ColorFromRedName()
    {
        var color = "red";
        var result = Color.FromName(color);
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(Color.Red, result);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#color-units")]
    public void ColorFromWhiteName()
    {
        var color = "white";
        var result = Color.FromName(color);
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(Color.White, result);
    }

    [TestMethod]
    public void ColorFromUnknownName()
    {
        var color = "bla";
        var result = Color.FromName(color);
        Assert.IsFalse(result.HasValue);
    }

    [TestMethod]
    public void ColorMixedLongString()
    {
        var color = "facc36";
        var result = Color.FromHex(color);
        Assert.AreEqual(Color.FromRgb(250, 204, 54), result);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    public void ColorMixedEightDigitLongStringTransparent()
    {
        var color = "facc3600";
        var result = Color.FromHex(color);
        Assert.AreEqual(Color.FromRgba(250, 204, 54, 0), result);
    }

    [TestMethod]
    [Spec("css-color-4", "#hex-notation")]
    public void ColorMixedEightDigitLongStringOpaque()
    {
        var color = "facc36ff";
        var result = Color.FromHex(color);
        Assert.AreEqual(Color.FromRgba(250, 204, 54, 1), result);
    }

    [TestMethod]
    public void ColorMixBlackOnWhite50Percent()
    {
        var color1 = Color.Black;
        var color2 = Color.White;
        var mix = Color.Mix(0.5, color1, color2);
        Assert.AreEqual(Color.FromRgb(127, 127, 127), mix);
    }

    [TestMethod]
    public void ColorMixRedOnWhite75Percent()
    {
        var color1 = Color.Red;
        var color2 = Color.White;
        var mix = Color.Mix(0.75, color1, color2);
        Assert.AreEqual(Color.FromRgb(255, 63, 63), mix);
    }

    [TestMethod]
    public void ColorMixBlueOnWhite10Percent()
    {
        var color1 = Color.Blue;
        var color2 = Color.White;
        var mix = Color.Mix(0.1, color1, color2);
        Assert.AreEqual(Color.FromRgb(229, 229, 255), mix);
    }

    [TestMethod]
    public void ColorMixGreenOnRed30Percent()
    {
        var color1 = Color.PureGreen;
        var color2 = Color.Red;
        var mix = Color.Mix(0.3, color1, color2);
        Assert.AreEqual(Color.FromRgb(178, 76, 0), mix);
    }

    [TestMethod]
    public void ColorMixWhiteOnBlack20Percent()
    {
        var color1 = Color.White;
        var color2 = Color.Black;
        var mix = Color.Mix(0.2, color1, color2);
        Assert.AreEqual(Color.FromRgb(51, 51, 51), mix);
    }

    [TestMethod]
    public void ColorHslBlackMixed()
    {
        var color = Color.FromHsl(0, 1, 0);
        Assert.AreEqual(Color.Black, color);
    }

    [TestMethod]
    public void ColorHslBlackMixed1()
    {
        var color = Color.FromHsl(0, 1, 0);
        Assert.AreEqual(Color.Black, color);
    }

    [TestMethod]
    public void ColorHslBlackMixed2()
    {
        var color = Color.FromHsl(0.5f, 1, 0);
        Assert.AreEqual(Color.Black, color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hsl-to-rgb")]
    public void ColorHslRedPure()
    {
        var color = Color.FromHsl(0, 1, 0.5f);
        Assert.AreEqual(Color.Red, color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hsl-to-rgb")]
    public void ColorHslGreenPure()
    {
        var color = Color.FromHsl(1f / 3f, 1, 0.5f);
        Assert.AreEqual(Color.PureGreen, color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hsl-to-rgb")]
    public void ColorHslBluePure()
    {
        var color = Color.FromHsl(2f / 3f, 1, 0.5f);
        Assert.AreEqual(Color.Blue, color);
    }

    [TestMethod]
    public void ColorHslBlackPure()
    {
        var color = Color.FromHsl(0, 0, 0);
        Assert.AreEqual(Color.Black, color);
    }

    [TestMethod]
    public void ColorHslMagentaPure()
    {
        var color = Color.FromHsl(300f / 360f, 1, 0.5f);
        Assert.AreEqual(Color.Magenta, color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hsl-to-rgb")]
    public void ColorHslYellowGreenMixed()
    {
        var color = Color.FromHsl(1f / 4f, 0.75f, 0.63f);
        Assert.AreEqual(Color.FromRgb(161, 231, 90), color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hsl-to-rgb")]
    public void ColorHslGrayBlueMixed()
    {
        var color = Color.FromHsl(210f / 360f, 0.25f, 0.25f);
        Assert.AreEqual(Color.FromRgb(48, 64, 80), color);
    }

    [TestMethod]
    public void ColorFlexHexOneLetter()
    {
        var color = Color.FromFlexHex("F");
        Assert.AreEqual(Color.FromRgb(0xf, 0x0, 0x0), color);
    }

    [TestMethod]
    public void ColorFlexHexTwoLetters()
    {
        var color = Color.FromFlexHex("0F");
        Assert.AreEqual(Color.FromRgb(0x0, 0xf, 0x0), color);
    }

    [TestMethod]
    public void ColorFlexHexFourLetters()
    {
        var color = Color.FromFlexHex("0F0F");
        Assert.AreEqual(Color.FromRgb(0xf, 0xf, 0x0), color);
    }

    [TestMethod]
    public void ColorFlexHexSevenLetters()
    {
        var color = Color.FromFlexHex("0F0F0F0");
        Assert.AreEqual(Color.FromRgb(0xf, 0xf0, 0x0), color);
    }

    [TestMethod]
    public void ColorFlexHexFifteenLetters()
    {
        var color = Color.FromFlexHex("1234567890ABCDE");
        Assert.AreEqual(Color.FromRgb(0x12, 0x67, 0xab), color);
    }

    [TestMethod]
    public void ColorFlexHexExtremelyLong()
    {
        var color = Color.FromFlexHex("1234567890ABCDE1234567890ABCDE");
        Assert.AreEqual(Color.FromRgb(0x34, 0xcd, 0x89), color);
    }

    [TestMethod]
    public void ColorFlexHexRandomString()
    {
        var color = Color.FromFlexHex("6db6ec49efd278cd0bc92d1e5e072d68");
        Assert.AreEqual(Color.FromRgb(0x6e, 0xcd, 0xe0), color);
    }

    [TestMethod]
    public void ColorFlexHexSixLettersInvalid()
    {
        var color = Color.FromFlexHex("zqbttv");
        Assert.AreEqual(Color.FromRgb(0x0, 0xb0, 0x0), color);
    }

    [TestMethod]
    public void ColorFromGraySimple()
    {
        var color = Color.FromGray(25);
        Assert.AreEqual(Color.FromRgb(25, 25, 25), color);
    }

    [TestMethod]
    public void ColorFromGrayWithAlpha()
    {
        var color = Color.FromGray(25, 0.5f);
        Assert.AreEqual(Color.FromRgba(25, 25, 25, 0.5f), color);
    }

    [TestMethod]
    public void ColorFromGrayPercent()
    {
        var color = Color.FromGray(0.5f, 0.5f);
        Assert.AreEqual(Color.FromRgba(128, 128, 128, 0.5f), color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hwb-to-rgb")]
    public void ColorFromHwbRed()
    {
        var color = Color.FromHwb(0f, 0.2f, 0.2f);
        Assert.AreEqual(Color.FromRgb(204, 51, 51), color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hwb-to-rgb")]
    public void ColorFromHwbGreen()
    {
        var color = Color.FromHwb(1f / 3f, 0.2f, 0.6f);
        Assert.AreEqual(Color.FromRgb(51, 102, 51), color);
    }

    [TestMethod]
    [Spec("css-color-4", "#hwb-to-rgb")]
    public void ColorFromHwbMagentaTransparent()
    {
        var color = Color.FromHwba(5f / 6f, 0.4f, 0.2f, 0.5f);
        Assert.AreEqual(Color.FromRgba(204, 102, 204, 0.5f), color);
    }
}
