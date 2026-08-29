using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Ported from PeachPDF.Tests' OutlineStylePaintIntegrationTests.cs, adapted to this fork's new (and more
/// limited) <c>OutlineDrawHandler</c> - see that class's remarks for exactly what's implemented: a plain
/// rectangular ring for <c>solid</c>/<c>auto</c> only, no <c>outline-offset</c> (this fork's CSS-OM has no
/// such property to read a value from), and every other style (<c>dotted</c>/<c>dashed</c>/<c>double</c>/
/// <c>groove</c>/<c>ridge</c>/<c>inset</c>/<c>outset</c>) paints nothing, same as <c>none</c>.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class OutlineStylePaintIntegrationTests
{
    [TestMethod]
    public void SolidOutline_PaintsAFourSidedRingAroundTheBorderBox()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:5px solid rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(51, 51, 51)).ToList();

        Assert.AreEqual(4, rects.Count, "expected one filled rect per side of the outline ring");

        // Two of the four bands are 5px tall (top/bottom, spanning the full outer width including
        // corners) and two are 5px wide (left/right, spanning just the box's own height) - regardless of
        // which order OutlineDrawHandler emits them in.
        Assert.AreEqual(2, rects.Count(r => System.Math.Abs(r.Height - 5) < 0.1));
        Assert.AreEqual(2, rects.Count(r => System.Math.Abs(r.Width - 5) < 0.1));
    }

    [Ignore("outline-style:auto cannot be parsed at all on this fork: Map.LineStyles (Core/CssEngine/Model/" +
            "Map.cs), the shared keyword table outline-style's converter reuses from border-style, has no " +
            "\"auto\" entry - adding one there would also make it a legal (but spec-invalid) border-style " +
            "value, so this needs its own dedicated converter rather than a shared-map edit, which is out of " +
            "scope for this pass. OutlineDrawHandler.Draw's own \"style == Auto\" check is consequently dead " +
            "code today - reachable only by setting the property programmatically, never via CSS text.")]
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
    public void Outline_PaintsAfterBorder()
    {
        // Paint-order: DrawBoxBorders runs, then OutlineDrawHandler - the outline's draw calls must come
        // after the border's in the recording.
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
    public void OutlineStyleNoneOrHidden_PaintsNothing(string style)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='width:100px;height:50px;outline:5px {style} rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(51, 51, 51)));
    }

    [TestMethod]
    public void ZeroWidthOutline_PaintsNothing()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;outline:0 solid rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(51, 51, 51)));
    }

    [Ignore("outline-offset is not implemented on this fork: the CSS-OM here has no OutlineOffsetProperty at " +
            "all (confirmed - no PropertyNames.OutlineOffset, no case in PropertyFactory), so there is no value " +
            "for OutlineDrawHandler to read; the ring always sits flush against the border-box edge instead of " +
            "the requested 10px further out.")]
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
}
