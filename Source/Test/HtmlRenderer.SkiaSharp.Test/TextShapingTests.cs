using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters;

namespace HtmlRenderer.SkiaSharp.Test;

[TestClass]
public sealed class TextShapingTests
{
    [TestMethod]
    public void Measure_AppliesKerning()
    {
        using var typeface = SKTypeface.FromData(SKData.CreateCopy(Resources.GetFontBytes("LiberationSans-Regular.ttf")));
        var font = new FontAdapter(typeface, SKFontStyle.Normal, 30);

        var kerned = font.MeasureText("AVAVAV");
        var separate = 3 * font.MeasureText("A") + 3 * font.MeasureText("V");

        Assert.IsLessThan(separate - 1, kerned);
    }

    [TestMethod]
    public void Measure_JoinsArabicLetters()
    {
        const string beh = "ب";
        using var typeface = SKFontManager.Default.MatchCharacter(beh[0]);
        if (typeface == null)
            Assert.Inconclusive("No installed font has Arabic glyphs.");

        var font = new FontAdapter(typeface, SKFontStyle.Normal, 30);

        // The initial and medial forms are narrower than three isolated letters
        Assert.IsLessThan(3 * font.MeasureText(beh) - 1, font.MeasureText(beh + beh + beh));
    }

    [TestMethod]
    public void Measure_TreatsATabAsASpace()
    {
        using var typeface = SKTypeface.FromData(SKData.CreateCopy(Resources.GetFontBytes("LiberationSans-Regular.ttf")));
        var font = new FontAdapter(typeface, SKFontStyle.Normal, 30);

        Assert.AreEqual(font.MeasureText("a b"), font.MeasureText("a\tb"), 0.01);
    }

    [TestMethod]
    public void BreakText_CountsTheCharactersThatFit()
    {
        using var typeface = SKTypeface.FromData(SKData.CreateCopy(Resources.GetFontBytes("LiberationMono-Regular.ttf")));
        var font = new FontAdapter(typeface, SKFontStyle.Normal, 30);
        var charWidth = font.MeasureText("a");

        var fit = font.BreakText("abcdefgh", charWidth * 3.5f, out var width);

        Assert.AreEqual(3, fit);
        Assert.AreEqual(charWidth * 3, width, 0.01);
    }
}
