using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Positioning;

[DoNotParallelize]
[TestClass]
public sealed class FixedPositionPaginationIntegrationTests
{
    [TestMethod]
    [Spec("css21", "visuren.html#fixed-positioning")]
    public void FixedPositionedBox_PaintPosition_IsInvariantToScrollOffset_WhileNormalContentShifts()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='fixedBox' style='position:fixed; top:10px; left:10px; width:30px; height:30px; background-color: rgb(12,34,56);'></div>" +
            "<div id='normalBox' style='width:30px; height:30px; background-color: rgb(65,43,21);'></div>"));

        var fixedBox = PaintHarness.FindById(root, "fixedBox")!;
        Assert.AreEqual(10.0, fixedBox.Location.X, 0.5);
        Assert.AreEqual(10.0, fixedBox.Location.Y, 0.5);

        container.ScrollOffset = RPoint.Empty;
        var g0 = PaintHarness.PaintBox(container, root);
        var fixedRect0 = g0.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(12, 34, 56));
        var normalRect0 = g0.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(65, 43, 21));

        container.ScrollOffset = new RPoint(0, 500);
        var g1 = PaintHarness.PaintBox(container, root);
        var fixedRect1 = g1.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(12, 34, 56));
        var normalRect1 = g1.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(65, 43, 21));

        Assert.AreEqual(fixedRect0.X, fixedRect1.X, 0.5);
        Assert.AreEqual(fixedRect0.Y, fixedRect1.Y, 0.5);
        Assert.AreEqual(normalRect0.Y + 500, normalRect1.Y, 0.5);
    }

    [Ignore("This fork has no page-break-before/page-break-after support at all - confirmed by direct source " +
            "read: no PageBreakBefore/PageBreakAfter property exists anywhere in CssBoxProperties (only " +
            "PageBreakInside), and the default stylesheet's own page-break-before/after rules (Core/CssDefaults.cs " +
            "~102-105) are inert since nothing consumes them. 'page-break-before: always' is silently dropped, so " +
            "there is no way to materialize distinct 'pages' the way PeachPDF's real PDF pagination does - " +
            "content just flows continuously down one tall canvas and every paragraph below stacks directly " +
            "under its predecessor instead of jumping to a new page band.")]
    [TestMethod]
    public void PageBreakBefore_PushesFollowingContentToTheNextSimulatedPage()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<p id='p1'>page 1</p>" +
            "<p id='p2' style='page-break-before: always'>page 2</p>" +
            "<p id='p3' style='page-break-before: always'>page 3</p>"));

        var p1 = PaintHarness.FindById(root, "p1")!;
        var p2 = PaintHarness.FindById(root, "p2")!;
        var p3 = PaintHarness.FindById(root, "p3")!;

        Assert.IsTrue(p2.Location.Y > p1.ActualBottom + 100, "expected page-break-before to push p2 onto a new page");
        Assert.IsTrue(p3.Location.Y > p2.ActualBottom + 100, "expected page-break-before to push p3 onto a new page");
    }
}
