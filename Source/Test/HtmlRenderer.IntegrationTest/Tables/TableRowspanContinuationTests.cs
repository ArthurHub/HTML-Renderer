using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// A <c>rowspan</c> cell whose ending row is moved to the next page (css-tables-3 §6.1) extends its bottom
/// edge to cover the gap that move opens up.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class TableRowspanContinuationTests
{
    private const double PageHeight = 300;
    private const double Margin = 20;

    private static CssBox FindByHtmlId(CssBox root, string id) => LayoutHarness.FindById(root, id)!;

    private static CssBox RowOf(CssBox cell) => cell.ParentBox!;

    // Four 40px rows, then a row opening a two-row span, then a 120px row that ENDS it and would land
    // across the band boundary [20, 300) on its own. The spanning cell is deliberately the FIRST cell of
    // its opening row (column 0), and the ending row's own real cell comes after it: a separate
    // gap in CssLayoutEngineTable.InsertEmptyBoxes means a CssSpacingBox placeholder is only ever inserted into a later row by walking that row's
    // OWN EXISTING cells looking for a matching column - so a spanning cell whose column sits at or past the
    // ending row's own last existing cell gets NO placeholder there at all, and neither the row-shift's
    // ActualBottom-extension nor anything else that reaches the span through its placeholder ever fires.
    // Column 0 always matches on the very first existing cell (or trivially, if the row is otherwise empty),
    // so it sidesteps the gap rather than exercising it - this file is about the row-shift/extension
    // mechanism downstream of a placeholder existing, not about InsertEmptyBoxes' own column-matching gap.
    private static string EndingRowWouldStraddle() => LayoutHarness.Wrap(
        "<table style='width:100%;border-collapse:collapse'>"
        + "<tr><td colspan='2'><div style='height:40px'>row 0</div></td></tr>"
        + "<tr><td colspan='2'><div style='height:40px'>row 1</div></td></tr>"
        + "<tr><td colspan='2'><div style='height:40px'>row 2</div></td></tr>"
        + "<tr><td colspan='2'><div style='height:40px'>row 3</div></td></tr>"
        + "<tr><td id='span' rowspan='2'><div id='spanContent' style='height:40px'>spans 4-5</div></td>"
        + "<td><div style='height:40px'>row 4</div></td></tr>"
        + "<tr><td id='r5'><div style='height:120px'>row 5</div></td></tr>"
        + "<tr><td colspan='2'><div style='height:40px'>row 6</div></td></tr>"
        + "</table>");

    private static string SpanInsideOneBand() => LayoutHarness.Wrap(
        "<table style='width:100%;border-collapse:collapse'>"
        + "<tr><td id='span' rowspan='2'><div id='spanContent' style='height:40px'>spans 0-1</div></td>"
        + "<td><div style='height:40px'>row 0</div></td></tr>"
        + "<tr><td id='r1'><div style='height:40px'>row 1</div></td></tr>"
        + "</table>");

    // The row that ends a rowspan is carried onto the next band whole, like any other row - it is not
    // exempted from css-tables-3 6.1's default row preservation just because a cell ends there.
    [TestMethod]
    public void ARowThatEndsARowspan_IsShiftedOntoTheNextBandLikeAnyOtherRow()
    {
        var (root, container) = LayoutHarness.Layout(EndingRowWouldStraddle(), 400, PageHeight, margin: Margin);

        // The row's first box is the CssSpacingBox placeholder (Display:none) for the span, whose own
        // Location is never touched by the shift (only its ExtendedBox's ActualBottom is - see the
        // remarks above) - so "where the row now starts" has to be read off its real cell, r5.
        var r5 = FindByHtmlId(root, "r5");

        Assert.AreEqual(container.PageTopOf(1), r5.Location.Y, 0.5,
            "the row ending the span should begin the band the shift opened, rather than straddling");
    }

    // The confirmed bugfix (RowspanCellShiftTest.cs), pinned again here deterministically: the spanning
    // cell's own ActualBottom extends by the same delta the row shift applies, tracking the shift rather
    // than being left stale.
    [TestMethod]
    public void TheSpanningCellsBottom_ExtendsToCoverTheGapTheShiftOpened()
    {
        var (root, container) = LayoutHarness.Layout(EndingRowWouldStraddle(), 400, PageHeight, margin: Margin);

        var span = FindByHtmlId(root, "span");
        var endingRow = RowOf(FindByHtmlId(root, "r5"));

        Assert.AreEqual(endingRow.Boxes.Max(b => b.ActualBottom), span.ActualBottom, 0.5,
            "the spanning cell must close level with its row's other real cell(s), not short of them");
    }

    // The spanning cell's own top is anchored to the earlier row it actually started in - the row-shift
    // that later extends its bottom (triggered by the ENDING row, several rows later) must never move it.
    // Pinned against its own opening row's plain sibling cell: both start on row 4, and only the spanning
    // cell's bottom is later touched by row 5's shift - if the shift also moved the cell's top, the two
    // would disagree.
    [TestMethod]
    public void TheSpanningCellsTop_IsUnaffectedByTheShift()
    {
        var (root, _) = LayoutHarness.Layout(EndingRowWouldStraddle(), 400, PageHeight, margin: Margin);

        var span = FindByHtmlId(root, "span");
        var openingRow = RowOf(span);
        var plainSibling = openingRow.Boxes.Single(b => !ReferenceEquals(b, span));

        Assert.AreEqual(plainSibling.Location.Y, span.Location.Y, 0.5,
            "the spanning cell's top should still be flush with its opening row's plain sibling cell");

        // And well clear of the shifted bottom - the top never travelled down to meet it.
        Assert.IsTrue(span.ActualBottom - span.Location.Y > 100,
            $"expected the cell's extended height to clearly separate its top ({span.Location.Y:F1}) " +
            $"from its shifted bottom ({span.ActualBottom:F1})");
    }

    // The control: a span comfortably inside one band is untouched by the row-preservation mechanism -
    // without this, "the cell was extended" would pass against a change that extended every cell.
    [TestMethod]
    public void ASpanInsideOneBand_IsNotExtended()
    {
        var (root, _) = LayoutHarness.Layout(SpanInsideOneBand(), 400, 2000, margin: Margin);

        var span = FindByHtmlId(root, "span");
        var row1 = RowOf(FindByHtmlId(root, "r1"));

        // Still stretched to the bottom of the row it ends on (ordinary rowspan behavior, unrelated to
        // pagination), and no further.
        Assert.AreEqual(row1.Boxes.Max(b => b.ActualBottom), span.ActualBottom, 0.5);
    }

    // A row can end more than one span, and each of the ending cells is extended - the mechanism must not
    // stop at the first one.
    [TestMethod]
    public void ARowEndingTwoSpans_ExtendsBothOfThem()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<tr><td colspan='3'><div style='height:40px'>row 0</div></td></tr>"
            + "<tr><td colspan='3'><div style='height:40px'>row 1</div></td></tr>"
            + "<tr><td colspan='3'><div style='height:40px'>row 2</div></td></tr>"
            + "<tr><td colspan='3'><div style='height:40px'>row 3</div></td></tr>"
            + "<tr><td id='left' rowspan='2'>spans 4-5</td>"
            + "<td id='right' rowspan='2'>also spans 4-5</td>"
            + "<td><div style='height:40px'>row 4</div></td></tr>"
            + "<tr><td id='r5'><div style='height:120px'>row 5</div></td></tr>"
            + "</table>");

        var (root, _) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        var left = FindByHtmlId(root, "left");
        var right = FindByHtmlId(root, "right");
        var endingRow = RowOf(FindByHtmlId(root, "r5"));
        var expectedBottom = endingRow.Boxes.Max(b => b.ActualBottom);

        Assert.AreEqual(expectedBottom, left.ActualBottom, 0.5);
        Assert.AreEqual(expectedBottom, right.ActualBottom, 0.5);
    }

    // A spanning cell's own descendant content is never displaced by the extension - only the box's outer
    // ActualBottom edge moves, matching TheSpanningCellsTop_IsUnaffectedByTheShift's own finding for the
    // cell's top.
    [TestMethod]
    public void TheSpanningCellsContent_SurvivesTheExtensionUnmoved()
    {
        var (root, _) = LayoutHarness.Layout(EndingRowWouldStraddle(), 400, PageHeight, margin: Margin);

        var content = LayoutHarness.FindById(root, "spanContent")!;
        var words = LayoutHarness.Descendants(content).SelectMany(b => b.Words).Select(w => w.Text).ToList();

        CollectionAssert.Contains(words, "spans");
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#breaking-rules")]
    public void AForcedBreakOnARowInsideASpan_FragmentsTheCellAtTheDeclaredRow()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>"
            + "<tr><td><div style='height:40px'>row 0</div></td><td id='span' rowspan='3'>spans 0-2</td></tr>"
            + "<tr id='forced' style='break-before:page'><td><div style='height:40px'>row 1</div></td></tr>"
            + "<tr><td><div style='height:40px'>row 2</div></td></tr>"
            + "</table>");

        var (root, container) = LayoutHarness.Layout(html, 400, PageHeight, margin: Margin);

        var forced = LayoutHarness.FindById(root, "forced")!;
        Assert.AreEqual(1, container.PageIndexOf(forced.Boxes[0].Location.Y),
            "break-before:page on the row should force it onto the next page");
    }
}
