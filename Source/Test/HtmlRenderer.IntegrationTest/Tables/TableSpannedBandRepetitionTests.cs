using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/TableSpannedBandRepetitionTests.cs: whether a repeated
/// <c>&lt;thead&gt;</c> appears on bands a table spans WITHOUT breaking on them - a single row (or cell)
/// tall enough to overflow straight through one or more page bands, rather than a break falling neatly
/// between two rows.
/// </summary>
/// <remarks>
/// A drastic reduction from PeachPDF's 13 (thead half only, per the port plan's tfoot-drop rule - this
/// fork implements no <c>&lt;tfoot&gt;</c> repeat at all), not a rename - confirmed by direct source read
/// that almost all of the rest assert against machinery this fork does not have:
/// <list type="bullet">
/// <item><c>BoxFragment.OverflowClip</c> is <c>null</c> on every fragment this fork ever builds - confirmed
/// by grep: <c>Core/Fragmentation/FragmentEmitter.cs</c> constructs every <c>BoxFragment</c> with
/// <c>OverflowClip: null</c> literally, with no other assignment anywhere in the file. PeachPDF's own
/// "slice the row's graphical representation, leave room for the header, state the strip's confinement"
/// mechanism (<c>TheStripsMeetExactly_...</c>, <c>TheStripsCoverTheWholeRow_...</c>,
/// <c>EveryBandOfASlicedRun_...</c>, <c>AClipOutsideTheSlicedRow_...</c>) has nothing to port onto: this
/// fork's <see cref="TheArtOfDev.HtmlRenderer.Core.Fragmentation.FragmentEmitter"/> already fragments any
/// box taller than one band into one <see cref="TheArtOfDev.HtmlRenderer.Core.Fragments.BoxFragment"/> per
/// band it overlaps (proven generically by <c>Painting/FragmentPaintIntegrationTests.BoxSpanningTwoPages_PaintsItsBackgroundOnBoth</c>),
/// with no room-reservation step and no separate "confinement" object - so there is no "does the strip
/// begin below the header" question to ask; the header repeat and the tall row's own natural per-band
/// fragments simply overlap in painted space when the header-repeat loop's own known limitation (see
/// below) doesn't apply.</item>
/// <item>A quarter-of-the-page-height cap on repeat eligibility (css-tables-3 6.2's second condition) is
/// not implemented - confirmed by reading <c>CssLayoutEngineTable.LayoutCells</c>'s <c>repeatsHeader</c>
/// gate in full: it checks only <c>BreakValues.AvoidsBreak(_headerBox.BreakInside)</c>, no height
/// comparison of any kind. <c>TableRepeatedGroupConditionsTests</c> documents this gap; it is not
/// re-documented here.</item>
/// <item><c>AFixedBoxInsideASlicedRow_...</c> is subsumed by the already-ported, general
/// <c>Painting/FragmentPaintIntegrationTests.FixedBox_PaintsAtTheSameCoordinatesOnEveryPage</c> - nothing
/// about a fixed box's own repeat-per-page mechanism (<c>FragmentEmitter.CollectFixedRoots</c>) is
/// table-specific.</item>
/// <item><c>ARowspanTallerThanABand_DoesNotSliceTheRowThatEndsIt</c> is <c>TableRowspanContinuationTests</c>'
/// own subject (the rowspan-extension mechanism), not this file's.</item>
/// </list>
/// What remains and DOES port is the file's own real, confirmed subject: the loop's own documented
/// per-row-only slot check (<c>CssLayoutEngineTable.cs</c> ~634-645's own "KNOWN LIMITATION" remark) means
/// a table that breaks BETWEEN rows across several bands repeats its header on every one of them (the
/// common case), but a table whose header-repeat opportunity is reached only through a SINGLE row's own
/// straddle - one cell taller than the rows around it, overflowing through one or more bands with no later
/// row to trigger the next check - only ever gets the FIRST such band's repeat, never a later intermediate
/// one. Pinned here as the accurate, current behavior (a documented gap, not silently reproduced as if it
/// were correct) rather than Ignored, since it is exactly what the loop's own comment already promises.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class TableSpannedBandRepetitionTests
{
    private const double PageHeight = 300;
    private const double Margin = 20;

    private static CssBox TableOf(CssBox root) => LayoutHarness.Descendants(root).First(b => b.Display == "table");

    // The common case, already exercised elsewhere (StageD4RepeatedHeaderTest,
    // CssLayoutEngineTablePageBreakTests.RepeatedThead_ClonesOntoEveryContinuationPage...) - restated here
    // as this file's own control, since the next test's whole point is to show it does NOT generalize to
    // every shape that spans several bands.
    [TestMethod]
    public void ATableThatBreaksBetweenOrdinaryRows_RepeatsItsHeaderOnEveryBandItSpans()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<thead style='break-inside:avoid'><tr><th>Head</th></tr></thead><tbody>"
            + string.Concat(Enumerable.Range(1, 20).Select(i => $"<tr><td><div style='height:40px'>row {i}</div></td></tr>"))
            + "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages >= 3, $"fixture must span at least 3 pages, got {pages}");

        var table = TableOf(root);
        Assert.IsNotNull(table.RepeatedHeaderRows);
        // One repeat per continuation page (slot 1..pages-1) - the header's own first page is in flow, not
        // a repeat, matching this fork's own established, confirmed count convention.
        Assert.AreEqual(pages - 1, table.RepeatedHeaderRows!.Count);
    }

    // Confirmed, documented gap (see class remarks): a table whose only body row is a single cell tall
    // enough to overflow through several bands on its own gets the header repeat inserted for the FIRST
    // band it crosses onto, but not any later one - there is no subsequent row's own start left to trigger
    // the loop's per-row slot-advance check for those further bands.
    [TestMethod]
    public void ATallSingleRowTable_RepeatsHeaderOnlyOnTheFirstBandItOverflowsOnto()
    {
        // A trailing ordinary row after the tall one is load-bearing, not decorative: the loop's own
        // slot-advance check only runs at the START of the NEXT row's own iteration (see
        // CssLayoutEngineTable.cs ~654-686), so without one, the tall row's own straddle - detected only
        // when ITS iteration ends - has no later check left to notice it crossed into band 1 at all, and
        // RepeatedHeaderRows stays null outright rather than gaining even the first entry this test is
        // about. Confirmed empirically: a single tall row with nothing after it produces zero repeats, not
        // one - a stricter version of the very limitation this test pins.
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<thead style='break-inside:avoid'><tr><th>Head</th></tr></thead>"
            + "<tbody><tr><td><div style='height:900px'>tall</div></td></tr>"
            + "<tr><td><div style='height:20px'>trailing</div></td></tr></tbody>"
            + "</table>");

        var (root, container) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages >= 4, $"fixture must span at least 4 bands for this gap to be meaningful, got {pages}");

        var table = TableOf(root);
        Assert.IsNotNull(table.RepeatedHeaderRows);
        // Not (pages - 1): only the first continuation band gets a repeat, per the documented limitation -
        // the trailing row's own start only ever triggers ONE more slot-advance check, for whichever band
        // it itself landed in.
        Assert.AreEqual(1, table.RepeatedHeaderRows!.Count);
    }

    // A group whose author opts back out of the UA default (break-inside:auto) never repeats, however many
    // bands the table spans - the loop's gate is checked once, up front, and is unaffected by how the
    // table's content happens to be shaped.
    [TestMethod]
    public void AGroupOptedOutOfAvoidBreakInside_NeverRepeatsEvenWhenTheTableOverflows()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<thead style='break-inside:auto'><tr><th>Head</th></tr></thead>"
            + "<tbody><tr><td><div style='height:900px'>tall</div></td></tr></tbody>"
            + "</table>");

        var (root, container) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        var pages = container.FragmentTree!.Fragmentainers.Count;
        Assert.IsTrue(pages >= 3, $"fixture must span several bands, got {pages}");

        Assert.IsNull(TableOf(root).RepeatedHeaderRows);
    }

    // With no real page grid there is only ever one fragmentainer for the whole document - nothing to
    // repeat onto, regardless of content height.
    [TestMethod]
    public void WithNoRealPageGrid_NothingRepeats()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<thead style='break-inside:avoid'><tr><th>Head</th></tr></thead>"
            + "<tbody><tr><td><div style='height:900px'>tall</div></td></tr></tbody>"
            + "</table>");

        var (root, _) = LayoutHarness.Layout(html, 400, 4000);

        Assert.IsNull(TableOf(root).RepeatedHeaderRows);
    }
}
