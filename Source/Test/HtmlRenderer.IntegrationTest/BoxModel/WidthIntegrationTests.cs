using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class WidthIntegrationTests
{
    private const double Delta = 0.5;

    private static readonly string Image = TestImages.Png(40, 20);

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 400).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;

    [TestMethod]
    [Spec("css21", "visudet.html#the-width-property")]
    public void LengthWidth_IsTheContentWidth()
    {
        var root = Layout("<div id='c' style='width:100px;padding:0 10px;border:0 solid;border-width:0 5px;height:5px'></div>");

        Assert.AreEqual(130, Box(root, "c").Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#the-width-property")]
    public void PercentageWidth_ResolvesAgainstTheContainingBlockWidth()
    {
        var root = Layout("<div style='width:200px;padding:0 50px'><div id='c' style='width:50%;padding:0 5px;height:5px'></div></div>");
        var c = Box(root, "c");

        Assert.AreEqual(50, c.Location.X, Delta);
        Assert.AreEqual(110, c.Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#the-width-property")]
    public void NegativeWidth_IsIgnored()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='width:-50px;height:5px'></div></div>");

        Assert.AreEqual(200, Box(root, "c").Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-width")]
    public void InlineAutoMargins_AreZero()
    {
        var root = Layout("<div>a<span id='s' style='margin-left:auto;margin-right:auto'>x</span>b</div>");
        var a = LayoutHarness.FindById(root, "s")!.ParentBox!.Boxes[0].Words[0];

        Assert.AreEqual(a.Right, FirstWord(Box(root, "s"))!.Left, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#inline-width")]
    public void WidthOnAnInline_DoesNotApply()
    {
        var root = Layout("<div>a<span id='s' style='width:200px'>x</span><span id='t'>b</span></div>");

        Assert.AreEqual(FirstWord(Box(root, "s"))!.Right, FirstWord(Box(root, "t"))!.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-width")]
    public void AutoWidthAndHeight_UseTheIntrinsicSize()
    {
        var root = TestImages.Layout($"<img id='i' src='{Image}'>");
        var r = TestImages.ImageRect(root, "i");

        Assert.AreEqual((40d, 20d), (r.Width, r.Height));
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-width")]
    public void AutoWidth_FollowsTheIntrinsicRatioOfTheHeight()
    {
        var root = TestImages.Layout($"<img id='i' style='height:40px' src='{Image}'>");

        Assert.AreEqual(80, TestImages.ImageRect(root, "i").Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-width")]
    public void PercentageWidth_ResolvesAgainstTheContainingBlock()
    {
        var root = TestImages.Layout($"<div style='width:200px'><img id='i' style='width:50%' src='{Image}'></div>");

        Assert.AreEqual(100, TestImages.ImageRect(root, "i").Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-width")]
    public void EmWidth_IsApplied()
    {
        var root = TestImages.Layout($"<div style='font-size:10px'><img id='i' style='width:8em' src='{Image}'></div>");

        Assert.AreEqual(80, TestImages.ImageRect(root, "i").Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void AutoWidth_FillsTheContainingBlockMinusTheMargins()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='margin-left:10px;margin-right:20px;height:5px'></div></div>");
        var c = Box(root, "c");

        Assert.AreEqual(10, c.Location.X, Delta);
        Assert.AreEqual(170, c.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void OverConstrained_IgnoresMarginRight()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='width:100px;margin:0 10px;height:5px'></div></div>");
        var c = Box(root, "c");

        Assert.AreEqual(10, c.Location.X, Delta);
        Assert.AreEqual(100, c.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void AutoWidth_WithAutoMargins_FillsAndTheMarginsAreZero()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='margin:0 auto;height:5px'></div></div>");
        var c = Box(root, "c");

        Assert.AreEqual(0, c.Location.X, Delta);
        Assert.AreEqual(200, c.Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void AutoMarginLeft_TakesTheRemainingSpace()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='width:100px;margin-left:auto;height:5px'></div></div>");

        Assert.AreEqual(100, Box(root, "c").Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#blockwidth")]
    public void OverConstrained_InRtl_IgnoresMarginLeft()
    {
        var root = Layout("<div style='width:200px;direction:rtl'><div id='c' style='width:100px;margin:0 10px;height:5px'></div></div>");

        Assert.AreEqual(90, Box(root, "c").Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#block-replaced-width")]
    public void BlockImage_KeepsItsIntrinsicWidth()
    {
        var root = TestImages.Layout($"<div style='width:200px'><img id='i' style='display:block' src='{Image}'><div id='n' style='height:5px'></div></div>");

        Assert.AreEqual(40, TestImages.ImageRect(root, "i").Width, Delta);
        Assert.AreEqual(20, Box(root, "n").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#block-replaced-width")]
    public void BlockImage_IsCenteredByAutoMargins()
    {
        var root = TestImages.Layout($"<div style='width:200px'><img id='i' style='display:block;margin:0 auto' src='{Image}'></div>");

        Assert.AreEqual(80, TestImages.ImageRect(root, "i").X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#float-width")]
    public void Float_WithAutoMargins_HasZeroMargins()
    {
        var root = Layout("<div style='width:400px'><div id='f' style='float:left;width:50px;margin-left:auto;margin-right:auto;height:5px'></div></div>");
        var f = Box(root, "f");

        Assert.AreEqual(0, f.Location.X, Delta);
        Assert.AreEqual(50, f.Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#float-width")]
    public void AutoWidthFloat_ShrinksToFitItsContent()
    {
        var root = Layout("<div id='f' style='float:left'>xx yy</div><div id='t'>zz</div>");
        var f = Box(root, "f");
        var words = LayoutHarness.Descendants(f).SelectMany(b => b.Words).ToList();

        Assert.AreEqual(words[^1].Right - words[0].Left, f.Size.Width, 1);
        Assert.AreEqual(words[0].Top, words[^1].Top, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#float-replaced-width")]
    public void FloatedImage_KeepsItsIntrinsicWidth()
    {
        var root = TestImages.Layout($"<div style='width:200px'><img id='i' style='float:left' src='{Image}'></div>");

        Assert.AreEqual(40, TestImages.ImageRect(root, "i").Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-width")]
    [Spec("css-position-3", "#abs-non-replaced-width")]
    public void Absolute_WithLeftAndWidth_IsPlacedAtLeft()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:10px;top:0;width:50px;height:10px'></div></div>");
        var a = Box(root, "a");

        Assert.AreEqual(10, a.Location.X, Delta);
        Assert.AreEqual(50, a.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-width")]
    [Spec("css-position-3", "#abs-non-replaced-width")]
    public void Absolute_WithRightAndWidth_IsPlacedAtRight()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;right:10px;top:0;width:50px;height:10px'></div></div>");

        Assert.AreEqual(140, Box(root, "a").Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-width")]
    [Spec("css-position-3", "#abs-non-replaced-width")]
    [Spec("css-position-3", "#abspos-auto-size")]
    public void Absolute_WithLeftAndRight_StretchesTheAutoWidth()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:10px;right:30px;top:0;height:10px'></div></div>");
        var a = Box(root, "a");

        Assert.AreEqual(10, a.Location.X, Delta);
        Assert.AreEqual(160, a.Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-width")]
    [Spec("css-position-3", "#abs-non-replaced-width")]
    [Spec("css-position-3", "#abspos-auto-size")]
    public void Absolute_WithAutoWidth_ShrinksToFitItsContent()
    {
        var root = Layout("<div style='position:relative;width:400px;height:100px'><div id='a' style='position:absolute;left:0;top:0'>xx yy</div></div>");
        var a = Box(root, "a");
        var words = LayoutHarness.Descendants(a).SelectMany(b => b.Words).ToList();

        Assert.AreEqual(words[^1].Right - words[0].Left, a.Size.Width, 1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-width")]
    [Spec("css-position-3", "#abs-non-replaced-width")]
    [Spec("css-position-3", "#abspos-margins")]
    public void Absolute_WithLeftRightAndWidth_IsCenteredByAutoMargins()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:0;right:0;width:100px;margin:0 auto;top:0;height:10px'></div></div>");

        Assert.AreEqual(50, Box(root, "a").Location.X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-replaced-width")]
    [Spec("css-position-3", "#abs-replaced-width")]
    public void AbsoluteImage_KeepsItsIntrinsicWidth()
    {
        var root = TestImages.Layout($"<div style='position:relative;width:200px;height:100px'><img id='i' style='position:absolute;left:0;top:0' src='{Image}'></div>");

        Assert.AreEqual(40, TestImages.ImageRect(root, "i").Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inlineblock-width")]
    public void InlineBlock_WithAWidth_TakesThatWidthOnTheLine()
    {
        var root = Layout("<div>a<span id='s' style='display:inline-block;width:100px'>x</span><span id='t'>y</span></div>");
        var s = Box(root, "s");

        Assert.AreEqual(100, s.Rectangles.Values.Single().Width, Delta);
        Assert.AreEqual(s.Rectangles.Values.Single().Right, FirstWord(Box(root, "t"))!.Left, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inlineblock-width")]
    public void InlineBlock_WithAutoWidth_IsAsWideAsItsContent()
    {
        var root = Layout("<div>a<span id='s' style='display:inline-block'>xx</span><span id='t'>y</span></div>");
        var word = FirstWord(Box(root, "s"))!;

        Assert.AreEqual(word.Width, Box(root, "s").Rectangles.Values.Single().Width, Delta);
        Assert.AreEqual(word.Right, FirstWord(Box(root, "t"))!.Left, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#inlineblock-width")]
    public void InlineBlock_TooWideForTheLine_ShrinksToTheAvailableWidthAsOneBox()
    {
        var root = Layout("<div style='width:200px'>a <span id='s' style='display:inline-block'>aaaa bbbb cccc dddd eeee ffff gggg hhhh</span></div>");
        var s = Box(root, "s");

        Assert.AreEqual(1, s.Rectangles.Count);
        Assert.AreEqual(0, s.Rectangles.Values.Single().X, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inlineblock-replaced-width")]
    public void InlineBlockImage_KeepsItsIntrinsicWidth()
    {
        var root = TestImages.Layout($"<div style='width:200px'><img id='i' style='display:inline-block;height:40px' src='{Image}'></div>");

        Assert.AreEqual(80, TestImages.ImageRect(root, "i").Width, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void MaxWidth_ScalesDownAnImageKeepingItsRatio()
    {
        var root = TestImages.Layout($"<img id='i' style='max-width:20px' src='{Image}'>");
        var r = TestImages.ImageRect(root, "i");

        Assert.AreEqual((20d, 10d), (r.Width, r.Height));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void MaxWidth_LimitsABlock()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='max-width:100px;height:5px'></div></div>");

        Assert.AreEqual(100, Box(root, "c").Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void MinWidth_WidensABlock()
    {
        var root = Layout("<div style='width:200px'><div id='c' style='width:50px;min-width:100px;height:5px'></div></div>");

        Assert.AreEqual(100, Box(root, "c").Size.Width, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-widths")]
    public void MinWidth_WidensAnImage()
    {
        var root = TestImages.Layout($"<img id='i' style='min-width:80px' src='{Image}'>");

        Assert.AreEqual(80, TestImages.ImageRect(root, "i").Width, Delta);
    }

    private static CssRect? FirstWord(CssBox box) =>
        box.Words.Count > 0 ? box.Words[0] : box.Boxes.Select(FirstWord).FirstOrDefault(w => w != null);
}
