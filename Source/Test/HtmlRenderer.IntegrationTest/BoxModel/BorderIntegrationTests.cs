using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.BoxModel;

[DoNotParallelize]
[TestClass]
public sealed class BorderIntegrationTests
{
    [TestMethod]
    [Spec("css21", "box.html#border-width-properties")]
    [Spec("css-backgrounds-3", "#border-width")]
    public void WidthKeywords_GrowFromThinOverMediumToThick()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='thin' style='border:thin solid'></div><div id='medium' style='border:medium solid'></div><div id='thick' style='border:thick solid'></div>"));

        var thin = LayoutHarness.FindById(root, "thin")!.ActualBorderTopWidth;
        var medium = LayoutHarness.FindById(root, "medium")!.ActualBorderTopWidth;
        var thick = LayoutHarness.FindById(root, "thick")!.ActualBorderTopWidth;

        Assert.IsGreaterThan(0, thin);
        Assert.IsGreaterThan(thin, medium);
        Assert.IsGreaterThan(medium, thick);
    }

    [TestMethod]
    [Spec("css21", "box.html#border-width-properties")]
    [Spec("css-backgrounds-3", "#border-width")]
    public void StyleNone_ComputesTheWidthToZero()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='border-width:5px;border-style:none'></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(0, b.ActualBorderTopWidth);
        Assert.AreEqual(0, b.Size.Height);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-width-properties")]
    [Spec("css-backgrounds-3", "#border-width")]
    public void StyleHidden_ComputesTheWidthToZero()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='border-width:5px;border-style:hidden'></div>"));

        Assert.AreEqual(0, LayoutHarness.FindById(root, "b")!.ActualBorderTopWidth);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-width-properties")]
    [Spec("css-backgrounds-3", "#border-width")]
    public void InitialWidth_IsMedium()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='initial' style='border-style:solid'></div><div id='medium' style='border:medium solid'></div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "medium")!.ActualBorderTopWidth, LayoutHarness.FindById(root, "initial")!.ActualBorderTopWidth);
    }

    [TestMethod]
    [Spec("css21", "box.html#border-color-properties")]
    [Spec("css-backgrounds-3", "#border-color")]
    public void TransparentColor_PaintsNothingVisible()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='border:2px solid transparent'></div>"));

        Assert.AreEqual(0, LayoutHarness.FindById(root, "b")!.ActualBorderTopColor.A);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#border-color-properties")]
    [Spec("css-backgrounds-3", "#border-color")]
    public void InitialColor_IsTheElementsColor()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='color:rgb(10,20,30);border:2px solid'></div>"));

        Assert.AreEqual(RColor.FromArgb(10, 20, 30), LayoutHarness.FindById(root, "b")!.ActualBorderTopColor);
    }

    [TestMethod]
    [Spec("css21", "box.html#border-shorthand-properties")]
    [Spec("css-backgrounds-3", "#border-shorthands")]
    public void BorderShorthand_SetsAllFourSides()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='border:3px dashed red'></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        foreach (var (width, style, color) in new[]
                 {
                     (b.ActualBorderTopWidth, b.BorderTopStyle, b.ActualBorderTopColor),
                     (b.ActualBorderRightWidth, b.BorderRightStyle, b.ActualBorderRightColor),
                     (b.ActualBorderBottomWidth, b.BorderBottomStyle, b.ActualBorderBottomColor),
                     (b.ActualBorderLeftWidth, b.BorderLeftStyle, b.ActualBorderLeftColor),
                 })
        {
            Assert.AreEqual(3, width);
            Assert.AreEqual("dashed", style);
            Assert.AreEqual(RColor.FromArgb(255, 0, 0), color);
        }
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "box.html#bidi-box-model")]
    [Spec("css-writing-modes-4", "#bidi-fragment-boxes")]
    public void SplitRtlInline_PutsTheRightBorderOnTheFirstLine()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='direction:rtl;width:60px'><span id='s' style='border-left:3px solid red;border-right:5px solid blue'>aaa bbb ccc ddd eee</span></div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "s")!);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().OrderBy(l => l.Y1).ToList();
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), lines.First().Color);
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), lines.Last().Color);
    }

    [TestMethod]
    [Spec("css21", "box.html#border-shorthand-properties")]
    [Spec("css-backgrounds-3", "#border-shorthands")]
    public void SideShorthand_OnlySetsThatSide()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b' style='border-top:4px solid blue'></div>"));
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(4, b.ActualBorderTopWidth);
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), b.ActualBorderTopColor);
        Assert.AreEqual(0, b.ActualBorderRightWidth);
        Assert.AreEqual(0, b.ActualBorderBottomWidth);
        Assert.AreEqual(0, b.ActualBorderLeftWidth);
    }
}
