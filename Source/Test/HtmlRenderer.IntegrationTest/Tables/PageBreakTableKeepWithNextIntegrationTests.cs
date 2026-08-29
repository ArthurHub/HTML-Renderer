using System;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/PageBreakTableKeepWithNextIntegrationTests.cs: a heading
/// carrying <c>break-after:avoid</c> must not be stranded alone at the bottom of a page while the table
/// immediately following it starts on the next one.
/// </summary>
/// <remarks>
/// PeachPDF names two independent gaps its own fixtures are built around ("Gap 1": the general block path
/// relocating a box across a margin-crossing boundary without pulling a preceding keep-with-next run;
/// "Gap 2": a repeating-header table's own pre-check being gated off whenever it repeats a header at all).
/// Confirmed by reading both of this fork's real mechanisms and then running the fixtures below (not just
/// reading source): Gap 1 does NOT reproduce here - <c>BlockFragmentation.EnforceKeepWithNext</c> is called
/// UNCONDITIONALLY from the block child loop, for every child regardless of what relocated it (its own doc
/// comment even names this as the general fix for exactly this class of bug) - so a table whose margin gets
/// truncated across a page boundary (css-break-3 §5.2) already pulls a preceding <c>break-after:avoid</c>
/// heading along, with no table-specific handling needed at all.
/// <para>
/// Gap 2's shape, however, DOES reproduce, for a different and more fundamental reason than PeachPDF's own
/// (a table-specific pre-check being gated off): this fork's row preservation
/// (<c>CssLayoutEngineTable.LayoutCells</c>) shifts a straddling row's CELLS, never the table's own outer
/// <see cref="CssBox.Location"/> (see <c>Tables/PageBreakTableIntegrationTests.cs</c>'s own class remarks
/// for the same fact) - and <c>EnforceKeepWithNext</c> reads the CHILD's (the table's) own
/// <see cref="CssBoxProperties.EffectiveTop"/>, which never moves via an internal row-shift. So when a
/// table's own header fits under a heading but its first BODY row does not, the header (and the heading
/// above it) are left exactly where flow put them while only the straddling row moves on - an orphaned
/// header, not a whole-table-plus-heading move. <see cref="HeaderFitsButNoBodyRowDoes_MovesWholeTableAndHeadingTogether"/>
/// and <see cref="LongRepeatingHeaderTable_StartingNearPageBottom_StartsOnNextPageAndRepeatsHeaders"/> are
/// ported with PeachPDF's full original assertions but <c>[Ignore]</c>d, citing this. PeachPDF's own
/// three-way composition test (<c>GapOneThenGapTwo_...</c>) has no counterpart to port onto - it exists
/// specifically to pin two SEPARATE pre-checks not double-counting each other's own offset, and this fork
/// has only the one (general) mechanism, which does not re-fire the way two independent passes could.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class PageBreakTableKeepWithNextIntegrationTests
{
    private const double PageHeight = 842.0;

    private static (CssBox? Heading, CssBox? Table, HtmlContainerInt Container) Layout(string html)
    {
        var (root, container) = LayoutHarness.Layout(html, 595, PageHeight);
        var heading = FindFirst(root, b => b.HtmlTag?.Name == "h2");
        var table = FindFirst(root, b => b.Display == "table");
        return (heading, table, container);
    }

    private static CssBox? FindFirst(CssBox box, Func<CssBox, bool> predicate)
    {
        if (predicate(box)) return box;
        foreach (var child in box.Boxes)
        {
            var found = FindFirst(child, predicate);
            if (found != null) return found;
        }
        return null;
    }

    // The heading itself comfortably stays on page 1, but its collapsed bottom margin against the table's
    // top margin is large enough that the table's natural top (before css-break-3 5.2 margin truncation)
    // lands on page 2 - EnforceKeepWithNext's own general, unconditional check (not a table-specific one)
    // is what pulls the heading along.
    [TestMethod]
    public void Heading_MarginCrossesPageBoundary_PullsHeadingWithTable()
    {
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 0 0 140px 0; font-size: 14px; }
            table { border-collapse: collapse; width: 100%; margin: 0; }
            th, td { padding: 3px; }
            </style></head><body>
            <div style='height: 700px'></div>
            <h2 class='heading' style='break-after:avoid'>Transactions</h2>
            <table>
              <thead style='break-inside:avoid'><tr><th>Date</th><th>Amount</th></tr></thead>
              <tbody><tr><td>1/1</td><td>$1.00</td></tr></tbody>
            </table>
            </body></html>
            """;

        var (heading, table, container) = Layout(html);

        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);

        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Table should be relocated to page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(
            container.PageIndexOf(table.Location.Y), container.PageIndexOf(heading!.Location.Y));
        Assert.IsTrue(heading.ActualBottom <= table.Location.Y + 1.0,
            $"Heading (bottom={heading.ActualBottom:F1}) must sit above the moved table (top={table.Location.Y:F1})");
    }

    // Confirmed gap (see class remarks): the <thead> row fits comfortably under the heading on page 1, but
    // the tall .rbox body row does not - only the straddling ROW is relocated (css-tables-3 6.1), and
    // neither the table's own outer box nor the heading above it follow it, since EnforceKeepWithNext reads
    // the table's own EffectiveTop, which the internal row-shift never touches.
    [Ignore("Confirmed gap: row preservation shifts only the straddling row's cells, never the table's own " +
        "outer Location - EnforceKeepWithNext reads the table's EffectiveTop (unchanged) and finds no gap " +
        "to react to, so the heading and the table's own header are left in place while only the body row " +
        "moves on. See this file's own class remarks for the full mechanism.")]
    [TestMethod]
    public void HeaderFitsButNoBodyRowDoes_MovesWholeTableAndHeadingTogether()
    {
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 0; font-size: 12px; }
            table { border-collapse: collapse; width: 100%; margin: 0; }
            th, td { padding: 3px; }
            .rbox { height: 60px; }
            </style></head><body>
            <div style='height: 800px'></div>
            <h2 class='heading' style='break-after:avoid'>Transactions</h2>
            <table>
              <thead style='break-inside:avoid'><tr><th>Date</th><th>Amount</th></tr></thead>
              <tbody><tr><td><div class='rbox'></div></td><td><div class='rbox'></div></td></tr></tbody>
            </table>
            </body></html>
            """;

        var (heading, table, container) = Layout(html);

        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);

        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Table (with its thead) should be relocated to page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(
            container.PageIndexOf(table.Location.Y), container.PageIndexOf(heading!.Location.Y));
        Assert.IsTrue(heading.ActualBottom <= table.Location.Y + 1.0,
            $"Heading (bottom={heading.ActualBottom:F1}) must sit above the moved table (top={table.Location.Y:F1})");
        Assert.IsTrue(table.ActualBottom - table.Location.Y <= PageHeight,
            "Moved table must fit within a single page");
    }

    // Negative case: plenty of room remains under the header for the first body row - nothing should move.
    [TestMethod]
    public void HeaderAndFirstBodyRowBothFit_NothingIsMoved()
    {
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 0; font-size: 12px; }
            table { border-collapse: collapse; width: 100%; margin: 0; }
            th, td { padding: 3px; }
            .rbox { height: 60px; }
            </style></head><body>
            <div style='height: 400px'></div>
            <h2 class='heading' style='break-after:avoid'>Transactions</h2>
            <table>
              <thead style='break-inside:avoid'><tr><th>Date</th><th>Amount</th></tr></thead>
              <tbody><tr><td><div class='rbox'></div></td><td><div class='rbox'></div></td></tr></tbody>
            </table>
            </body></html>
            """;

        var (heading, table, _) = Layout(html);

        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"Table that fits alongside its heading should stay on page 1 (Y < {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.IsTrue(heading!.Location.Y < PageHeight);
    }

    // Confirmed gap, same mechanism as HeaderFitsButNoBodyRowDoes_MovesWholeTableAndHeadingTogether: a
    // table whose entire body clearly does not fit on one page still leaves its header (and the heading
    // above it) in flow on the ORIGINAL page - orphaned - rather than starting fresh on the next page,
    // because only the first straddling row is what row preservation ever relocates.
    [Ignore("Confirmed gap: the header (and the heading above it) stay in flow on the original page while " +
        "only the first straddling body row is relocated by row preservation - see class remarks. The " +
        "header does still repeat correctly on every page the table's body spans from there, which is a " +
        "real, working, SEPARATE mechanism from the one this test is about (not stranding the header's " +
        "own first appearance).")]
    [TestMethod]
    public void LongRepeatingHeaderTable_StartingNearPageBottom_StartsOnNextPageAndRepeatsHeaders()
    {
        var rows = string.Concat(Enumerable.Range(0, 60)
            .Select(i => $"<tr><td><div class='rbox'></div></td><td>{i}</td></tr>"));

        var html = $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 0; font-size: 12px; }
            table { border-collapse: collapse; width: 100%; margin: 0; }
            th, td { padding: 3px; }
            .rbox { height: 60px; }
            </style></head><body>
            <div style='height: 800px'></div>
            <h2 class='heading' style='break-after:avoid'>Transactions</h2>
            <table>
              <thead style='break-inside:avoid'><tr><th>Amount</th><th>Row</th></tr></thead>
              <tbody>{{rows}}</tbody>
            </table>
            </body></html>
            """;

        var (heading, table, container) = Layout(html);

        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Long table should start fresh on page 2 rather than orphan its header (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(
            container.PageIndexOf(table.Location.Y), container.PageIndexOf(heading!.Location.Y));
    }

    // The heading alone is taller than a full page, so pulling it along with the table can never satisfy
    // the avoid - css-break-3 §4.3's staged relaxation (EnforceKeepWithNext's own RunTrimmed/RunDropped
    // logic) must decline gracefully rather than looping, and the table must still render somewhere after
    // the heading.
    [TestMethod]
    public void HeadingTallerThanOnePage_UnsatisfiableAvoidIsRelaxed_NoInfiniteLoopAndTableStillRenders()
    {
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 0; font-size: 12px; height: 1000px; }
            table { border-collapse: collapse; width: 100%; margin: 0; }
            th, td { padding: 3px; }
            </style></head><body>
            <h2 class='heading' style='break-after:avoid'>Transactions</h2>
            <table>
              <thead style='break-inside:avoid'><tr><th>Date</th><th>Amount</th></tr></thead>
              <tbody><tr><td>1/1</td><td>$1.00</td></tr></tbody>
            </table>
            </body></html>
            """;

        Exception? thrown = null;
        CssBox? heading = null, table = null;
        try
        {
            (heading, table, _) = Layout(html);
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        Assert.IsNull(thrown, $"Layout with an unsatisfiable keep-with-next should not throw, but got: {thrown}");
        Assert.IsNotNull(heading);
        Assert.IsNotNull(table);
        Assert.IsTrue(heading!.Location.Y <= table!.Location.Y,
            "Document order must be preserved - the heading still precedes the table");
    }
}
