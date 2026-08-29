using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/RepeatedTableHeaderClipIntegrationTests.cs: a repeating
/// &lt;thead&gt;'s own <c>overflow:hidden</c> clip must be resolved against the geometry the clip's
/// content actually paints at on THAT page, not against some other page's geometry.
/// </summary>
/// <remarks>
/// PeachPDF's own root defect does not exist here by construction, the same way Batch 3 found for list
/// markers: PeachPDF repeats a header by re-emitting fragments for one shared, live <c>CssProxyBox</c>
/// subtree whose <c>ContainingBlock</c> walk (used to resolve an <c>overflow:hidden</c> ancestor's clip
/// rectangle) reads that ONE box's current position - last set by whichever page positioned it most
/// recently. This port's <c>TableHeaderRepeat.CloneAndPosition</c> (<c>Core/Fragmentation/TableHeaderRepeat.cs</c>)
/// instead deep-clones a real, independent <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBox"/> subtree
/// per repeat, each with its OWN <c>Location</c>/<c>Size</c> baked in at clone time
/// (<c>CloneSubtree</c> copies them, then <c>CloneAndPosition</c> shifts the whole clone by
/// <c>targetTop - sourceRenderedTop</c>) - so <c>RenderUtils.ClipGraphicsByOverflow</c>'s own
/// <c>ContainingBlock</c> walk, run per-clone during <c>FragmentPainter.PaintFragmentContent</c>, always
/// reads THAT clone's own already-correctly-positioned geometry, never another page's. These three tests
/// are accordingly regression pins of the invariant rather than reproductions of PeachPDF's bug - confirmed
/// by actually running them (not just reading source), matching this batch's established practice.
/// <para>
/// <c>break-inside:avoid</c> is declared explicitly on the &lt;thead&gt; below rather than relied on from
/// the UA default stylesheet's <c>@media print { thead, tfoot { break-inside: avoid } }</c>
/// (<c>Core/CssDefaults.cs</c>) - <see cref="PaintHarness"/> lays out over <c>MockAdapter</c>, whose
/// <c>DefaultMediaType</c> (the <see cref="TheArtOfDev.HtmlRenderer.Adapters.RAdapter"/> base default) is
/// not <c>"print"</c>, so that print-scoped rule never matches here, same as the established convention in
/// <c>StageD4RepeatedHeaderTest.cs</c>/<c>KeepWithNextIntegrationTests.cs</c>. Confirmed empirically:
/// without the explicit declaration, <c>CssLayoutEngineTable.LayoutCells</c>'s <c>repeatsHeader</c> gate
/// (<c>BreakValues.AvoidsBreak(_headerBox.BreakInside)</c>) never fires and no page after the first paints
/// the header at all.
/// </para>
/// <para>
/// A real, confirmed production bug was found and fixed while calibrating this fixture, not merely
/// documented: <c>CssLayoutEngineTable.LayoutCells</c> fed the row cursor's raw <c>starty</c>/<c>cury</c>
/// straight into <c>HtmlContainerInt.PageIndexOf</c> for its own slot arithmetic. For a
/// <c>border-collapse:collapse</c> table, <c>GetVerticalSpacing()</c> is <c>-1</c> (a deliberate one-pixel
/// row/border overlap), so <c>starty</c> sits one pixel BELOW <c>CssBox.ClientTop</c> - and whenever a
/// table starts flush at a page's own content top (this fixture's own case: nothing precedes the table),
/// that one pixel was enough for <c>PageIndexOf</c> to floor into the slot BEFORE the one the table
/// actually starts on. Observed directly (200px pages, <c>MarginTop=10</c>, table at
/// <c>ClientTop=10</c>): <c>PageIndexOf(9)</c> returned <c>-1</c> instead of <c>0</c>. That corrupted two
/// things: the row-preservation straddle check (css-tables-3 6.1) saw the header row as spuriously
/// straddling a boundary it never crossed, and the repeated-header loop saw a spurious "transition" into
/// slot 0 at the very first body row - consuming its first repeat on a duplicate painted almost exactly on
/// top of the header the table already has in flow there (confirmed: before the fix, page 0 alone painted
/// "HEADERMARKER" twice, at (0,3) and (0,4)). Fixed at its source by clamping the slot lookup to
/// <c>CssBox.ClientTop</c> (immune to the collapsed-border overlap) in a new <c>PageSlotOf</c> helper - see
/// its own remarks in <c>CssLayoutEngineTable.cs</c> for the full mechanism.
/// </para>
/// <para>
/// The fixture repeats 42 body rows, not a smaller number, deliberately: with the duplicate-clone bug
/// fixed, the header-repeat loop's own known limitation (documented on the loop itself - a page reached
/// only because ITS OWN last row's straddle-correction pushed it there, with no LATER row's own start left
/// to notice the crossing, gets no repeat at all) still applies to whichever page the table's very LAST row
/// happens to land on. 42 rows leaves several trailing rows on the final page after that row, so a later
/// row's own start is what the loop actually observes the transition through - the same mechanism a real,
/// longer document exercises in practice. This is why <see cref="ClippedRepeatedHeader_IsPaintedOnEveryPageItRepeatsOn"/>
/// and its sibling do not also serve as a regression test for that separate, still-open limitation.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class RepeatedTableHeaderClipIntegrationTests
{
    private const double PageHeight = 200;
    private const double Margin = 10;

    /// <summary>
    /// A table long enough to repeat its header on several pages, whose header cell clips its own
    /// content via an inner <c>overflow:hidden</c> div - mirroring PeachPDF's own fixture shape, where
    /// the clipped text has to sit in a box of its own below the clipping div (the walk starts at the
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
