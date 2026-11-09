using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class OutlineStylePaintIntegrationTests
{
    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void SolidOutline_PaintsAFourSidedRingAroundTheBorderBox()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:5px solid rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51)).ToList();

        Assert.AreEqual(4, rects.Count, "expected one filled rect per side of the outline ring");

        // Top and bottom bands are 5px tall, left and right bands 5px wide
        Assert.AreEqual(2, rects.Count(r => System.Math.Abs(r.Height - 5) < 0.1));
        Assert.AreEqual(2, rects.Count(r => System.Math.Abs(r.Width - 5) < 0.1));
    }

    [Ignore("outline-style: auto (CSS UI 3) isn't parsed")]
    [TestMethod]
    public void OutlineAuto_PaintsTheSameAsSolid()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:5px auto rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51)).ToList();

        Assert.AreEqual(4, rects.Count);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void OutlineColorUnset_FallsBackToCurrentColor()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;color:rgb(10,20,30);outline-style:solid;outline-width:5px'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();

        Assert.AreEqual(4, rects.Count);
        Assert.IsTrue(rects.All(r => r.Color == RColor.FromArgb(10, 20, 30)));
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void Outline_PaintsAfterBorder()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;border:3px solid rgb(1,2,3);outline:5px solid rgb(4,5,6)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var borderIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawLineCall line && line.Color == RColor.FromArgb(1, 2, 3));
        var outlineIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall rect && rect.Color == RColor.FromArgb(4, 5, 6));

        Assert.IsTrue(borderIndex >= 0 && outlineIndex >= 0);
        Assert.IsTrue(outlineIndex > borderIndex, "outline must paint after (on top of) the border");
    }

    [TestMethod]
    [DataRow("none")]
    [DataRow("hidden")]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void OutlineStyleNoneOrHidden_PaintsNothing(string style)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='width:100px;height:50px;outline:5px {style} rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(51, 51, 51)));
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void ZeroWidthOutline_PaintsNothing()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:0 solid rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(51, 51, 51)));
    }

    [Ignore("outline-offset (CSS UI 3) isn't parsed")]
    [TestMethod]
    public void OutlineOffset_PushesTheRingFurtherFromTheBorderBox()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:5px solid rgb(51,51,51);outline-offset:10px'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var borderBox = div.Rectangles.Values.Single();
        var top = g.Log.OfType<RecordingGraphics.DrawRectCall>().First(r => r.Color == RColor.FromArgb(51, 51, 51));

        Assert.AreEqual(borderBox.Y - 10 - 5, top.Y, 0.1);
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void OutlineColorInvert_FallsBackToCurrentColor()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='width:100px;height:50px;color:rgb(51,51,51);outline:5px solid invert'></div>"));

        var g = PaintHarness.PaintPage(container);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51)).ToList();

        Assert.AreEqual(4, rects.Count);
    }

    [TestMethod]
    [DataRow("<img id='e' style='width:20px;height:20px;outline:5px solid rgb(51,51,51)'>")]
    [DataRow("<hr id='e' style='outline:5px solid rgb(51,51,51)'>")]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void ReplacedAndRuleElements_PaintTheirOutline(string element)
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(element));

        var g = PaintHarness.PaintPage(container);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51)).ToList();

        Assert.AreEqual(4, rects.Count);
    }

    [TestMethod]
    public void InlineSplitAroundABlock_KeepsTheOutlineOnEveryPart()
    {
        // The span gets split into a part before and a part after the block, and both parts are copies.
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span style='outline:2px solid rgb(51,51,51)'>before<div>block</div>after</span>"));

        var g = PaintHarness.PaintPage(container);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51)).ToList();

        Assert.IsGreaterThanOrEqualTo(8, rects.Count, "both the part before and the part after the block need a ring");
    }

    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void Outline_DoesNotTakeUpSpace()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='a' style='height:50px;outline:10px solid black'></div><div id='b' style='height:10px'></div>"));

        var a = PaintHarness.FindById(root, "a")!;
        var b = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual(0, a.Location.X, 0.1);
        Assert.AreEqual(0, a.Location.Y, 0.1);
        Assert.AreEqual(50, b.Location.Y, 0.1);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [DataRow("dotted")]
    [DataRow("dashed")]
    [DataRow("double")]
    [DataRow("groove")]
    [DataRow("ridge")]
    [DataRow("inset")]
    [DataRow("outset")]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void OutlineStyle_AcceptsEveryBorderStyle(string style)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='width:100px;height:50px;outline:6px {style} rgb(51,51,51)'>x</div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        // The box has no background or border, every shape painted belongs to the outline
        var shapes = g.Log.Count(c => c is RecordingGraphics.DrawRectCall or RecordingGraphics.DrawLineCall or RecordingGraphics.DrawPolygonCall);
        Assert.IsGreaterThan(0, shapes, $"'{style}' outline painted nothing");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void Outline_IsPaintedOnTopOfTheContent()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:5px solid rgb(51,51,51)'>"
            + "<div style='margin:-5px;height:60px;background-color:rgb(10,20,30)'></div></div>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "b")!);

        var outlineIndex = g.Log.FindLastIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == RColor.FromArgb(51, 51, 51));
        var childIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == RColor.FromArgb(10, 20, 30));

        Assert.IsTrue(outlineIndex >= 0 && childIndex >= 0);
        Assert.IsTrue(outlineIndex > childIndex, "the child's background covers the outline");
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "ui.html#dynamic-outlines")]
    public void InlineBrokenAcrossLines_GetsOneOutlineEnclosingAllItsBoxes()
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div style='width:60px'><span style='outline:2px solid rgb(51,51,51)'>aaaa bbbb cccc</span></div>"));

        var g = PaintHarness.PaintPage(container);

        // Horizontal bands of the outline, the lines are equally wide so only the top and bottom edge remain
        var bandTops = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51) && r.Width > 2)
            .Select(r => r.Y).Distinct().ToList();
        Assert.AreEqual(2, bandTops.Count);
    }
}
