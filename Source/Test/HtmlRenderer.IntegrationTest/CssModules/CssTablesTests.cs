using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssTablesTests
{
    private const double Delta = 0.5;

    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    private static double Width(CssBox box) => box.ActualRight - box.Location.X;

    private static double Height(CssBox box) => box.ActualBottom - box.Location.Y;

    private static bool PaintsColor(RecordingGraphics g, RColor color) =>
        g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == color)
        || g.Log.OfType<RecordingGraphics.DrawLineCall>().Any(l => l.Color == color)
        || g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Any(p => p.Color == color);

    [TestMethod]
    [Spec("css-tables-3", "#fixup-boxes")]
    public void AnonymousCell_HasNoPaddingBorderOrBackground()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table;border-spacing:0'><div id='r' style='display:table-row'><span id='s'>text</span></div></div>"));

        var cell = LayoutHarness.FindById(root, "r")!.Boxes.Single();
        var word = LayoutHarness.Descendants(cell).SelectMany(b => b.Words).First();

        Assert.IsNull(cell.HtmlTag);
        Assert.AreEqual(0, cell.ActualPaddingLeft, Delta);
        Assert.AreEqual(0, cell.ActualBorderLeftWidth, Delta);
        Assert.AreEqual(RColor.Transparent, cell.ActualBackgroundColor);
        Assert.AreEqual(cell.Location.X, word.Left, Delta);
    }

    [TestMethod]
    [Spec("css-tables-3", "#table-layout-algorithm")]
    public void EmptyTable_GetsItsSpecifiedWidthAndHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='width:100px;height:50px;border-spacing:0'></table>"));

        var t = LayoutHarness.FindById(root, "t")!;

        Assert.AreEqual(100, Width(t), 1);
        Assert.AreEqual(50, Height(t), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#dimensioning-the-row-column-grid--step1")]
    public void ColspanOnANonCellElement_IsIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table;border-spacing:0'>"
            + "<div style='display:table-row'><div id='a' style='display:table-cell' colspan='2'>a</div></div>"
            + "<div style='display:table-row'><div id='b' style='display:table-cell;width:50px'>b</div><div id='c' style='display:table-cell;width:50px'>c</div></div>"
            + "</div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "b")!.ActualRight, LayoutHarness.FindById(root, "a")!.ActualRight, Delta);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#dimensioning-the-row-column-grid--step2")]
    public void ColumnsSpannedByTheSameCells_AreMergedIntoOne()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='border-spacing:10px'>"
            + "<tr><td id='a' colspan='2' style='width:50px;padding:0'>a</td></tr>"
            + "<tr><td colspan='2' style='padding:0'>b</td></tr></table>"));

        var t = LayoutHarness.FindById(root, "t")!;
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(50, Width(a), 1);
        Assert.AreEqual(70, Width(t), 1);
    }

    [TestMethod]
    [Spec("css-tables-3", "#missing-cells-fixup")]
    public void ShortRow_LeavesTheMissingSlotsEmpty()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='border-spacing:0'>"
            + "<tr><td id='a' style='width:40px;padding:0'>a</td><td id='b' style='width:60px;padding:0'>b</td></tr>"
            + "<tr><td id='c' style='padding:0'>c</td></tr></table>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var c = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(a.Location.X, c.Location.X, Delta);
        Assert.AreEqual(Width(a), Width(c), Delta);
        Assert.AreEqual(LayoutHarness.FindById(root, "b")!.ActualRight, LayoutHarness.FindById(root, "t")!.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css-tables-3", "#border-spacing-property")]
    public void BorderSpacing_IsZeroByDefault()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table'><div style='display:table-row'>"
            + "<div id='a' style='display:table-cell'>a</div><div id='b' style='display:table-cell'>b</div></div></div>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "a")!.ActualRight, LayoutHarness.FindById(root, "b")!.Location.X, Delta);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#border-spacing-property")]
    public void NegativeBorderSpacing_IsInvalid()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:4px;border-spacing:-5px'><tr><td id='a'>a</td><td id='b'>b</td></tr></table>"));

        Assert.AreEqual(4, LayoutHarness.FindById(root, "b")!.Location.X - LayoutHarness.FindById(root, "a")!.ActualRight, Delta);
    }

    [TestMethod]
    [Spec("css-tables-3", "#global-style-overrides")]
    public void RowPadding_IsIgnored()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='border-spacing:0'><tr style='padding:20px'><td id='a' style='padding:0'>a</td></tr></table>"));

        var t = LayoutHarness.FindById(root, "t")!;
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(t.Location.X, a.Location.X, Delta);
        Assert.AreEqual(t.Location.Y, a.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-tables-3", "#collapsed-style-overrides")]
    public void CollapsedTable_IgnoresItsPadding()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='padding:30px'><table id='t' style='border-collapse:collapse;padding:20px'><tr><td id='a' style='padding:0'>a</td></tr></table></div>"));

        var t = LayoutHarness.FindById(root, "t")!;
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual(t.Location.X, a.Location.X, 1);
        Assert.AreEqual(t.Location.Y, a.Location.Y, 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#collapsed-style-overrides")]
    public void CollapsedTable_IgnoresTheCellBorderRadius()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse'><tr><td style='border-radius:10px;background-color:rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#computing-cell-measures")]
    public void CellMinWidth_IsAMinimumForItsColumn()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td id='a' style='min-width:150px;padding:0'>a</td><td>b</td></tr></table>"));

        Assert.AreEqual(150, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [TestMethod]
    [Spec("css-tables-3", "#computing-cell-measures")]
    public void BorderBoxCellWidth_IncludesPaddingAndBorder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table;border-spacing:0'><div style='display:table-row'>"
            + "<div id='a' style='display:table-cell;box-sizing:border-box;width:100px;padding:10px;border:2px solid'>a</div></div></div>"));

        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#computing-cell-measures")]
    public void ContentBoxCellWidth_ExcludesPaddingAndBorder()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='display:table;border-spacing:0'><div style='display:table-row'>"
            + "<div id='a' style='display:table-cell;width:100px;padding:10px;border:2px solid'>a</div></div></div>"));

        Assert.AreEqual(124, Width(LayoutHarness.FindById(root, "a")!), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#computing-column-measures")]
    public void SpanningCell_WidensTheColumnsInProportionToTheirMaxContent()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'>"
            + "<tr><td id='a' style='padding:0'>x</td><td id='b' style='padding:0'>xxxxxxxxxxxxxxxxxxxx</td></tr>"
            + "<tr><td colspan='2' style='width:400px;padding:0'>y</td></tr></table>"));

        Assert.IsGreaterThan(3 * Width(LayoutHarness.FindById(root, "a")!), Width(LayoutHarness.FindById(root, "b")!));
    }

    [TestMethod]
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void AutoWidth_IsLimitedToTheContainingBlock()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='width:200px'><table id='t'><tr><td>some text that is a lot wider than two hundred pixels when it is not wrapped</td></tr></table></div>"));

        Assert.AreEqual(200, Width(LayoutHarness.FindById(root, "t")!), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#computing-the-table-width")]
    public void MinWidth_IsAMinimumForAnAutoWidthTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='min-width:300px'><tr><td>a</td></tr></table>"));

        Assert.AreEqual(300, Width(LayoutHarness.FindById(root, "t")!), 1);
    }

    [TestMethod]
    [Spec("css-tables-3", "#width-distribution-algorithm")]
    [Spec("css-tables-3", "#distributing-width-to-columns")]
    public void PercentAndPixelColumns_GetTheirWidth_AutoColumnsTheRest()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:400px;border-spacing:0'><tr>"
            + "<td id='a' style='width:25%;padding:0'>a</td><td id='b' style='width:50px;padding:0'>b</td><td id='c' style='padding:0'>c</td></tr></table>"));

        Assert.AreEqual(100, Width(LayoutHarness.FindById(root, "a")!), 1);
        Assert.AreEqual(50, Width(LayoutHarness.FindById(root, "b")!), 1);
        Assert.AreEqual(250, Width(LayoutHarness.FindById(root, "c")!), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#distributing-width-to-columns")]
    public void ExcessWidth_GoesToAutoColumnsInProportionToTheirMaxContent()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='width:600px;border-spacing:0'><tr>"
            + "<td id='a' style='padding:0'>x</td><td id='b' style='padding:0'>xxxxxxxxxxxxxxxxxxxx</td></tr></table>"));

        Assert.IsGreaterThan(5 * Width(LayoutHarness.FindById(root, "a")!), Width(LayoutHarness.FindById(root, "b")!));
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#height-distribution-algorithm")]
    public void ExtraTableHeight_IsDistributedOverTheRows()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='height:200px;border-spacing:0'>"
            + "<tr><td id='a' style='padding:0'>a</td></tr><tr><td id='b' style='padding:0'>b</td></tr></table>"));

        Assert.AreEqual(100, Height(LayoutHarness.FindById(root, "a")!), 1);
        Assert.AreEqual(100, Height(LayoutHarness.FindById(root, "b")!), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#row-relayout")]
    public void PercentageCellHeight_IsResolvedAgainstTheTableHeight()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='height:200px;border-spacing:0'>"
            + "<tr><td id='a' style='height:75%;padding:0'>a</td></tr><tr><td style='padding:0'>b</td></tr></table>"));

        Assert.AreEqual(150, Height(LayoutHarness.FindById(root, "a")!), 1);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#table-cell-content-relayout")]
    public void PercentageHeightInACellWithAHeight_IsResolvedAgainstTheCell()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:0'><tr><td style='height:100px;padding:0;vertical-align:top'><div id='d' style='height:50%'>x</div></td></tr></table>"));

        Assert.AreEqual(50, Height(LayoutHarness.FindById(root, "d")!), 1);
    }

    [TestMethod]
    [Spec("css-tables-3", "#abspos-boxes-in-table-root")]
    public void AbsoluteBox_InAPositionedTable_IsPlacedRelativeToTheTable()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div style='height:10px'></div>"
            + "<table id='t' style='position:relative;margin-left:30px;border-spacing:0'><tr><td style='width:100px;height:50px;padding:0;vertical-align:top'>"
            + "<div id='p' style='position:absolute;top:5px;left:7px;width:10px;height:10px'></div></td></tr></table>"));

        var t = LayoutHarness.FindById(root, "t")!;
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(t.Location.X + 7, p.Location.X, Delta);
        Assert.AreEqual(t.Location.Y + 5, p.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#abspos-boxes-in-table-root")]
    public void AbsoluteBox_InAMiddleAlignedCell_IsNotMovedByTheAlignment()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table id='t' style='position:relative;border-spacing:0'><tr><td style='width:100px;height:50px;padding:0;vertical-align:middle'>"
            + "<div id='p' style='position:absolute;top:5px;left:7px;width:10px;height:10px'></div></td></tr></table>"));

        Assert.AreEqual(LayoutHarness.FindById(root, "t")!.Location.Y + 5, LayoutHarness.FindById(root, "p")!.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-tables-3", "#abspos-boxes-in-table-internal")]
    public void AbsoluteBox_InAPositionedCell_IsPlacedRelativeToTheCell()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:10px'><tr><td style='width:50px'>a</td>"
            + "<td id='c' style='position:relative;width:100px;height:50px;padding:0;vertical-align:top'>"
            + "<div id='p' style='position:absolute;top:5px;left:7px;width:10px;height:10px'></div></td></tr></table>"));

        var c = LayoutHarness.FindById(root, "c")!;
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(c.Location.X + 7, p.Location.X, Delta);
        Assert.AreEqual(c.Location.Y + 5, p.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#abspos-boxes-in-table-internal")]
    public void AbsoluteBox_InAPositionedCell_IsPlacedAgainstItsPaddingEdge()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:10px'><tr><td style='width:50px'>a</td>"
            + "<td id='c' style='position:relative;width:100px;height:50px;padding:4px;border:3px solid;vertical-align:top'>"
            + "<div id='p' style='position:absolute;top:5px;left:7px;width:10px;height:10px'></div></td></tr></table>"));

        var c = LayoutHarness.FindById(root, "c")!;
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(c.Location.X + 3 + 7, p.Location.X, Delta);
        Assert.AreEqual(c.Location.Y + 3 + 5, p.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("css-tables-3", "#static-position")]
    public void AbsoluteBox_WithoutOffsetsInAPositionedCell_StaysAtItsStaticPosition()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='border-spacing:10px'><tr><td style='width:50px'>a</td>"
            + "<td id='c' style='position:relative;width:100px;height:50px;padding:4px;vertical-align:top'>"
            + "<div id='p' style='position:absolute;width:10px;height:10px'></div></td></tr></table>"));

        var c = LayoutHarness.FindById(root, "c")!;
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(c.Location.X + 4, p.Location.X, Delta);
        Assert.AreEqual(c.Location.Y + 4, p.Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#static-position")]
    public void AbsoluteBox_WithoutOffsetsInACell_StaysAtItsStaticPosition()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table style='position:relative;border-spacing:10px'><tr><td style='width:50px'>a</td>"
            + "<td id='c' style='width:100px;height:50px;padding:4px;vertical-align:top'>"
            + "<div id='p' style='position:absolute;width:10px;height:10px'></div></td></tr></table>"));

        var c = LayoutHarness.FindById(root, "c")!;
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(c.Location.X + 4, p.Location.X, Delta);
        Assert.AreEqual(c.Location.Y + 4, p.Location.Y, Delta);
    }

    private static string LongTable(string group) => PaintHarness.Wrap(
        $"<table style='width:100%'>{group}<tbody>"
        + string.Concat(Enumerable.Range(1, 30).Select(i => $"<tr><td style='height:40px'>row{i}</td></tr>"))
        + "</tbody></table>");

    [TestMethod]
    [Spec("css-tables-3", "#repeated-headers")]
    public void Header_IsPaintedAgainOnTheNextPage()
    {
        var (_, container) = PaintHarness.LayoutPaginated(LongTable("<thead style='break-inside:avoid'><tr><td>Head</td></tr></thead>"), 400, 300);

        Assert.IsGreaterThan(1, container.FragmentTree!.Fragmentainers.Count);
        Assert.IsTrue(PaintHarness.PaintPage(container, 1).DrawStringCalls.Any(s => s.Text.Contains("Head")));
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#repeated-headers")]
    public void Footer_IsPaintedOnThePageBeforeTheLast()
    {
        var (_, container) = PaintHarness.LayoutPaginated(LongTable("<tfoot style='break-inside:avoid'><tr><td>Foot</td></tr></tfoot>"), 400, 300);

        Assert.IsGreaterThan(1, container.FragmentTree!.Fragmentainers.Count);
        Assert.IsTrue(PaintHarness.PaintPage(container, 0).DrawStringCalls.Any(s => s.Text.Contains("Foot")));
    }

    [TestMethod]
    [Spec("css-tables-3", "#paint-order")]
    public void Cells_ArePaintedInDocumentOrder()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t'><tr><td rowspan='2' style='background-color:rgb(255,0,0);width:40px'></td><td style='width:40px;height:20px'></td></tr>"
            + "<tr><td style='background-color:rgb(0,0,255);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);
        var rects = g.Log.OfType<RecordingGraphics.DrawRectCall>().ToList();

        var red = rects.FindIndex(r => r.Color == Red);
        var blue = rects.FindIndex(r => r.Color == Blue);
        Assert.IsGreaterThanOrEqualTo(0, red);
        Assert.IsGreaterThan(red, blue);
    }

    [Ignore("Gap, see Spec/css-tables-3.yml")]
    [TestMethod]
    [Spec("css-tables-3", "#empty-cell-rendering")]
    public void EmptyCellsHide_HasNoEffectInCollapsedMode()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t' style='border-collapse:collapse;empty-cells:hide'><tr><td style='background-color:rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(g.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == Red));
    }

    [TestMethod]
    [Spec("css-tables-3", "#drawing-cell-borders")]
    public void CellBorder_IsPaintedInSeparatedMode()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t'><tr><td style='border:2px solid rgb(255,0,0);width:40px;height:20px'></td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsTrue(PaintsColor(g, Red));
    }

    [TestMethod]
    [Spec("css-tables-3", "#visibility-collapse-cell-rendering")]
    public void CollapsedCell_IsRenderedLikeAHiddenOne()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<table id='t'><tr><td id='a' style='width:40px'>a</td><td style='visibility:collapse;background-color:rgb(255,0,0);border:2px solid rgb(255,0,0);width:40px'>hidden</td></tr></table>"));

        var g = PaintHarness.PaintBox(container, PaintHarness.FindById(root, "t")!);

        Assert.IsFalse(PaintsColor(g, Red));
        Assert.IsFalse(g.DrawStringCalls.Any(s => s.Text.Contains("hidden")));
        Assert.IsTrue(g.DrawStringCalls.Any(s => s.Text.Contains('a')));
    }
}
