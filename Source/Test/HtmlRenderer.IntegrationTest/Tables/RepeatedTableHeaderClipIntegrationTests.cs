using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// The <c>overflow: hidden</c> clip of a repeated &lt;thead&gt; is resolved against where its content paints on
/// that page, not on another one.
/// </summary>
/// <remarks>
/// <c>break-inside: avoid</c> is set on the &lt;thead&gt; because the UA rule for it is under
/// <c>@media print</c> and <c>MockAdapter</c> uses screen. The fixture has 42 rows so several rows land on the
/// last page, a page only reached through the last row's own move gets no repeated header.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class RepeatedTableHeaderClipIntegrationTests
{
    private const double PageHeight = 200;
    private const double Margin = 10;

    /// <summary>
    /// A table long enough to repeat its header on several pages, whose header cell clips its own
    /// content via an inner <c>overflow:hidden</c> div, where the clipped text has to sit in a box of its own below the clipping div (the walk starts at the
    /// painted box's containing block; text held directly on the clipping box itself never asks about it).
    /// </summary>
    private static string ClippingHeaderTable() => PaintHarness.Wrap(
        "<table style='width:300px;border-collapse:collapse;font:10px Arial'>"
        + "<thead style='break-inside:avoid'><tr><th style='padding:0;text-align:left'>"
        + "<div id='h' style='overflow:hidden;height:14px'><span>HEADERMARKER</span></div>"
        + "</th></tr></thead><tbody>"
        + string.Concat(Enumerable.Range(1, 42).Select(i => $"<tr><td style='height:14px;padding:0'>Row {i}</td></tr>"))
        + "</tbody></table>");

    [TestMethod]
    public void ClippedRepeatedHeader_IsPaintedOnEveryPageItRepeatsOn()
    {
        var (_, container) = PaintHarness.LayoutPaginated(ClippingHeaderTable(), pageHeight: PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages >= 3, $"fixture must span several pages, got {pages}");

        // Including the intermediate pages, which is where a clip resolved from another page's geometry
        // would cull the row outright.
        for (var page = 0; page < pages; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);
            Assert.IsTrue(recording.DrawStringCalls.Any(w => w.Text.Contains("HEADERMARKER")),
                $"page {page} did not paint the repeated header's clipped content");
        }
    }

    [TestMethod]
    public void ClippedRepeatedHeader_ClipsAtItsOwnPagesPosition()
    {
        var (_, container) = PaintHarness.LayoutPaginated(ClippingHeaderTable(), pageHeight: PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages >= 3, $"fixture must span several pages, got {pages}");

        // Fragment coordinates - and every clip pushed while painting a page - are fragmentainer-local, so
        // a clip resolved from a stale/shared position (rather than this clone's own, correctly-shifted
        // geometry) would land far outside this small page's own band.
        for (var page = 0; page < pages; page++)
        {
            var recording = PaintHarness.PaintPage(container, page);
            Assert.IsTrue(recording.Log.OfType<RecordingGraphics.PushClipCall>().Any(),
                $"page {page} pushed no clip at all - the header's overflow:hidden div never painted");

            foreach (var push in recording.Log.OfType<RecordingGraphics.PushClipCall>())
            {
                Assert.IsTrue(push.Rect.Top > -PageHeight && push.Rect.Top < 2 * PageHeight,
                    $"page {page} pushed a clip at Y={push.Rect.Top:F1}, well outside this page's own band");
            }
        }
    }

    [TestMethod]
    public void PaintingAPage_DoesNotMoveTheLiveSourceBoxes()
    {
        var (root, container) = PaintHarness.LayoutPaginated(ClippingHeaderTable(), pageHeight: PageHeight, margin: Margin);

        var sourceHeader = PaintHarness.FindById(root, "h")!;
        var before = (sourceHeader.Location.X, sourceHeader.Location.Y, sourceHeader.ActualRight, sourceHeader.ActualBottom);

        // Paint is a read of the fragment tree/the clones it holds - it must never write a page's geometry
        // back onto the shared, live source subtree, which is what would make painting one page change
        // what a later page paints.
        PaintHarness.PaintPage(container, 0);

        Assert.AreEqual(before, (sourceHeader.Location.X, sourceHeader.Location.Y, sourceHeader.ActualRight, sourceHeader.ActualBottom));
    }
}
