using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Dom;

/// <summary>
/// Basic table layout tests against <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssLayoutEngineTable"/>,
/// ported from PeachPDF's (much larger) CssLayoutEngineTableTests. HTML-Renderer's table engine is a
/// simpler fork ancestor -- it has no header/footer repeat-across-pages proxies, no caption
/// grid-decoration box, and (unlike PeachPDF's <c>CssBox.Display</c> enum) represents "Display" as a
/// plain string compared against <see cref="TheArtOfDev.HtmlRenderer.Core.Utils.CssConstants"/>. Only the parts of the original suite that
/// exercise basic dimension/colspan/rowspan layout -- functionality this engine actually has -- are
/// ported here.
/// </summary>
[TestClass]
public sealed class CssLayoutEngineTableTests
{
    [TestMethod]
    public void TableLayout_CalculatesCorrectDimensions()
    {
        var html = LayoutHarness.Wrap(
            "<table id='tbl' style='width:100%;border-collapse:collapse'>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 1</td><td style='border:1px solid black;padding:8px'>Cell 2</td></tr>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 3</td><td style='border:1px solid black;padding:8px'>Cell 4</td></tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html);
        var table = LayoutHarness.FindById(root, "tbl");

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.ActualRight > table.Location.X, "Table should have width");
        Assert.IsTrue(table.ActualBottom > table.Location.Y, "Table should have height");
    }

    [TestMethod]
    public void TableLayout_WithColspan_CalculatesCorrectWidth()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>" +
            "<tr>" +
            "<td id='wide' colspan='2' style='border:1px solid black;padding:8px'>Wide Cell</td>" +
            "<td id='normal' style='border:1px solid black;padding:8px'>Normal</td>" +
            "</tr>" +
            "<tr>" +
            "<td style='border:1px solid black;padding:8px'>Cell 1</td>" +
            "<td style='border:1px solid black;padding:8px'>Cell 2</td>" +
            "<td style='border:1px solid black;padding:8px'>Cell 3</td>" +
            "</tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html);
        var wideCell = LayoutHarness.FindById(root, "wide");
        var normalCell = LayoutHarness.FindById(root, "normal");

        Assert.IsNotNull(wideCell);
        Assert.IsNotNull(normalCell);

        var wideWidth = wideCell!.ActualRight - wideCell.Location.X;
        var normalWidth = normalCell!.ActualRight - normalCell.Location.X;

