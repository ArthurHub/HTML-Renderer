using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class StackingContextTests
{
    [TestMethod]
    [Spec("css21", "visuren.html#z-index")]
    public void AbsolutelyPositionedBox_PaintsAfterALaterInFlowBlock()
    {
        var html = PaintHarness.Wrap(
            "<div style='position:absolute;top:0;left:0;width:50px;height:50px;background-color:rgb(255,0,0)'></div>"
            + "<div style='height:50px;background-color:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(PaintIndex(html, 0, 0, 255), PaintIndex(html, 255, 0, 0));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#z-index")]
    public void RelativelyPositionedBox_PaintsAfterALaterInFlowBlock()
    {
        var html = PaintHarness.Wrap(
            "<div style='position:relative;top:25px;height:50px;background-color:rgb(255,0,0)'></div>"
            + "<div style='height:50px;background-color:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(PaintIndex(html, 0, 0, 255), PaintIndex(html, 255, 0, 0));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#z-index")]
    public void HigherZIndex_PaintsInFrontRegardlessOfDocumentOrder()
    {
        var html = PaintHarness.Wrap(
            "<div style='position:absolute;z-index:2;top:0;left:0;width:50px;height:50px;background-color:rgb(255,0,0)'></div>"
            + "<div style='position:absolute;z-index:1;top:0;left:0;width:50px;height:50px;background-color:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(PaintIndex(html, 0, 0, 255), PaintIndex(html, 255, 0, 0));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#z-index")]
    public void NegativeZIndex_PaintsBehindInFlowBlocks()
    {
        var html = PaintHarness.Wrap(
            "<div style='height:50px;background-color:rgb(0,0,255)'></div>"
            + "<div style='position:absolute;z-index:-1;top:0;left:0;width:50px;height:50px;background-color:rgb(255,0,0)'></div>");

        Assert.IsLessThan(PaintIndex(html, 0, 0, 255), PaintIndex(html, 255, 0, 0));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#z-index")]
    public void PositionedZIndex_PaintsOverFixedPositionedContent()
    {
        var html = LayoutHarness.Wrap(
            "<div id='fixedbar' style='position:fixed; top:0; left:0; width:50px; height:50px; background:rgb(0,0,0);'></div>"
            + "<div id='intro' style='position:relative; z-index:2; width:50px; height:50px; background:rgb(255,255,255);'></div>");

        var (_, container) = PaintHarness.Layout(html);
        var recorder = PaintHarness.PaintPage(container);

        var drawRectCalls = recorder.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();
        var fixedBarPaintIndex = drawRectCalls.FindIndex(c => c.Color.R == 0 && c.Color.G == 0 && c.Color.B == 0);
        var introPaintIndex = drawRectCalls.FindIndex(c => c.Color.R == 255 && c.Color.G == 255 && c.Color.B == 255);

        Assert.IsGreaterThanOrEqualTo(0, fixedBarPaintIndex);
        Assert.IsGreaterThanOrEqualTo(0, introPaintIndex);
        Assert.IsTrue(fixedBarPaintIndex < introPaintIndex,
            "the z-index:2 box must paint after (on top of) the fixed bar, regardless of document order");
    }

    private static int PaintIndex(string html, int r, int g, int b)
    {
        var (_, container) = PaintHarness.Layout(html);
        var rects = PaintHarness.PaintPage(container).Log.OfType<RecordingGraphics.DrawRectCall>().ToList();
        var index = rects.FindIndex(c => c.Color.R == r && c.Color.G == g && c.Color.B == b);

        Assert.IsGreaterThanOrEqualTo(0, index);
        return index;
    }
}
