using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Whether a repeated <c>&lt;thead&gt;</c> appears on pages a table spans without breaking between rows there,
/// because one row or cell runs through them.
/// </summary>
/// <remarks>
/// The header repeat is only checked at the start of a row, so a single row running through several pages
/// only gets the repeat on the first of them. These tests pin that.
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
        // One repeat per continuation page, the header's own first page is in flow, not a repeat.
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
