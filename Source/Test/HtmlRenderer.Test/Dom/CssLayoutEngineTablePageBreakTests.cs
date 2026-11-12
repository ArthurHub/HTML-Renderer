using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Dom;

[TestClass]
public sealed class CssLayoutEngineTablePageBreakTests
{
    // Guards against adding the top page margin twice.
    [TestMethod]
    public void PageBreakOffset_RowsOnSubsequentPages_StartAtCorrectY()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'><tbody>" +
            string.Concat(Enumerable.Range(1, 30).Select(i =>
                $"<tr><td style='border:1px solid black;padding:5px;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, pageHeight: 200, margin: 20);

        var rows = TableRows(root);
        Assert.IsTrue(rows.Count > 0);

        // Find the first row that starts on page 1 (slot >= 1) - i.e. past the first page's own band.
        var firstRowOnLaterPage = rows.FirstOrDefault(r => container.PageIndexOf(RowTop(r)) >= 1);
        Assert.IsNotNull(firstRowOnLaterPage, "table should span more than one page for this test to be meaningful");

        var slot = container.PageIndexOf(RowTop(firstRowOnLaterPage!));
        Assert.AreEqual(container.PageTopOf(slot), RowTop(firstRowOnLaterPage), 0.5,
            $"row starting page-slot {slot} should be flush at that page's own content top");
    }

    // Guards against leaving the top page margin out of the available height, which breaks one row too late.
    [TestMethod]
    public void AvailableHeight_PageBreakFiringPoint_RowDoesNotBleedIntoBottomMargin()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'><tbody>" +
            string.Concat(Enumerable.Range(1, 15).Select(i =>
                $"<tr><td style='border:1px solid black;padding:5px;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, pageHeight: 85, margin: 20);

        var rows = TableRows(root);
        Assert.IsTrue(rows.Count > 0);

        const double epsilon = 0.5;
        foreach (var row in rows)
        {
            var top = RowTop(row);
            var bottom = RowBottom(row);
            var slot = container.PageIndexOf(top);
            var contentBottom = container.PageTopOf(slot + 1);
            Assert.IsTrue(bottom <= contentBottom + epsilon,
                $"row at top={top} (slot {slot}) has bottom={bottom}, " +
                $"which bleeds past that slot's own content bottom {contentBottom}");
        }
    }

    // Regression test (adapted): across a whole multi-page table, no row may straddle a page's margin
    // band - it lands entirely within a single page's content band, or (css-tables-3 §6.1's own default)
    // is shifted whole onto the next page's content top rather than being sliced across the boundary.
    [TestMethod]
    public void TableLayout_MultiPageTable_RowsDoNotOverlapPageMargins()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'><tbody>" +
            string.Concat(Enumerable.Range(1, 20).Select(i =>
                $"<tr><td style='border:1px solid black;padding:5px;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, pageHeight: 260, margin: 20);

        var rows = TableRows(root);
        Assert.IsTrue(rows.Count > 1, "table should span more than one page for this test to be meaningful");

        const double epsilon = 0.5;
        foreach (var row in rows)
        {
            var top = RowTop(row);
            var bottom = RowBottom(row);
            var topSlot = container.PageIndexOf(top);
            var bottomSlot = container.PageIndexOf(System.Math.Max(top, bottom - epsilon));
            Assert.AreEqual(topSlot, bottomSlot,
                $"row [{top}, {bottom}] straddles a page boundary between slots " +
                $"{topSlot} and {bottomSlot} instead of being kept on one page or shifted whole to the next");
        }
    }

    // break-inside: avoid is set on <thead> because the UA rule for it is under @media print and MockAdapter
    // uses screen. The last page a table spans gets no repeated header, the check only runs at the start of a row.
    [TestMethod]
    public void RepeatedThead_ClonesOntoEveryContinuationPage_AtThePagesOwnContentTop()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'>" +
            "<thead style='break-inside:avoid;'><tr><th style='height:20px;padding:0;margin:0;'>Header</th></tr></thead>" +
            "<tbody>" +
            string.Concat(Enumerable.Range(1, 40).Select(i =>
                $"<tr><td style='height:20px;padding:0;margin:0;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, pageHeight: 100, margin: 0);

        var table = FindTableBox(root);
        Assert.IsNotNull(table);

        var lastSlot = container.FragmentTree!.Fragmentainers.Count - 1;
        Assert.IsTrue(lastSlot >= 3, "table should span at least 4 pages for this test to be meaningful");

        Assert.IsNotNull(table!.RepeatedHeaderRows);

        var clonedRowsInPageOrder = table.RepeatedHeaderRows!
            .OrderBy(RowTop)
            .ToList();

        // See the adaptation note above: slots 1..(lastSlot-1) get a repeat, not slot lastSlot itself.
        Assert.AreEqual(lastSlot - 1, clonedRowsInPageOrder.Count);

        for (var i = 0; i < clonedRowsInPageOrder.Count; i++)
        {
            var slot = i + 1; // continuation pages start at slot 1 (slot 0 has the header in flow already).
            Assert.AreEqual(container.PageTopOf(slot), RowTop(clonedRowsInPageOrder[i]), 0.5,
                $"repeated header clone for page-slot {slot} should sit at that page's own content top");
        }
    }

    // The clone carries the header's own cell text - TableHeaderRepeat.CloneSubtree's word-copying path
    // (Core/Fragmentation/TableHeaderRepeat.cs), not just an empty positioned box.
    [TestMethod]
    public void RepeatedThead_ClonedRowsCarryTheHeadersOwnCellText()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'>" +
            "<thead style='break-inside:avoid;'><tr><th style='border:1px solid black;padding:5px;'>HEADERMARKER</th></tr></thead>" +
            "<tbody>" +
            string.Concat(Enumerable.Range(1, 40).Select(i =>
                $"<tr><td style='border:1px solid black;padding:5px;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, pageHeight: 200, margin: 20);

        var table = FindTableBox(root);
        Assert.IsNotNull(table);
        Assert.IsNotNull(table!.RepeatedHeaderRows);
        Assert.IsTrue(table.RepeatedHeaderRows!.Count > 0);

        foreach (var clonedRow in table.RepeatedHeaderRows)
        {
            var text = string.Concat(LayoutHarness.Descendants(clonedRow).SelectMany(b => b.Words).Select(w => w.Text));
            StringAssert.Contains(text, "HEADERMARKER");
        }
    }

    // A table that fits entirely on one page has nothing to repeat - the header appears once, in flow,
    // and RepeatedHeaderRows stays null. Deliberately NOT border-collapse:collapse - see the dedicated
    // [Ignore]d test below for why that combination is a separate, narrower confirmed gap.
    [TestMethod]
    public void RepeatedThead_SinglePageTable_NoRepeatedHeaderRows()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;'>" +
            "<thead style='break-inside:avoid;'><tr><th style='height:20px;padding:0;margin:0;'>Header</th></tr></thead>" +
            "<tbody>" +
            string.Concat(Enumerable.Range(1, 3).Select(i =>
                $"<tr><td style='height:20px;padding:0;margin:0;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, _) = LayoutHarness.Layout(html, pageHeight: 2000, margin: 20);

        var table = FindTableBox(root);
        Assert.IsNotNull(table);
        Assert.IsNull(table!.RepeatedHeaderRows);
    }

    // Fixed, not just documented, as part of the fragmentation-engine-parity table batch: a
    // border-collapse:collapse table's row cursor (GetVerticalSpacing() is -1, a deliberate one-pixel
    // overlap between the first row and the table's own top border) starts one pixel below CssBox.ClientTop
    // whenever the table sits flush at a page's own content top. Fed straight into HtmlContainerInt's
    // PageIndexOf, that pixel used to floor into the slot BEFORE the one the table's box actually starts
    // in, which CssLayoutEngineTable.LayoutCells's repeated-header loop seeded "lastRepeatSlot" from - so
    // the very first body row read as having "advanced" a slot, and a header repeat was spuriously inserted
    // even though the table never left its own first page. Fixed at its source by the loop's new
    // PageSlotOf helper (CssLayoutEngineTable.cs), which clamps to ClientTop - see its own remarks for the
    // full mechanism, including why the fix is scoped to this loop alone and not the row-preservation
    // straddle check a few lines below it (a separate, unrelated caller of the same raw PageIndexOf call).
    [TestMethod]
    public void RepeatedThead_SinglePageBorderCollapseTable_PhantomHeaderRepeatDueToNegativeSlotRounding()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'>" +
            "<thead style='break-inside:avoid;'><tr><th style='height:20px;padding:0;margin:0;'>Header</th></tr></thead>" +
            "<tbody>" +
            string.Concat(Enumerable.Range(1, 3).Select(i =>
                $"<tr><td style='height:20px;padding:0;margin:0;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        var (root, container) = LayoutHarness.Layout(html, pageHeight: 2000, margin: 20);

        var table = FindTableBox(root);
        Assert.IsNotNull(table);
        Assert.AreEqual(0, container.PageIndexOf(table!.ActualBottom - 0.01), "table should genuinely fit on one page");
        Assert.IsNull(table.RepeatedHeaderRows);
    }

    // ── helpers ───────────────────────────────────────────────────────────

    // A <tr> box's own Location/ActualBottom are never assigned by the table layout row loop - only its
    // cells' are (see TableHeaderRepeat.CloneAndPosition's own doc comment, and CssLayoutEngineTable's
    // LayoutCells) - so "where a row is" has to be read off its own cells, not the row box itself.
    private static double RowTop(CssBox row) =>
        row.Boxes.Count > 0 ? row.Boxes[0].Location.Y : row.Location.Y;

    private static double RowBottom(CssBox row) =>
        row.Boxes.Count > 0 ? row.Boxes.Max(c => c.ActualBottom) : row.ActualBottom;

    private static CssBox? FindTableBox(CssBox box)
    {
        if (box.Display == TheArtOfDev.HtmlRenderer.Core.Utils.CssConstants.Table)
            return box;

        foreach (var child in box.Boxes)
        {
            var found = FindTableBox(child);
            if (found is not null) return found;
        }

        return null;
    }

    private static System.Collections.Generic.List<CssBox> TableRows(CssBox root)
    {
        var table = FindTableBox(root);
        Assert.IsNotNull(table);

        return LayoutHarness.Descendants(table!)
            .Where(b => b.Display == TheArtOfDev.HtmlRenderer.Core.Utils.CssConstants.TableRow)
            .ToList();
    }
}
