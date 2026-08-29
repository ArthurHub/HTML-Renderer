using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// CSS 2.1 §14.2.1: <c>background-color</c> paints a solid fill behind a box's content, covering the box's
/// padding + content area (border-box minus the border itself). No dedicated background paint-call test
/// existed anywhere in this repo before this - only CSS-OM parsing (<c>BackgroundPropertyTests.cs</c>).
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class BackgroundPaintIntegrationTests
{
    [TestMethod]
    public void BackgroundColor_PaintsASolidFillRect()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;background-color:rgb(10,20,30)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);
        var fills = g.Log.OfType<RecordingGraphics.DrawRectCall>()
            .Where(r => r.Color == RColor.FromArgb(10, 20, 30)).ToList();

        Assert.AreEqual(1, fills.Count);
        Assert.AreEqual(100, fills[0].Width, 0.1);
        Assert.AreEqual(50, fills[0].Height, 0.1);
    }

    [TestMethod]
    public void BackgroundColorTransparent_PaintsNothing()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;background-color:transparent'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [TestMethod]
    public void BackgroundColorDefault_PaintsNothing()
    {
        // The initial value of background-color is "transparent" - a box with no background-color set at
        // all must not paint a fill either.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any());
    }

    [TestMethod]
    public void BackgroundColor_PaintsBeforeBorderAndOutline()
    {
        // CSS 2.1 Appendix E: background paints before border/outline for the same box.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:100px;height:50px;background-color:rgb(10,20,30);"
            + "border:3px solid rgb(1,2,3);outline:5px solid rgb(4,5,6)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var backgroundIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == RColor.FromArgb(10, 20, 30));
        var borderIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawLineCall l && l.Color == RColor.FromArgb(1, 2, 3));
        var outlineIndex = g.Log.FindIndex(c => c is RecordingGraphics.DrawRectCall r && r.Color == RColor.FromArgb(4, 5, 6));

        Assert.IsTrue(backgroundIndex >= 0 && borderIndex >= 0 && outlineIndex >= 0);
        Assert.IsTrue(backgroundIndex < borderIndex, "background must paint before the border");
        Assert.IsTrue(borderIndex < outlineIndex, "border must paint before the outline");
    }
}
