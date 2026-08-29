using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/TableRowBreakValueTests.cs: css-break-3 §3.1's forced breaks at
/// the class-A break point between two table rows.
/// </summary>
/// <remarks>
/// Confirmed gap, not a rename: <c>CssLayoutEngineTable.cs</c>'s row loop never reads
/// <c>BreakBefore</c>/<c>BreakAfter</c> anywhere - confirmed by grepping the whole file for both names (no
/// matches). The table engine's only page-break-related logic is the straddle-driven row-preservation
/// shift (css-tables-3 §6.1, unconditional-by-default per this fork's own commit history) and the
/// repeated-header loop; there is no forced-break handling of any kind for a <c>&lt;tr&gt;</c> or a row
/// group, unlike the general block layout path (<see cref="TheArtOfDev.HtmlRenderer.Core.Fragmentation.BlockFragmentation.TryGetForcedBreakTarget"/>),
/// which table rows never go through (they are laid out by <c>CssLayoutEngineTable.LayoutCells</c>'s own
/// manual per-cell loop, not the generic block child loop). 4 of PeachPDF's 5 tests are accordingly ported
/// with their full original assertions but <c>[Ignore]</c>d, citing this exact gap; only the one negative
/// control that holds regardless (no break value anywhere, nothing moves) is left active.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class TableRowBreakValueTests
{
    private const double PageHeight = 300;
    private const double Margin = 20;

    private static string Table(string rowCss, string extraRowAttrs = "") => LayoutHarness.Wrap(
        "<table id='t' style='width:100%'>"
        + "<tbody>"
        + "<tr id='r1'><td>Row one</td></tr>"
        + $"<tr id='r2' {extraRowAttrs}><td>Row two</td></tr>"
        + $"<tr id='r3' style='{rowCss}'><td>Row three</td></tr>"
        + "</tbody></table>");

    [TestMethod]
    public void WithoutABreakValue_EveryRowStaysOnTheFirstPage()
    {
        var (root, container) = LayoutHarness.Layout(Table(""), 400, PageHeight, margin: Margin);

        foreach (var id in new[] { "r1", "r2", "r3" })
        {
            var row = LayoutHarness.FindById(root, id)!;
            Assert.AreEqual(0, container.PageIndexOf(row.Boxes[0].Location.Y));
        }
    }

    [Ignore("Confirmed gap: CssLayoutEngineTable.cs's row loop never reads BreakBefore anywhere (grepped " +
        "the whole file - no matches), so break-before:page on a <tr> has no effect on where it lands.")]
    [TestMethod]
    public void BreakBeforeOnARow_StartsItOnTheNextPage()
    {
        var (root, container) = LayoutHarness.Layout(Table("break-before:page"), 400, PageHeight, margin: Margin);

        var r2 = LayoutHarness.FindById(root, "r2")!;
        var r3 = LayoutHarness.FindById(root, "r3")!;
        Assert.AreEqual(0, container.PageIndexOf(r2.Boxes[0].Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(r3.Boxes[0].Location.Y));
    }

    [Ignore("Confirmed gap: CssLayoutEngineTable.cs's row loop never reads BreakAfter anywhere (grepped the " +
        "whole file - no matches), so break-after:page on a <tr> has no effect on the row after it.")]
    [TestMethod]
    public void BreakAfterOnTheRowBefore_StartsTheNextOneOnTheNextPage()
    {
        var (root, container) = LayoutHarness.Layout(Table("", "style='break-after:page'"), 400, PageHeight, margin: Margin);

        var r2 = LayoutHarness.FindById(root, "r2")!;
        var r3 = LayoutHarness.FindById(root, "r3")!;
        Assert.AreEqual(0, container.PageIndexOf(r2.Boxes[0].Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(r3.Boxes[0].Location.Y));
    }

    [Ignore("Same confirmed gap as BreakBeforeOnARow_StartsItOnTheNextPage - a row group's own break-before " +
        "is no more read than an individual row's, since neither ever reaches CssLayoutEngineTable's forced-" +
        "break-free row loop.")]
    [TestMethod]
    public void BreakBeforeOnARowGroup_IsSeenAtItsFirstRow()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:100%'>"
            + "<tbody><tr id='r1'><td>Row one</td></tr></tbody>"
            + "<tbody style='break-before:page'><tr id='r2'><td>Row two</td></tr></tbody>"
            + "</table>"), 400, PageHeight, margin: Margin);

        var r1 = LayoutHarness.FindById(root, "r1")!;
        var r2 = LayoutHarness.FindById(root, "r2")!;
        Assert.AreEqual(0, container.PageIndexOf(r1.Boxes[0].Location.Y));
        Assert.AreEqual(1, container.PageIndexOf(r2.Boxes[0].Location.Y));
    }

    [Ignore("Same confirmed gap: break-before:page on a <tr> is never read at all, so there is no forced " +
        "break for the repeated-header loop to take part in taking.")]
    [TestMethod]
    public void AForcedRowBreak_StillRepeatsTheHeaderOnTheNewPage()
    {
        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:100%'>"
            + "<thead style='break-inside:avoid'><tr><th>Head</th></tr></thead>"
            + "<tbody><tr id='r1'><td>Row one</td></tr>"
            + "<tr id='r2' style='break-before:page'><td>Row two</td></tr></tbody>"
            + "</table>"), 400, PageHeight, margin: Margin);

        var r2 = LayoutHarness.FindById(root, "r2")!;
        Assert.AreEqual(1, container.PageIndexOf(r2.Boxes[0].Location.Y));
    }
}