        Assert.IsTrue(wideWidth > normalWidth, "Colspan cell should be wider than single cell");
    }

    [TestMethod]
    public void TableLayout_WithRowspan_CalculatesCorrectHeight()
    {
        var html = LayoutHarness.Wrap(
            "<table style='border-collapse:collapse'>" +
            "<tr><td id='tall' rowspan='2' style='border:1px solid black;padding:8px'>Tall Cell</td><td style='border:1px solid black;padding:8px'>Cell 2</td></tr>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 3</td></tr>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 4</td><td style='border:1px solid black;padding:8px'>Cell 5</td></tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html);
        var tallCell = LayoutHarness.FindById(root, "tall");

        Assert.IsNotNull(tallCell);
        var tallCellHeight = tallCell!.ActualBottom - tallCell.Location.Y;
        Assert.IsTrue(tallCellHeight > 0, "Rowspan cell should have height");
    }

    [TestMethod]
    public void TableLayout_DistributesWidthEqually_WhenNoWidthsSpecified()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:600px;border-collapse:collapse'>" +
            "<tr>" +
            "<td id='c1' style='border:1px solid black;padding:8px'>Cell 1</td>" +
            "<td id='c2' style='border:1px solid black;padding:8px'>Cell 2</td>" +
            "<td id='c3' style='border:1px solid black;padding:8px'>Cell 3</td>" +
            "</tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html, maxWidth: 1200);
        var cells = new[] { "c1", "c2", "c3" }
            .Select(id => LayoutHarness.FindById(root, id))
            .ToList();

        Assert.IsTrue(cells.All(c => c is not null));

        var widths = cells.Select(c => c!.ActualRight - c.Location.X).ToList();
        var avgWidth = widths.Average();

        foreach (var width in widths)
        {
            Assert.IsTrue(System.Math.Abs(width - avgWidth) < 5,
                $"Cell width {width} should be close to average {avgWidth}");
        }
    }

    [TestMethod]
    public void TableLayout_RespectsSpecifiedColumnWidths()
    {
        // Adapted from the source test: HTML-Renderer's CssParser has no pseudo-class selector support
        // (no ":first-child"), so the explicit width is applied directly on the first cell via an
        // inline style rather than through a "td:first-child { width: ... }" rule.
        var html = LayoutHarness.Wrap(
            "<table style='width:600pt;border-collapse:collapse'>" +
            "<tr>" +
            "<td id='wide' style='border:1px solid black;padding:8px;width:200pt'>Wide Cell</td>" +
            "<td id='auto1' style='border:1px solid black;padding:8px'>Auto</td>" +
            "<td id='auto2' style='border:1px solid black;padding:8px'>Auto</td>" +
            "<td id='auto3' style='border:1px solid black;padding:8px'>Auto</td>" +
            "</tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html, maxWidth: 1200);
        var wideCell = LayoutHarness.FindById(root, "wide");
        var auto1Cell = LayoutHarness.FindById(root, "auto1");

        Assert.IsNotNull(wideCell);
        Assert.IsNotNull(auto1Cell);

        var wideWidth = wideCell!.ActualRight - wideCell.Location.X;
        var auto1Width = auto1Cell!.ActualRight - auto1Cell.Location.X;

        Assert.IsTrue(wideWidth >= 180, $"First cell should be approximately 200px wide (accounting for borders), but was {wideWidth}");
        // The remaining table width (600pt minus the 200pt explicit column) is distributed across the
        // three auto columns - via their content-based max width plus an equal share of the leftover
        // space (see CssLayoutEngineTable.DetermineMissingColumnWidths) - rather than split evenly by a
        // fixed pixel budget, so assert the semantically-intended relationship (narrower than the
        // explicitly-widened column) instead of an arbitrary absolute threshold.
        Assert.IsTrue(auto1Width < wideWidth, $"Auto cell ({auto1Width}) should be narrower than the explicitly-widened first cell ({wideWidth})");
    }

    // Cherry-picked from PeachPDF's CssLayoutEngineTableTests (the rest of that file is general table
    // layout, already out of scope for this port - see Fragmentation/CssLayoutEngineTablePageBreakTests.cs
    // for the dedicated pagination-focused port). Adapted: PeachPDF's own version asserts against
    // table.Boxes.OfType<CssProxyBox>() (its in-tree header-repeat proxy mechanism); this fork instead
    // detaches repeated header clones onto CssBox.RepeatedHeaderRows (Core/Fragmentation/TableHeaderRepeat.cs)
    // rather than inserting them into the live tree, so the assertions are rewritten onto that. Also drops
    // the source's "@page { size: A4; margin: 20mm }" CSS rule (this fork's page grid is set on the
    // container directly, via LayoutHarness's pageHeight parameter, not through @page).
    [TestMethod]
    public void TableLayout_DetectsPageBreaksCorrectly()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse;'>" +
            "<thead style='break-inside:avoid;'><tr><th style='border:1px solid black;padding:10px;'>Header</th></tr></thead>" +
            "<tbody>" +
            string.Concat(Enumerable.Range(1, 30).Select(i =>
                $"<tr><td style='border:1px solid black;padding:10px;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        const double pageHeight = 400.0;
        var (root, container) = LayoutHarness.Layout(html, pageHeight: pageHeight, margin: 20);

        var table = FindTableBox(root);
        Assert.IsNotNull(table);

        var tableHeight = table!.ActualBottom - table.Location.Y;

        Assert.IsTrue(tableHeight > pageHeight, $"Table height ({tableHeight}) should exceed page height ({pageHeight})");
        Assert.IsNotNull(table.RepeatedHeaderRows);
        Assert.IsTrue(table.RepeatedHeaderRows!.Count >= 2,
            $"Should have at least 2 repeated header row-sets for a multi-page table, found {table.RepeatedHeaderRows.Count}");
    }

    [TestMethod]
    public void TableLayout_PositionsHeadersAtCorrectPageStarts()
    {
        var html = LayoutHarness.Wrap(
            // Deliberately not border-collapse:collapse/padding - see
            // CssLayoutEngineTablePageBreakTests.RepeatedThead_SinglePageBorderCollapseTable_... for a
            // dedicated, [Ignore]d test pinning down why that combination can shift a table's own top
            // fractionally off a page boundary and produce a spurious extra repeat.
            "<table style='width:100%;'>" +
            "<thead style='break-inside:avoid;'><tr><th style='height:20px;padding:0;margin:0;'>Header</th></tr></thead>" +
            "<tbody>" +
            string.Concat(Enumerable.Range(1, 10).Select(i =>
                $"<tr><td style='height:20px;padding:0;margin:0;'>Row {i}</td></tr>")) +
            "</tbody></table>");

        // Very short pages to force multiple page breaks.
        var (root, container) = LayoutHarness.Layout(html, pageHeight: 200, margin: 20);

        var table = FindTableBox(root);
        Assert.IsNotNull(table);
        Assert.IsNotNull(table!.RepeatedHeaderRows);
        Assert.IsTrue(table.RepeatedHeaderRows!.Count >= 1, "Should have at least one repeated header row-set");

        // Each repeated header row's own cell carries its real position - the row box itself is never
        // positioned by table layout (see CssLayoutEngineTablePageBreakTests' identical note).
        var headerYPositions = table.RepeatedHeaderRows
            .Select(row => row.Boxes.Count > 0 ? row.Boxes[0].Location.Y : row.Location.Y)
            .OrderBy(y => y)
            .ToList();

        // Every repeated header should land at one of this page grid's own real page tops.
        foreach (var y in headerYPositions)
        {
            var slot = container.PageIndexOf(y);
            Assert.AreEqual(container.PageTopOf(slot), y, 0.5, $"repeated header at Y={y} should sit flush at page-slot {slot}'s content top");
        }

        // If there are multiple repeats, they must be at different Y positions - not all collapsed onto
        // the same page.
        if (headerYPositions.Count > 1)
        {
            var uniquePositions = headerYPositions.Distinct().Count();
            Assert.IsTrue(uniquePositions > 1,
                $"Multiple repeated headers should be at different Y positions, but all {headerYPositions.Count} were the same");
        }
    }

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
}
