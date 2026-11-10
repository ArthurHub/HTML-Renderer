using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// 'overflow' and 'clip'. The clip tests replay the recorded PushClip/PopClip calls to find the clip that was
/// active when a box painted its background.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class OverflowClipIntegrationTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    // --- Geometry tests (verify clip bounds after layout) ---

    [TestMethod]
    public void OverflowHidden_WithPadding_ChildBoundsWithinPaddingBoxClip()
    {
        // Container: overflow:hidden, padding:10px, 100px content width. Child fills the content area.
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='outer' style='overflow:hidden; padding:10px; width:100px; height:100px;'>"
            + "<div id='inner' style='height:80px; border-radius:20px;'></div></div>"));

        var outer = PaintHarness.FindById(root, "outer")!;
        var inner = PaintHarness.FindById(root, "inner")!;

        Assert.AreEqual("hidden", outer.Overflow);

        var paddingBoxRight = outer.ClientRight + outer.ActualPaddingRight;

        Assert.IsTrue(inner.ActualRight <= paddingBoxRight + 0.5,
            $"Child right ({inner.ActualRight:F3}) exceeds padding-box clip right ({paddingBoxRight:F3})");
    }

    [TestMethod]
    public void RoundedBoxInTableCell_NonUniformRadius_BoundsWithinPaddingBoxClip()
    {
        // td gets overflow:hidden from the default stylesheet, a non-uniformly rounded div fills its content area.
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table style='border-collapse:collapse; width:300px;'><tr>"
            + "<td id='cell' style='padding:3px;'><div id='box' style='border-top-left-radius:10px; border-bottom-right-radius:30px; height:60px; border:2px solid black;'></div></td>"
            + "</tr></table>"));

        var td = PaintHarness.FindById(root, "cell")!;
        var div = PaintHarness.FindById(root, "box")!;

        Assert.AreEqual("hidden", td.Overflow);

        var paddingBoxRight = td.ClientRight + td.ActualPaddingRight;

        Assert.IsTrue(div.ActualRight <= paddingBoxRight + 0.5,
            $"Div right ({div.ActualRight:F3}) exceeds padding-box clip right ({paddingBoxRight:F3})");
    }

    [TestMethod]
    public void RoundedBoxInTableCell_FourCornerRadii_BoundsWithinPaddingBoxClip()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table style='border-collapse:collapse; width:300px;'><tr>"
            + "<td id='cell' style='padding:3px;'><div id='box' style='border-radius:5px 15px 30px 45px; height:60px; border:2px solid black;'></div></td>"
            + "</tr></table>"));

        var td = PaintHarness.FindById(root, "cell")!;
        var div = PaintHarness.FindById(root, "box")!;

        var paddingBoxRight = td.ClientRight + td.ActualPaddingRight;

        Assert.IsTrue(div.ActualRight <= paddingBoxRight + 0.5,
            $"Div right ({div.ActualRight:F3}) exceeds padding-box clip right ({paddingBoxRight:F3})");
    }

    [TestMethod]
    public void OverflowHidden_ZeroPadding_ClipEqualsContentBox()
    {
        // With zero padding the padding box and the content box coincide.
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='outer' style='overflow:hidden; padding:0; width:100px; height:100px;'>"
            + "<div id='inner' style='height:80px;'></div></div>"));

        var outer = PaintHarness.FindById(root, "outer")!;
        var inner = PaintHarness.FindById(root, "inner")!;

        Assert.AreEqual(0.0, outer.ActualPaddingRight, 0.5);
        var paddingBoxRight = outer.ClientRight + outer.ActualPaddingRight;
        Assert.AreEqual(outer.ClientRight, paddingBoxRight, 0.5);

        Assert.IsTrue(inner.ActualRight <= paddingBoxRight + 0.5,
            $"Child right ({inner.ActualRight:F3}) exceeds content-box clip right ({paddingBoxRight:F3})");
    }

    // --- Smoke test (layout + paint must not throw) ---

    [TestMethod]
    public void RoundedBoxInTableCell_MultipleRadii_LayoutAndPaintDoNotThrow()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse; width:400px;'><tr>"
            + "<td style='padding:3px;'><div style='border-radius:20px; height:60px; background:blue; border:2px solid #1a6b8a;'></div></td>"
            + "<td style='padding:3px;'><div style='border-top-left-radius:10px; border-bottom-right-radius:30px; height:60px; background:blue; border:2px solid #1a6b8a;'></div></td>"
            + "<td style='padding:3px;'><div style='border-radius:5px 15px 30px 45px; height:60px; background:blue; border:2px solid #1a6b8a;'></div></td>"
            + "</tr></table>"));

        var table = PaintHarness.FindById(root, "t")!;

        PaintHarness.PaintBox(container, table);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_WithPadding_PaintClipReachesThePaddingBoxEdge()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='outer' style='overflow:hidden; padding:10px; width:100px; height:100px;'>"
            + "<div id='inner' style='height:80px;'></div></div>"));

        var outer = PaintHarness.FindById(root, "outer")!;

        var g = PaintHarness.PaintBox(container, outer);

        // The harness pushes the container bounds first, the overflow clip is the last push.
        var clip = g.Log.OfType<RecordingGraphics.PushClipCall>().Last().Rect;
        var paddingBoxRight = outer.ClientRight + outer.ActualPaddingRight;

        Assert.AreEqual(paddingBoxRight, clip.Right, 1.5);
    }

    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowVisible_DoesNotClipOverflowingContent()
    {
        var clip = ClipOfRedBox(
            "<div style='width:50px;height:50px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(1000, clip.Right, 0.5);
        Assert.AreEqual(4000, clip.Bottom, 0.5);
    }

    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_ClipsOverflowingChildToTheBox()
    {
        var clip = ClipOfRedBox(
            "<div style='overflow:hidden;width:50px;height:60px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(0, clip.Top, 0.5);
        Assert.AreEqual(50, clip.Right, 0.5);
        Assert.AreEqual(60, clip.Bottom, 0.5);
    }

    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_ClipsOverflowingText()
    {
        var (_, container) = PaintHarness.Layout(Doc(
            "<div style='overflow:hidden;width:30px;height:10px'>aaa bbb ccc ddd</div>"));
        var g = PaintHarness.PaintPage(container);

        Assert.AreEqual(10, ClipAt(g, e => e is RecordingGraphics.DrawStringCall s && s.Text == "aaa").Bottom, 0.5);
        if (g.DrawStringCalls.Any(c => c.Text == "ddd"))
            Assert.AreEqual(10, ClipAt(g, e => e is RecordingGraphics.DrawStringCall s && s.Text == "ddd").Bottom, 0.5);
    }

    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_ClipsAbsoluteDescendantWhoseContainingBlockIsTheBox()
    {
        var clip = ClipOfRedBox(
            "<div style='position:relative;overflow:hidden;width:50px;height:50px'>"
            + "<div style='position:absolute;left:0;top:0;width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(50, clip.Right, 0.5);
        Assert.AreEqual(50, clip.Bottom, 0.5);
    }

    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_OnAnInlineElement_DoesNotClip()
    {
        var clip = ClipOfRedBox(
            "<div><span style='overflow:hidden'><span style='display:inline-block;width:150px;height:150px;background:rgb(255,0,0)'></span></span></div>");

        Assert.AreEqual(1000, clip.Right, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [DataRow("scroll")]
    [DataRow("auto")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void ScrollAndAuto_ClipOverflowingContent(string overflow)
    {
        var clip = ClipOfRedBox(
            $"<div style='overflow:{overflow};width:50px;height:50px'><div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(50, clip.Right, 0.5);
        Assert.AreEqual(50, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_DoesNotClipAtAnIntermediateNonClippingBox()
    {
        var clip = ClipOfRedBox(
            "<div style='overflow:hidden;width:200px;height:200px'><div style='width:50px;height:50px'>"
            + "<div style='width:150px;height:150px;background:rgb(255,0,0)'></div></div></div>");

        Assert.AreEqual(200, clip.Right, 0.5);
        Assert.AreEqual(200, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_DoesNotClipAbsoluteDescendantWhoseContainingBlockIsAnAncestor()
    {
        var clip = ClipOfRedBox(
            "<div style='position:relative'><div style='overflow:hidden;width:50px;height:50px'>"
            + "<div style='position:absolute;left:0;top:0;width:150px;height:150px;background:rgb(255,0,0)'></div></div></div>");

        Assert.AreEqual(1000, clip.Right, 0.5);
        Assert.AreEqual(4000, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-control")]
    public void OverflowHidden_DoesNotClipFixedDescendant()
    {
        var clip = ClipOfRedBox(
            "<div style='overflow:hidden;width:50px;height:50px'>"
            + "<div style='position:fixed;left:0;top:0;width:150px;height:150px;background:rgb(255,0,0)'></div></div>");

        Assert.AreEqual(1000, clip.Right, 0.5);
        Assert.AreEqual(4000, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#overflow")]
    [Spec("css-overflow-3", "#overflow-propagation")]
    public void OverflowOnBody_IsAppliedToTheViewport()
    {
        var (_, container) = PaintHarness.Layout(
            "<!DOCTYPE html><html><body style='overflow:hidden;height:10px;margin:0'><div style='height:100px;background:rgb(255,0,0)'></div></body></html>",
            300, 300);
        var g = PaintHarness.PaintPage(container);

        var clip = ClipAt(g, e => e is RecordingGraphics.DrawRectCall r && r.Color == Red);

        Assert.AreEqual(300, clip.Bottom, 0.5);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#clipping")]
    public void ClipRect_ClipsAnAbsolutelyPositionedBox()
    {
        var clip = ClipOfRedBox(
            "<div style='position:absolute;left:0;top:0;clip:rect(0,10px,20px,0);width:50px;height:50px;background:rgb(255,0,0)'></div>");

        Assert.AreEqual(10, clip.Right, 0.5);
        Assert.AreEqual(20, clip.Bottom, 0.5);
    }

    private static string Doc(string body) => $"<!DOCTYPE html><html><head></head><body style='margin:0'>{body}</body></html>";

    private static RRect ClipOfRedBox(string body)
    {
        var (_, container) = PaintHarness.Layout(Doc(body));
        var g = PaintHarness.PaintPage(container);

        return ClipAt(g, e => e is RecordingGraphics.DrawRectCall r && r.Color == Red);
    }

    /// <summary>The intersection of all clips pushed and not yet popped when the first matching call was recorded.</summary>
    private static RRect ClipAt(RecordingGraphics g, System.Func<object, bool> match)
    {
        var stack = new List<RRect>();
        foreach (var entry in g.Log)
        {
            switch (entry)
            {
                case RecordingGraphics.PushClipCall push:
                    stack.Add(push.Rect);
                    break;
                case RecordingGraphics.PopClipCall:
                    stack.RemoveAt(stack.Count - 1);
                    break;
                default:
                    if (match(entry))
                        return stack.Aggregate(RRect.Intersect);
                    break;
            }
        }

        Assert.Fail("No matching draw call was recorded");
        return RRect.Empty;
    }
}
