using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class HeightIntegrationTests
{
    private const double Delta = 0.5;

    private static readonly string Image = TestImages.Png(40, 20);

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body), maxWidth: 400).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;

    private static double Height(CssBox box) => box.ActualBottom - box.Location.Y;

    private static double LineHeight() => Height(Box(Layout("<div id='p'>x</div>"), "p"));

    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void LengthHeight_IsTheContentHeight()
    {
        var root = Layout("<div id='c' style='height:50px;padding:10px 0;border-top:5px solid'></div>");

        Assert.AreEqual(75, Height(Box(root, "c")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    [Spec("css-sizing-3", "#behave-auto")]
    public void PercentageHeight_AgainstAnAutoHeightContainingBlock_IsAuto()
    {
        var root = Layout("<div><div id='c' style='height:50%'>x</div></div>");

        Assert.AreEqual(LineHeight(), Height(Box(root, "c")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    [Spec("css-sizing-3", "#percentage-sizing")]
    public void PercentageHeight_ResolvesAgainstAnExplicitContainingBlockHeight()
    {
        var root = Layout("<div style='height:200px'><div id='c' style='height:50%'></div></div>");

        Assert.AreEqual(100, Height(Box(root, "c")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    [Spec("css-sizing-3", "#sizing-values")]
    public void NegativeHeight_IsIgnored()
    {
        var root = Layout("<div id='c' style='height:-50px'>x</div>");

        Assert.AreEqual(LineHeight(), Height(Box(root, "c")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    public void HeightOnAnInline_DoesNotApply()
    {
        var plain = Layout("<div id='p'>a<span>x</span>b</div>");
        var withHeight = Layout("<div id='p'>a<span style='height:200px'>x</span>b</div>");

        Assert.AreEqual(Height(Box(plain, "p")), Height(Box(withHeight, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#the-height-property")]
    [Spec("css-sizing-3", "#preferred-size-properties")]
    public void HeightOnAnInlineBlock_IsApplied()
    {
        var root = Layout("<div id='p'>a<span id='s' style='display:inline-block;height:100px'>x</span></div>");

        Assert.AreEqual(100, Box(root, "s").Rectangles.Values.Single().Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-non-replaced")]
    public void InlineContentArea_IsAsTallAsTheFont()
    {
        var root = Layout("<div>a<span id='s' style='font-size:30px'>x</span></div>");
        var s = Box(root, "s");

        Assert.AreEqual(s.ActualFont.Height, s.Rectangles.Values.Single().Height, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#inline-non-replaced")]
    public void InlineVerticalPaddingAndBorder_DontChangeTheLineHeight()
    {
        var plain = Layout("<div id='p'>a<span>x</span></div>");
        var padded = Layout("<div id='p'>a<span style='padding-top:30px;border-top:5px solid'>x</span></div>");

        Assert.AreEqual(Height(Box(plain, "p")), Height(Box(padded, "p")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-height")]
    public void AutoHeight_FollowsTheIntrinsicRatioOfTheWidth()
    {
        var root = TestImages.Layout($"<img id='i' style='width:80px' src='{Image}'>");

        Assert.AreEqual(40, TestImages.ImageRect(root, "i").Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-height")]
    public void BlockImage_AutoHeight_IsTheIntrinsicHeight()
    {
        var root = TestImages.Layout($"<img id='i' style='display:block' src='{Image}'>");

        Assert.AreEqual(20, TestImages.ImageRect(root, "i").Height, Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#inline-replaced-height")]
    public void EmHeight_IsApplied()
    {
        var root = TestImages.Layout($"<div style='font-size:10px'><img id='i' style='height:4em' src='{Image}'></div>");

        Assert.AreEqual(40, TestImages.ImageRect(root, "i").Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#normal-block")]
    public void AutoHeight_EndsAtTheLastChildsMarginEdge_WhenTheBottomMarginDoesNotCollapseThrough()
    {
        var root = Layout("<div id='p' style='border:1px solid'><div style='height:10px'></div><div style='height:20px;margin-bottom:7px'></div></div>");

        Assert.AreEqual(39, Height(Box(root, "p")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#normal-block")]
    public void AutoHeight_EndsAtTheLastChildsBorderEdge_WhenTheBottomMarginCollapsesThrough()
    {
        var root = Layout("<div id='p'><div style='height:10px;margin-bottom:20px'></div></div><div id='n' style='height:1px'></div>");

        Assert.AreEqual(10, Height(Box(root, "p")), Delta);
        Assert.AreEqual(30, Box(root, "n").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#normal-block")]
    public void AutoHeight_EndsAtTheLastLineBox()
    {
        var root = Layout("<div id='p'>x<div style='height:10px'></div>y</div>");

        Assert.AreEqual(2 * LineHeight() + 10, Height(Box(root, "p")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#normal-block")]
    public void AutoHeight_IgnoresFloatsAndAbsoluteChildren()
    {
        var root = Layout("<div id='f'><div style='float:left;height:50px;width:10px'></div></div>"
                          + "<div id='a'><div style='position:absolute;height:50px;width:10px'></div></div>");

        Assert.AreEqual(0, Height(Box(root, "f")), Delta);
        Assert.AreEqual(0, Height(Box(root, "a")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-height")]
    [Spec("css-position-3", "#abs-non-replaced-height")]
    public void Absolute_WithTopAndHeight_IsPlacedAtTop()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:0;top:10px;width:10px;height:30px'></div></div>");
        var a = Box(root, "a");

        Assert.AreEqual(10, a.Location.Y, Delta);
        Assert.AreEqual(30, Height(a), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-height")]
    [Spec("css-position-3", "#abs-non-replaced-height")]
    public void Absolute_WithBottomAndHeight_IsPlacedAtBottom()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:0;bottom:10px;width:10px;height:30px'></div></div>");

        Assert.AreEqual(60, Box(root, "a").Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-height")]
    [Spec("css-position-3", "#abs-non-replaced-height")]
    public void Absolute_WithAutoHeight_IsAsTallAsItsContent()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:0;top:0;width:100px'>x</div></div>");

        Assert.AreEqual(LineHeight(), Height(Box(root, "a")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#abs-non-replaced-height")]
    [Spec("css-position-3", "#abs-non-replaced-height")]
    [Spec("css-position-3", "#abspos-auto-size")]
    public void Absolute_WithTopAndBottom_StretchesTheAutoHeight()
    {
        var root = Layout("<div style='position:relative;width:200px;height:100px'><div id='a' style='position:absolute;left:0;top:10px;bottom:30px;width:10px'></div></div>");

        Assert.AreEqual(60, Height(Box(root, "a")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#abs-replaced-height")]
    [Spec("css-position-3", "#abs-replaced-height")]
    public void AbsoluteImage_KeepsItsIntrinsicHeight()
    {
        var root = TestImages.Layout($"<div style='position:relative;width:200px;height:100px'><img id='i' style='position:absolute;left:0;top:0' src='{Image}'></div>");

        Assert.AreEqual(20, TestImages.ImageRect(root, "i").Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#block-root-margin")]
    public void OverflowHiddenBlock_WithAHeight_IsThatTall()
    {
        var root = Layout("<div id='c' style='height:10px;overflow:hidden'>x<br>y<br>z</div>");

        Assert.AreEqual(10, Height(Box(root, "c")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#block-root-margin")]
    public void InlineBlock_WithBlockContent_StaysOnTheLine()
    {
        var root = Layout("<div>a<div id='s' style='display:inline-block'><div style='height:50px;width:30px'></div></div><span id='t'>y</span></div>");
        var s = Box(root, "s");

        Assert.IsNotNull(s);
        Assert.AreEqual(50, s.Rectangles.Values.Single().Height, Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#root-height")]
    public void BlockFormattingContextRoot_AutoHeight_CoversItsInFlowContent()
    {
        var root = Layout("<div id='p' style='overflow:hidden'><div style='height:10px'></div><div style='height:20px'></div></div>");

        Assert.AreEqual(30, Height(Box(root, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#root-height")]
    public void BlockFormattingContextRoot_AutoHeight_CoversItsFloats()
    {
        var root = Layout("<div id='p' style='overflow:hidden'><div style='height:10px'></div><div style='float:left;height:50px;width:10px'></div></div>");

        Assert.AreEqual(60, Height(Box(root, "p")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#root-height")]
    public void Float_AutoHeight_CoversItsFloatingDescendants()
    {
        var root = Layout("<div id='f' style='float:left;width:50px'><div style='height:30px'></div><div style='float:left;width:5px;height:60px'></div></div>");

        Assert.AreEqual(90, Height(Box(root, "f")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    [Spec("css-sizing-3", "#min-size-properties")]
    public void MinHeight_GrowsAnAutoHeightBlock()
    {
        var root = Layout("<div id='c' style='min-height:50px'>x</div>");

        Assert.AreEqual(50, Height(Box(root, "c")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    [Spec("css-sizing-3", "#max-size-properties")]
    public void MaxHeight_LimitsAnAutoHeightBlock()
    {
        var root = Layout("<div id='c' style='max-height:5px'>x<br>y<br>z</div>");

        Assert.AreEqual(5, Height(Box(root, "c")), Delta);
    }

    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    public void PercentageMinHeight_AgainstAnAutoHeightContainingBlock_IsZero()
    {
        var root = Layout("<div><div id='c' style='min-height:50%'>x</div></div>");

        Assert.AreEqual(LineHeight(), Height(Box(root, "c")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    [Spec("css-sizing-3", "#percentage-sizing")]
    public void PercentageMaxHeight_ResolvesAgainstAnExplicitContainingBlockHeight()
    {
        var root = Layout("<div style='height:200px'><div id='c' style='height:150px;max-height:50%'></div></div>");

        Assert.AreEqual(100, Height(Box(root, "c")), Delta);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visudet.html#min-max-heights")]
    [Spec("css-sizing-3", "#max-size-properties")]
    public void MaxHeight_ScalesDownAnImageKeepingItsRatio()
    {
        var root = TestImages.Layout($"<img id='i' style='max-height:10px' src='{Image}'>");
        var r = TestImages.ImageRect(root, "i");

        Assert.AreEqual((20d, 10d), (r.Width, r.Height));
    }
}
