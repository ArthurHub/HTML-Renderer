using System;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// A heading with <c>break-after: avoid</c> isn't left alone at the bottom of a page while the table after it
/// starts on the next one.
/// </summary>
/// <remarks>
/// Keeping a row whole moves its cells, not the table's own box, so a header that fits on the page stays
/// there with the heading while the first body row moves on.
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

    [Ignore("Only the body row moves to the next page, moving the header and the heading along with it isn't implemented")]
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

    [Ignore("Only the first body row moves to the next page, moving the header and the heading along with it isn't implemented")]
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
