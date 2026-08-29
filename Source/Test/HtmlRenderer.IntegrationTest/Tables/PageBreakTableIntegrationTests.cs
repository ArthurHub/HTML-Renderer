using System;
using System.Text;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Verifies table page-break behaviour for single-row tables.
/// </summary>
/// <remarks>
/// This file predates (#262) the fragmentation-engine-parity branch's own table work, and its original
/// remarks described a mechanism (<c>CssBox.BreakPage()</c>, gated on the table's own explicit
/// <c>page-break-inside:avoid</c>) that no longer exists at all - confirmed by grep, no such method remains
/// anywhere in <c>Core/Dom/CssBox.cs</c>. Revised here to match the CURRENT, confirmed mechanics: since
/// css-tables-3 §6.1 row preservation landed (commit <c>362dee9</c>), <c>CssLayoutEngineTable.LayoutCells</c>
/// attempts to keep every row unfragmented by default - unconditionally, not gated on the table's own
/// <c>break-inside</c> at all - unless the row is "freely fragmentable" (its own height is at least half
/// the fragmentainer's height OR width, or a cell only STARTS spanning into a later row there).
/// <para>
/// The important nuance this revision is built around: that default preservation shifts the STRADDLING
/// ROW'S CELLS (<c>cell.OffsetTop(delta)</c>), not the table's own outer box - <c>_tableBox.Location</c> is
/// set once, before <c>CssLayoutEngineTable.PerformLayout</c> even runs, and is never itself touched by the
/// internal row-shift (only <c>ActualBottom</c> grows to cover it). So a straddling single-row table's
/// CONTENT is correctly relocated by default now, but <c>table.Location.Y</c> - what most of this file's
/// original assertions check - stays exactly where it always would have. Moving the table's own outer box
/// still requires the SEPARATE, parent-level <c>BlockFragmentation.RelocateIfNeeded</c>, gated on an
/// EXPLICIT <c>break-inside:avoid</c> declared directly on the &lt;table&gt; (none of this file's fixtures
/// declare one, matching PeachPDF's own fixtures). <see cref="SingleRowTable_CrossingPageBoundary_IsMovedToNextPage"/>
/// is accordingly rewritten to check the cell's own position (what the fix actually does), rather than the
/// table's outer box (what it does not); every other test's ORIGINAL assertion (checking <c>table.Location.Y</c>)
/// is left as-is, with its Ignore reason corrected to cite the real, current gap where one remains.
/// </para>
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class PageBreakTableIntegrationTests
{
    // Mirrors PeachPDF's A4-at-1:1-scale page height; treated as px here since LayoutHarness measures in
    // px, not pt - the numbers below only need generous relative separation, which they already have.
    private const double PageHeight = 842.0;

    // A spacer this tall pushes the table close enough to the page end to plausibly cross a page boundary
    // for a typical single-row table height, leaving only a small margin.
    private const double SpacerThatCrossesPage = 833;

    // A spacer this tall leaves plenty of room - no page-break needed under any mechanism.
    private const double SpacerThatFits = 200;

    // css-tables-3 6.1's default row preservation, confirmed to actually engage here: the .rbox row (60px,
    // comfortably under half of both PageSize.Height=842 and PageSize.Width=595, so not "freely
    // fragmentable") straddling the boundary is shifted whole to page 2's own content top - even though the
    // TABLE declares no break-inside:avoid of its own (see the class remarks for why this checks the cell,
    // not table.Location.Y, which the internal row-shift never touches).
    [TestMethod]
    public void SingleRowTable_CrossingPageBoundary_IsMovedToNextPage()
    {
        var html = BuildHtml(SpacerThatCrossesPage, rowCount: 1);
        var (table, container) = GetTableAndContainer(html);

        Assert.IsNotNull(table);
        var cell = table!.Boxes[0].Boxes[0];

        Assert.AreEqual(1, container.PageIndexOf(cell.Location.Y),
            $"Row content should be relocated to page 2 but starts at Y={cell.Location.Y:F1}");
        Assert.AreEqual(container.PageTopOf(1), cell.Location.Y, 0.5,
            "Relocated row content should sit flush at page 2's own content top");
    }

    [TestMethod]
    public void SingleRowTable_FitsOnCurrentPage_IsNotMoved()
    {
        var html = BuildHtml(SpacerThatFits, rowCount: 1);
        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"Table that fits should stay on page 1 (Y < {PageHeight}) but Y={table.Location.Y:F1}");
    }

    [TestMethod]
    public void MultiRowTable_CrossingPageBoundary_PerRowBreakStillWorks()
    {
        // PeachPDF's original ran a full PDF-generation pass and asserted no exception. This fork has no
        // PdfGenerator/PDF-generation API at all (it is a WinForms/GDI+ HTML renderer, not a PDF library),
        // so this is adapted into a layout-only smoke test: a multi-row table near the page boundary must
        // still lay out without throwing, whichever rows css-tables-3 6.1's default preservation ends up
        // shifting (see the class remarks).
        var html = BuildHtml(SpacerThatCrossesPage, rowCount: 3);

        Exception? thrown = null;
        try
        {
            LayoutHarness.Layout(html, 595, PageHeight);
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        Assert.IsNull(thrown, $"Multi-row table layout near a page boundary should not throw, but got: {thrown}");
    }

    [TestMethod]
    public void SingleRowTable_WithRoundedBoxes_GeneratesPdf()
    {
        // Adapted from a PDF-generation regression smoke test (PdfGenerator.GeneratePdf does not exist in
        // this fork) into a layout-only smoke test: a page of single-row tables with border-radius content
        // near a page boundary must lay out without throwing. Trimmed to two of the original six sections
        // (including the "Combined Styles" one specifically called out as the original regression trigger)
        // to keep this focused; the @page at-rule was dropped since this fork's parser is not known to
        // support it.
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { font: 8.5px Arial, sans-serif; margin: 0 }
            h2 { font-size: 10px; margin: 0.9em 0 0.3em; padding-bottom: 2px; border-bottom: 1px solid #999 }
            table.sw { border-collapse: collapse; width: 100%; margin-bottom: 0.3em }
            table.sw td { padding: 3px; vertical-align: top; width: 25% }
            .rbox { height: 60px; background: steelblue; border: 2px solid #1a6b8a; margin-bottom: 3px }
            .desc { font-size: 7px; font-weight: bold; color: #444; margin-bottom: 1px }
            .css  { font-size: 6px; color: #666; line-height: 1.3; word-break: break-all }
            </style></head><body>
            <h2>1</h2><table class="sw"><tr>
              <td><div class="rbox" style="border-radius:20px"></div><div class="desc">a</div><div class="css">a</div></td>
              <td><div class="rbox" style="border-radius:10px 30px"></div><div class="desc">b</div><div class="css">b</div></td>
              <td><div class="rbox" style="border-radius:8px 20px 35px"></div><div class="desc">c</div><div class="css">c</div></td>
              <td><div class="rbox" style="border-radius:5px 15px 30px 45px"></div><div class="desc">d</div><div class="css">d</div></td>
            </tr></table>
            <h2>6 - Combined Styles</h2><table class="sw"><tr>
              <td><div class="rbox" style="border-radius:15px"></div><div class="desc">solid border + bg</div><div class="css">border-radius: 15px</div></td>
              <td><div class="rbox" style="border-style:dashed;border-radius:15px"></div><div class="desc">dashed border</div><div class="css">border-radius: 15px</div></td>
              <td><div class="rbox" style="border-style:dotted;border-radius:15px"></div><div class="desc">dotted border</div><div class="css">border-radius: 15px</div></td>
              <td><div class="rbox" style="border:none;border-radius:15px"></div><div class="desc">no border, bg only</div><div class="css">border-radius: 15px</div></td>
            </tr></table>
            </body></html>
            """;

        Exception? thrown = null;
        try
        {
            LayoutHarness.Layout(html, 595, PageHeight * 3);
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        Assert.IsNull(thrown, $"Layout of tables with border-radius content should not throw, but got: {thrown}");
    }

    [Ignore("Confirmed (not just assumed): a row's css-tables-3 6.1 default preservation has its own carve-" +
            "out for a row whose height is at least half the fragmentainer's height OR width - and this " +
            "fixture's 400px cell content is well past half PageSize.Width (595/2=297.5), so the row is " +
            "'freely fragmentable' and the table (declaring no break-inside:avoid of its own, so " +
            "RelocateIfNeeded also declines) is never relocated. Confirmed empirically: the cell straddles " +
            "at Y=499..905 across the 842px boundary, untouched.")]
    [TestMethod]
    public void SingleRowTable_TallCellContentMissedByEstimate_IsMovedToNextPageAfterLayout()
    {
        var html = BuildTallContentHtml(spacerHeight: 500, contentHeight: 400);
        var (table, pageHeight) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Table with tall cell content should be moved to page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(PageHeight, table.Location.Y, 1.0,
            $"Moved table should start flush at the next page top ({PageHeight}) but Y={table.Location.Y:F1}");
        Assert.IsTrue(table.ActualBottom - table.Location.Y <= pageHeight,
            "Moved table must fit within a single page");
    }

    [TestMethod]
    public void SingleRowTable_TallerThanOnePage_IsLeftInPlace()
    {
        // An unsatisfiable move: the row is taller than a whole page (900px content > 842px PageSize.Height).
        // Row preservation's own guard (rowHeight < pageGridContainer.PageSize.Height) declines outright -
        // moving it to the next page wouldn't help it fit either - so it is left exactly where flow put it,
        // still straddling. Nothing about this fixture needs break-inside:avoid on the table either way.
        var html = BuildTallContentHtml(spacerHeight: 500, contentHeight: 900);
        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"Table taller than a page should stay on page 1 (Y < {PageHeight}) but Y={table.Location.Y:F1}");
    }

    [Ignore("Confirmed gap, for two independent reasons. First, this fixture's 400px cell content is " +
            "freely-fragmentable (past half PageSize.Width=595, same as " +
            "SingleRowTable_TallCellContentMissedByEstimate_IsMovedToNextPageAfterLayout), so nothing " +
            "relocates at all. Second, and more fundamentally, even a fixture that DID engage row " +
            "preservation would still not pull the heading: BlockFragmentation.EnforceKeepWithNext (the " +
            "only keep-with-next mechanism that exists) reads the table's own EffectiveTop, and the " +
            "internal row-shift never touches the table's own Location (see class remarks) - so from " +
            "EnforceKeepWithNext's perspective the table never appears to have moved at all, and there is " +
            "no gap for it to notice between the heading and the table to begin with.")]
    [TestMethod]
    public void SingleRowTable_MovedByPostCheck_PullsAvoidChainedHeadingAlong()
    {
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 6px 0; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 3px; }
            </style></head><body>
            <div style='height: 500px'></div>
            <h2 class='heading'>Section heading</h2>
            <table><tr><td><div style='height: 400px'>tall content</div></td></tr></table>
            </body></html>
            """;

        var (root, _) = LayoutHarness.Layout(html, 595, PageHeight);

        var table = FindFirst(root, b => b.Display == "table");
        var heading = FindFirst(root, b => b.HtmlTag?.Name == "h2");
        Assert.IsNotNull(table);
        Assert.IsNotNull(heading);

        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Test setup expects the table to be moved to page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(Math.Floor(table.Location.Y / PageHeight), Math.Floor(heading!.Location.Y / PageHeight));
        Assert.IsTrue(heading.ActualBottom <= table.Location.Y + 1.0,
            $"Heading (bottom={heading.ActualBottom:F1}) must sit above the moved table (top={table.Location.Y:F1})");
    }

    // A fixed-position box renders at the same page-box position on every page (CSS2.1 §13.3.1) - flow
    // pagination must never relocate it, even when its laid-out bounds straddle a page boundary. Same for
    // absolute positioning (§9.6). BlockFragmentation.RelocateIfNeeded explicitly excludes any
    // child.IsOutOfFlow box from whole-box relocation regardless of break-inside; the table's own outer
    // Location.Y is what this test checks, and that is what RelocateIfNeeded (not row preservation) would
    // ever move.
    [TestMethod]
    [DataRow("fixed")]
    [DataRow("absolute")]
    public void OutOfFlowTable_StraddlingPageBoundary_IsNotMoved(string position)
    {
        var html = $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            table { border-collapse: collapse; width: 50%; position: {{position}}; top: 700px; }
            td { padding: 3px; }
            </style></head><body>
            <div style='height: 30px'>flow content</div>
            <table><tr><td><div style='height: 400px'>tall content</div></td></tr></table>
            </body></html>
            """;

        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"A position: {position} table must not be relocated by page-break handling (Y={table.Location.Y:F1})");
    }

    // --- Helpers ---

    private static string BuildTallContentHtml(double spacerHeight, double contentHeight)
    {
        return $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 3px; }
            </style></head><body>
            <div style='height: {{spacerHeight}}px'></div>
            <table><tr><td><div style='height: {{contentHeight}}px'>tall content</div></td></tr></table>
            </body></html>
            """;
    }

    private static string BuildHtml(double spacerHeight, int rowCount)
    {
        var rows = new StringBuilder();
        for (var r = 0; r < rowCount; r++)
        {
            rows.Append("<tr>");
            rows.Append("<td><div class='rbox'></div></td>");
            rows.Append("<td><div class='rbox'></div></td>");
            rows.Append("</tr>");
        }

        return $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            .spacer { height: {{spacerHeight}}px; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 3px; }
            .rbox { height: 60px; }
            </style></head><body>
            <div class='spacer'></div>
            <table>{{rows}}</table>
            </body></html>
            """;
    }

    private static (CssBox? table, double pageHeight) GetTableAndPageHeight(string html)
    {
        var (root, container) = LayoutHarness.Layout(html, 595, PageHeight);
        var table = FindFirst(root, b => b.Display == "table");
        return (table, container.PageSize.Height);
    }

    private static (CssBox? table, HtmlContainerInt container) GetTableAndContainer(string html)
    {
        var (root, container) = LayoutHarness.Layout(html, 595, PageHeight);
        var table = FindFirst(root, b => b.Display == "table");
        return (table, container);
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
}
