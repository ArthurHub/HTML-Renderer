using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Painting order within a stacking context, checked by the order of the recorded draw calls.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class StackingContextTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void BlockBackgrounds_PaintInTreeOrder()
    {
        var g = Paint("<div style='height:40px;background:rgb(255,0,0)'><div style='height:20px;background:rgb(0,0,255)'></div></div>");

        Assert.IsLessThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void Background_PaintsBeforeText()
    {
        var g = Paint("<div style='background:rgb(0,0,255)'>Text</div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), TextIndex(g, "Text"));
    }

    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    [Spec("css21", "visuren.html#z-index")]
    [Spec("css-position-3", "#stacking")]
    public void AbsoluteBox_PaintsAfterLaterInFlowSiblings()
    {
        var g = Paint("<div style='position:relative'>"
                      + "<div style='position:absolute;width:20px;height:20px;background:rgb(255,0,0)'></div>"
                      + "<div style='height:20px;background:rgb(0,0,255)'></div></div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visuren.html#z-index")]
    [Spec("css-position-3", "#stacking")]
    public void HigherZIndex_PaintsInFrontRegardlessOfDocumentOrder()
    {
        var g = Paint("<div style='position:absolute;z-index:2;top:0;left:0;width:50px;height:50px;background:rgb(255,0,0)'></div>"
                      + "<div style='position:absolute;z-index:1;top:0;left:0;width:50px;height:50px;background:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    [Spec("css21", "visuren.html#z-index")]
    public void PositionedZIndex_PaintsOverFixedPositionedContent()
    {
        var g = Paint("<div style='position:fixed; top:0; left:0; width:50px; height:50px; background:rgb(255,0,0);'></div>"
                      + "<div style='position:relative; z-index:2; width:50px; height:50px; background:rgb(0,0,255);'></div>");

        Assert.IsGreaterThan(RectIndex(g, Red), RectIndex(g, Blue));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    [Spec("css21", "visuren.html#z-index")]
    public void NegativeZIndex_PaintsBelowInFlowBlocks()
    {
        var g = Paint("<div style='height:20px;background:rgb(0,0,255)'>"
                      + "<div style='position:absolute;z-index:-1;width:20px;height:20px;background:rgb(255,0,0)'></div></div>");

        Assert.IsLessThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    [Spec("css21", "visuren.html#z-index")]
    [Spec("css-position-3", "#stacking")]
    public void RelativeBox_PaintsAfterLaterInFlowSiblings()
    {
        var g = Paint("<div style='position:relative;height:20px;background:rgb(255,0,0)'></div>"
                      + "<div style='margin-top:-20px;height:20px;background:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void AbsoluteBox_PaintsAfterInFlowContentOutsideItsParent()
    {
        var g = Paint("<div style='position:relative'><div style='position:absolute;width:20px;height:20px;background:rgb(255,0,0)'></div></div>"
                      + "<div style='height:20px;background:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void Float_PaintsAfterLaterBlockBackgrounds()
    {
        var g = Paint("<div style='float:left;width:50px;height:50px;background:rgb(255,0,0)'></div>"
                      + "<div style='height:50px;background:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), RectIndex(g, Red));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void BlockBackgrounds_PaintBeforeInlineContent()
    {
        var g = Paint("<div style='height:20px'>Text</div>"
                      + "<div style='margin-top:-20px;height:20px;background:rgb(0,0,255)'></div>");

        Assert.IsGreaterThan(RectIndex(g, Blue), TextIndex(g, "Text"));
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void UnderlineAndOverline_PaintBeforeTheText_LineThroughAfter()
    {
        var g = Paint("<div><span style='text-decoration:underline overline line-through'>Deco</span></div>");

        var text = TextIndex(g, "Deco");
        var lines = g.Log.Select((e, i) => (e, i)).Where(x => x.e is RecordingGraphics.DrawLineCall).ToList();
        Assert.HasCount(3, lines);

        var lineThroughY = lines.Select(x => ((RecordingGraphics.DrawLineCall)x.e).Y1).OrderBy(y => y).ElementAt(1);
        foreach (var (e, i) in lines)
        {
            if (((RecordingGraphics.DrawLineCall)e).Y1 == lineThroughY)
                Assert.IsGreaterThan(text, i);
            else
                Assert.IsLessThan(text, i);
        }
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "zindex.html#painting-order")]
    public void RootBackground_CoversTheWholeCanvas()
    {
        var (_, container) = PaintHarness.Layout(
            "<!DOCTYPE html><html style='background:rgb(255,0,0)'><body style='margin:10px;height:20px'>x</body></html>", 300, 300);
        var g = PaintHarness.PaintPage(container);

        var background = g.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == Red);

        Assert.AreEqual(0, background.Y, 0.5);
        Assert.AreEqual(300, background.Height, 0.5);
    }

    private static RecordingGraphics Paint(string body)
    {
        var (_, container) = PaintHarness.Layout($"<!DOCTYPE html><html><head></head><body style='margin:0'>{body}</body></html>");
        return PaintHarness.PaintPage(container);
    }

    private static int RectIndex(RecordingGraphics g, RColor color)
    {
        var index = g.Log.FindIndex(e => e is RecordingGraphics.DrawRectCall r && r.Color == color);
        Assert.IsGreaterThanOrEqualTo(0, index, $"No rectangle painted in {color}");
        return index;
    }

    private static int TextIndex(RecordingGraphics g, string text)
    {
        var index = g.Log.FindIndex(e => e is RecordingGraphics.DrawStringCall s && s.Text == text);
        Assert.IsGreaterThanOrEqualTo(0, index, $"'{text}' wasn't painted");
        return index;
    }
}
