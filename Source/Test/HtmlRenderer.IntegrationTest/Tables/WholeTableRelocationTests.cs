using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// A table that didn't break between its own rows is monolithic content (css-break-3 §4.3), so a table with
/// <c>break-inside: avoid</c> that straddles a page boundary is moved whole rather than sliced.
/// </summary>
/// <remarks>
/// Without <c>break-inside: avoid</c> a table is never moved as a whole, only a straddling row is, so every
/// fixture declares it.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class WholeTableRelocationTests
{
    private const double PageHeight = 842;

    private static string Document(string tableMarkup, double spacerHeight, string extraCss = "") =>
        $$"""
        <!DOCTYPE html><html><head><style>
        body { margin: 0 }
        table { border-collapse: collapse; width: 100% }
        td, th { padding: 3px }
        {{extraCss}}
        </style></head><body>
        <div style='height: {{spacerHeight}}px'></div>
        {{tableMarkup}}
        </body></html>
        """;

    private static (CssBox Table, HtmlContainerInt Container, CssBox Root) Layout(string html)
    {
        var (root, container) = LayoutHarness.Layout(html, 595, PageHeight);
        var table = LayoutHarness.Descendants(root).First(b => b.Display == "table");
        return (table, container, root);
    }

    // RelocateIfNeeded always sees the table's real, already-measured height, so a single-row table with tall cell content
    // is moved whole exactly like any other straddling break-inside:avoid box.
    [TestMethod]
    public void ATallSingleRowTable_IsMovedWholeOnceItsHeightIsKnown()
    {
        var (table, _, _) = Layout(Document(
            "<table style='break-inside:avoid'><tr><td><div style='height:400px'>tall content</div></td></tr></table>",
            spacerHeight: 500));

        Assert.AreEqual(PageHeight, table.Location.Y, 1);
    }

    // The move places the table at the page's own content top, and nothing inside the engine may nudge it
    // off there afterwards - GetVerticalSpacing() is -1 for a collapsed-border table, but RelocateIfNeeded's
    // own target (container.PageTopOf(topSlot + 1)) is computed independently of the table's internal row
    // cursor, so this stays exact regardless of that offset.
    [TestMethod]
    public void ARelocatedCollapsedBorderTable_IsNotNudgedPastThePageTop()
    {
        var (table, container, _) = Layout(Document(
            "<table style='break-inside:avoid'><tr><td><div style='height:400px'>tall content</div></td></tr></table>",
            spacerHeight: 500));

        Assert.AreEqual(container.PageTopOf(1), table.Location.Y, 1);
    }

    // A table taller than a whole page cannot be helped by moving it: RelocateIfNeeded declines outright
    // once height >= container.PageSize.Height, leaving it where flow put it.
    [TestMethod]
    public void ATableTallerThanOnePage_IsLeftWhereFlowPutIt()
    {
        var (table, _, _) = Layout(Document(
            "<table style='break-inside:avoid'><tr><td><div style='height:900px'>tall content</div></td></tr></table>",
            spacerHeight: 500));

        Assert.IsTrue(table.Location.Y < PageHeight,
            $"expected the table to stay on page 1 but it is at Y={table.Location.Y:F1}");
    }

    // A table that fragments between two of its own rows (css-tables-3 6.1's per-row preservation, not
    // this whole-table mover) straddles a boundary too - but RelocateIfNeeded's own "fits on no single
    // page" guard (its total height clearly exceeds one page) declines to move it.
    [TestMethod]
    public void ATableThatBrokeBetweenItsOwnRows_IsNotMoved()
    {
        var (table, _, _) = Layout(Document(
            "<table style='break-inside:avoid'>" + string.Concat(Enumerable.Range(1, 40).Select(i =>
                $"<tr><td><div style='height:30px'>row {i}</div></td></tr>")) + "</table>",
            spacerHeight: 500));

        Assert.IsTrue(table.Location.Y < PageHeight,
            $"expected the fragmenting table to stay where it began but it is at Y={table.Location.Y:F1}");
    }

    // The move introduces a break between the table and whatever precedes it, so EnforceKeepWithNext
    // applies exactly as it does to every other relocation - break-after:avoid declared explicitly on the
    // heading (the UA h1-h6{break-after:avoid} default is @media print-scoped, and LayoutHarness's
    // WinFormsAdapter reports "screen" - see this repo's own StageD4RepeatedHeaderTest for the established
    // convention) chains the heading to the table and it travels too.
    [TestMethod]
    public void AnAvoidChainedHeading_TravelsWithTheMovedTable()
    {
        var (table, _, root) = Layout(Document(
            "<h2 id='h' style='break-after:avoid'>Heading</h2>"
            + "<table style='break-inside:avoid'><tr><td><div style='height:400px'>tall content</div></td></tr></table>",
            spacerHeight: 500,
            extraCss: "h2 { margin: 6px 0 }"));

        var heading = LayoutHarness.FindById(root, "h")!;

        Assert.IsTrue(heading.Location.Y >= PageHeight,
            $"the heading should have travelled with its table but it is at Y={heading.Location.Y:F1}");
        Assert.IsTrue(heading.Location.Y < table.Location.Y,
            "the heading must still precede the table it is chained to");
    }

    // The table engine rebuilds RepeatedHeaderRows on every layout, so the moved table gets no stale or
    // duplicate header.
    [TestMethod]
    public void ATableWithARepeatingHeader_IsMovedAndItsHeaderRepeatsCorrectlyAtTheNewPosition()
    {
        var (table, container, _) = Layout(Document(
            "<table style='break-inside:avoid'><thead style='break-inside:avoid'><tr><th>Head</th></tr></thead>"
            + "<tbody><tr><td><div style='height:400px'>tall content</div></td></tr></tbody></table>",
            spacerHeight: 500));

        Assert.AreEqual(PageHeight, table.Location.Y, 1);

        // The table's single body row comfortably fits alongside its header on the page it was moved to,
        // so there is nothing to repeat - not a stale, non-null list left behind by an earlier layout pass
        // at the table's pre-relocation position.
        Assert.IsNull(table.RepeatedHeaderRows);

        var headerCells = LayoutHarness.Descendants(table).Count(b => b.HtmlTag?.Name == "th");
        Assert.AreEqual(1, headerCells, "the header must appear exactly once, not duplicated by relocation");
    }

    // A table with a <tfoot> and no <thead> is moved like any other break-inside:avoid table.
    [TestMethod]
    public void ATableWithAFooterAndNoHeader_IsMovedToo()
    {
        var (table, _, _) = Layout(Document(
            "<table style='break-inside:avoid'><tfoot><tr><td>Foot</td></tr></tfoot>"
            + "<tbody><tr><td><div style='height:400px'>tall content</div></td></tr></tbody></table>",
            spacerHeight: 500));

        Assert.IsTrue(table.Location.Y >= PageHeight,
            $"the footer-only table should have moved to page 2 but it is at Y={table.Location.Y:F1}");
    }
}
