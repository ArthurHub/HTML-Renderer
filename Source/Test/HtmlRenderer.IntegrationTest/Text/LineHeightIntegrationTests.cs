using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class LineHeightIntegrationTests
{
    private const double Delta = 0.5;

    private static readonly string Image = TestImages.Png(10, 10);

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 400).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;

    private static double Height(CssBox box) => box.ActualBottom - box.Location.Y;

    private static CssRect FirstWord(CssBox box) => LayoutHarness.Descendants(box).SelectMany(b => b.Words).First();

    [TestMethod]
    [Spec("css21", "visudet.html#line-height")]
    [Spec("css-inline-3", "#line-layout")]
    [Spec("css-inline-3", "#inline-height")]
    public void TallerInlineText_MakesTheLineTaller()
    {
        var root = Layout("<div id='p'>x<span id='s' style='font-size:60px'>T</span></div>");

        Assert.AreEqual(Box(root, "s").ActualFont.Height, Height(Box(root, "p")), Delta);
    }

    // The image sits on the baseline, so the text's descent hangs below it.
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#line-height")]
    [Spec("css-inline-3", "#line-layout")]
    public void TallImage_MakesTheLineTaller()
    {
        var plain = Layout("<div id='p'>x</div>");
        var plainP = Box(plain, "p");
        var descent = Height(plainP) - (FirstWord(plainP).Top - plainP.Location.Y) - plainP.ActualFont.UnderlineOffset;
        var root = TestImages.Layout($"<div id='p'>x<img style='width:10px;height:100px' src='{Image}'>y</div>");

        Assert.AreEqual(100 + descent, Height(Box(root, "p")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#line-height")]
    [Spec("css-inline-3", "#line-layout")]
    public void InlineBlockVerticalPadding_MakesTheLineTaller()
    {
        var plain = Layout("<div id='p'>a<div style='display:inline-block'>x</div></div>");
        var padded = Layout("<div id='p'>a<div style='display:inline-block;padding:30px 0'>x</div></div>");

        Assert.AreEqual(Height(Box(plain, "p")) + 60, Height(Box(padded, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#line-height")]
    [Spec("css-inline-3", "#line-layout")]
    public void InlineBlockVerticalMargins_MakeTheLineTaller()
    {
        var plain = Layout("<div id='p'>a<div style='display:inline-block'>x</div></div>");
        var withMargins = Layout("<div id='p'>a<div style='display:inline-block;margin:30px 0'>x</div></div>");

        Assert.AreEqual(Height(Box(plain, "p")) + 60, Height(Box(withMargins, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#line-height")]
    [Spec("css-inline-3", "#inline-height")]
    public void InlineLineHeight_MakesTheLineTaller()
    {
        var root = Layout("<div id='p'>x<span style='line-height:60px'>y</span></div>");

        Assert.AreEqual(60, Height(Box(root, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#line-height")]
    [Spec("css-writing-modes-4", "#replaced-baselines")]
    [Spec("css-inline-3", "#baseline-tables")]
    public void Image_SitsOnTheBaseline()
    {
        var root = TestImages.Layout($"<div id='p' style='font-size:20px'><span id='t'>x</span><img id='i' src='{Image}'></div>");
        var word = FirstWord(Box(root, "t"));
        var baseline = word.Top + word.OwnerBox.ActualFont.UnderlineOffset;

        Assert.AreEqual(baseline, TestImages.ImageRect(root, "i").Bottom, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#line-height-property")]
    public void LineHeightLargerThanTheFont_MakesTheLineTaller()
    {
        var root = Layout("<div id='p' style='line-height:40px'>x</div>");
        var p = Box(root, "p");

        Assert.IsGreaterThan(p.ActualFont.Height, Height(p));
    }

    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#line-height-property")]
    public void NumberLineHeight_ScalesWithTheFontSize()
    {
        var small = Layout("<div id='p' style='font-size:20px;line-height:2'>x</div>");
        var large = Layout("<div id='p' style='font-size:40px;line-height:2'>x</div>");

        Assert.AreEqual(2 * Height(Box(small, "p")), Height(Box(large, "p")), 1);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#line-height-property")]
    public void EveryLine_GetsTheLineHeight()
    {
        var one = Layout("<div id='p' style='line-height:40px'>x</div>");
        var two = Layout("<div id='p' style='line-height:40px'>x<br>y</div>");

        Assert.AreEqual(2 * Height(Box(one, "p")), Height(Box(two, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#line-height-property")]
    public void LengthLineHeight_IsTheHeightOfTheLine()
    {
        var root = Layout("<div id='p' style='line-height:40px'>x</div>");

        Assert.AreEqual(40, Height(Box(root, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#inline-height")]
    public void LineHeightSmallerThanTheFont_MakesTheLineShorter()
    {
        var root = Layout("<div id='p' style='line-height:5px'>x</div>");

        Assert.AreEqual(5, Height(Box(root, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#inline-height")]
    public void HalfLeading_CentersTheTextInTheLine()
    {
        var root = Layout("<div id='p' style='line-height:60px'>x</div>");
        var p = Box(root, "p");

        Assert.AreEqual((60 - p.ActualFont.Height) / 2, FirstWord(p).Top - p.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#line-height-property")]
    public void PercentageLineHeight_ResolvesAgainstTheFontSize()
    {
        var root = Layout("<div id='p' style='font-size:20px;line-height:300%'>x</div>");

        Assert.AreEqual(60, Height(Box(root, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#leading")]
    [Spec("css-inline-3", "#line-height-property")]
    public void NumberLineHeight_IsInheritedAsTheNumber()
    {
        var root = Layout("<div style='font-size:10px;line-height:3'><div id='p' style='font-size:20px'>x</div></div>");

        Assert.AreEqual(60, Height(Box(root, "p")), Delta);
    }
}
